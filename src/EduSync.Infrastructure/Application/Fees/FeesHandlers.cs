using System.Text.Json;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Events.Domain;
using Microsoft.AspNetCore.Http;using EduSync.Modules.Fees.Application;
using EduSync.Modules.Fees.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Fees;

internal static class FeeMapping
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static FeeRecordDto ToDto(FeeInvoice f) => new(
        f.ExternalId, f.InvoiceNo, f.StudentExternalId, f.StudentName, f.ClassName, f.FeeType,
        f.TotalFee, f.Paid, f.Pending, f.Discount, f.Fine,
        f.DueDate.ToString("yyyy-MM-dd"), f.PaidDate?.ToString("yyyy-MM-dd"), f.Status, f.PaymentMethod,
        ParseFeeItems(f.FeeItemsJson));

    public static PaymentDto ToPaymentDto(FeePayment p) => new(
        p.ExternalId, p.Invoice.ExternalId, p.StudentExternalId, p.Amount,
        p.PaymentMethod, p.TransactionId, p.Remarks, p.PaidAt);

    private static IReadOnlyList<FeeLineItemDto>? ParseFeeItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<FeeLineItemDto>>(json, Json); }
        catch { return null; }
    }

    public static void RecalculateStatus(FeeInvoice f)
    {
        f.Pending = Math.Max(0, f.TotalFee - f.Discount + f.Fine - f.Paid);
        if (f.Pending <= 0) { f.Status = "paid"; f.PaidDate ??= DateOnly.FromDateTime(DateTime.UtcNow); }
        else if (f.DueDate < DateOnly.FromDateTime(DateTime.UtcNow)) f.Status = "overdue";
        else f.Status = "pending";
    }
}

public sealed class ListFeesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListFeesQuery, Result<PaginatedList<FeeRecordDto>>>
{
    public async Task<Result<PaginatedList<FeeRecordDto>>> Handle(ListFeesQuery request, CancellationToken ct)
    {
        var query = db.FeeInvoices.AsNoTracking().Where(f => !f.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(f => f.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.StudentId)) query = query.Where(f => f.StudentExternalId == request.StudentId);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(f => f.StudentName.ToLower().Contains(term) || f.InvoiceNo.Contains(term));
        }

        query = query.OrderByDescending(f => f.DueDate);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(FeeMapping.ToDto).ToList();
        return Result<PaginatedList<FeeRecordDto>>.Success(
            PaginatedList<FeeRecordDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetFeeByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetFeeByIdQuery, Result<FeeRecordDto>>
{
    public async Task<Result<FeeRecordDto>> Handle(GetFeeByIdQuery request, CancellationToken ct)
    {
        var f = await db.FeeInvoices.AsNoTracking().FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return f is null ? Result<FeeRecordDto>.Failure(Error.NotFound("Fee not found."))
            : Result<FeeRecordDto>.Success(FeeMapping.ToDto(f));
    }
}

public sealed class CreateFeeCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateFeeCommand, Result<FeeRecordDto>>
{
    public async Task<Result<FeeRecordDto>> Handle(CreateFeeCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<FeeRecordDto>.Failure(Error.Forbidden("Tenant required."));
        var body = request.Request;
        if (!DateOnly.TryParse(body.DueDate, out var due)) return Result<FeeRecordDto>.Failure(Error.Validation("Invalid due date."));

        var invoice = new FeeInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            InvoiceNo = $"INV{DateTime.UtcNow:yyyy}{Random.Shared.Next(100000, 999999)}",
            StudentExternalId = body.StudentId,
            StudentName = body.StudentName,
            ClassName = body.Class,
            FeeType = body.FeeType,
            TotalFee = body.TotalFee,
            Paid = 0,
            Discount = body.Discount,
            Fine = body.Fine,
            DueDate = due,
            FeeItemsJson = body.FeeItems is null ? null : JsonSerializer.Serialize(body.FeeItems),
        };
        FeeMapping.RecalculateStatus(invoice);
        db.FeeInvoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        return Result<FeeRecordDto>.Success(FeeMapping.ToDto(invoice));
    }
}

public sealed class RecordPaymentCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IIntegrationEventCollector events,
    IRegionContext region,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<RecordPaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<PaymentDto>.Failure(Error.Forbidden("Tenant required."));
        var invoice = await db.FeeInvoices.FirstOrDefaultAsync(
            f => f.ExternalId == request.FeeExternalId && !f.IsDeleted, ct);
        if (invoice is null) return Result<PaymentDto>.Failure(Error.NotFound("Fee not found."));

        var body = request.Request;
        var payment = new FeePayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FeeInvoiceId = invoice.Id,
            StudentExternalId = invoice.StudentExternalId,
            Amount = body.Amount,
            PaymentMethod = body.PaymentMethod,
            TransactionId = body.TransactionId,
            Remarks = body.Remarks,
            PaidAt = DateTime.UtcNow,
            Invoice = invoice,
        };
        invoice.Paid += body.Amount;
        invoice.PaymentMethod = body.PaymentMethod;
        FeeMapping.RecalculateStatus(invoice);
        db.FeePayments.Add(payment);
        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.FeePaymentRecorded,
            new { paymentId = payment.ExternalId, feeId = invoice.ExternalId, payment.Amount },
            tenant,
            region,
            httpContextAccessor));
        await db.SaveChangesAsync(ct);
        return Result<PaymentDto>.Success(FeeMapping.ToPaymentDto(payment));
    }
}

public sealed class ListPaymentsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListPaymentsQuery, Result<PaginatedList<PaymentDto>>>
{
    public async Task<Result<PaginatedList<PaymentDto>>> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        var query = db.FeePayments.AsNoTracking().Include(p => p.Invoice).Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.StudentId)) query = query.Where(p => p.StudentExternalId == request.StudentId);
        query = query.OrderByDescending(p => p.PaidAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(FeeMapping.ToPaymentDto).ToList();
        return Result<PaginatedList<PaymentDto>>.Success(
            PaginatedList<PaymentDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}
