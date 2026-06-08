using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Fees.Application;

public sealed record FeeLineItemDto(string FeeType, decimal Amount, decimal LineDiscount);

public sealed record FeeRecordDto(
    string Id,
    string InvoiceNo,
    string StudentId,
    string StudentName,
    string Class,
    string FeeType,
    decimal TotalFee,
    decimal Paid,
    decimal Pending,
    decimal Discount,
    decimal Fine,
    string DueDate,
    string? PaidDate,
    string Status,
    string? PaymentMethod,
    IReadOnlyList<FeeLineItemDto>? FeeItems);

public sealed record CreateFeeRequest(
    string StudentId,
    string StudentName,
    string Class,
    string FeeType,
    decimal TotalFee,
    decimal Discount,
    decimal Fine,
    string DueDate,
    IReadOnlyList<FeeLineItemDto>? FeeItems);

public sealed record RecordPaymentRequest(
    decimal Amount,
    string PaymentMethod,
    string? TransactionId,
    string? Remarks);

public sealed record PaymentDto(
    string Id,
    string FeeId,
    string StudentId,
    decimal Amount,
    string PaymentMethod,
    string? TransactionId,
    string? Remarks,
    DateTime PaidAt);

public sealed record ListFeesQuery(PaginationQuery Pagination, string? Status, string? StudentId)
    : IRequest<Result<PaginatedList<FeeRecordDto>>>;

public sealed record GetFeeByIdQuery(string ExternalId) : IRequest<Result<FeeRecordDto>>;
public sealed record UpdateFeeRequest(
    string? StudentName,
    string? Class,
    string? FeeType,
    decimal? TotalFee,
    decimal? Discount,
    decimal? Fine,
    string? DueDate,
    string? Status,
    IReadOnlyList<FeeLineItemDto>? FeeItems);

public sealed record CreateFeeCommand(CreateFeeRequest Request) : IRequest<Result<FeeRecordDto>>;
public sealed record UpdateFeeCommand(string ExternalId, UpdateFeeRequest Request) : IRequest<Result<FeeRecordDto>>;
public sealed record DeleteFeeCommand(string ExternalId) : IRequest<Result>;
public sealed record RecordPaymentCommand(string FeeExternalId, RecordPaymentRequest Request) : IRequest<Result<PaymentDto>>;
public sealed record ListPaymentsQuery(PaginationQuery Pagination, string? StudentId)
    : IRequest<Result<PaginatedList<PaymentDto>>>;
