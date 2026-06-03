using System.Text.Json;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Timetable.Application;
using EduSync.Modules.Timetable.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Timetable;

internal static class TimetableMapping
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static TimetableDto ToDto(TimetableEntry e) => new(
        e.ExternalId, e.ClassName, e.Day, ParsePeriods(e.PeriodsJson));

    public static string SerializePeriods(IReadOnlyList<TimetablePeriodDto> periods) =>
        JsonSerializer.Serialize(periods, Json);

    private static IReadOnlyList<TimetablePeriodDto> ParsePeriods(string json)
    {
        try { return JsonSerializer.Deserialize<List<TimetablePeriodDto>>(json, Json) ?? []; }
        catch { return []; }
    }
}

public sealed class ListTimetableQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListTimetableQuery, Result<IReadOnlyList<TimetableDto>>>
{
    public async Task<Result<IReadOnlyList<TimetableDto>>> Handle(ListTimetableQuery request, CancellationToken ct)
    {
        var query = db.TimetableEntries.AsNoTracking().Where(t => !t.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.ClassName)) query = query.Where(t => t.ClassName == request.ClassName);
        if (!string.IsNullOrWhiteSpace(request.Day)) query = query.Where(t => t.Day == request.Day);
        var items = await query.OrderBy(t => t.Day).ToListAsync(ct);
        return Result<IReadOnlyList<TimetableDto>>.Success(items.Select(TimetableMapping.ToDto).ToList());
    }
}

public sealed class GetTimetableByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetTimetableByIdQuery, Result<TimetableDto>>
{
    public async Task<Result<TimetableDto>> Handle(GetTimetableByIdQuery request, CancellationToken ct)
    {
        var e = await db.TimetableEntries.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == request.ExternalId && !t.IsDeleted, ct);
        return e is null ? Result<TimetableDto>.Failure(Error.NotFound("Timetable entry not found."))
            : Result<TimetableDto>.Success(TimetableMapping.ToDto(e));
    }
}

public sealed class UpsertTimetableCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<UpsertTimetableCommand, Result<TimetableDto>>
{
    public async Task<Result<TimetableDto>> Handle(UpsertTimetableCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<TimetableDto>.Failure(Error.Forbidden("Tenant required."));
        var body = request.Request;
        var existing = await db.TimetableEntries.FirstOrDefaultAsync(t =>
            t.TenantId == tenant.TenantId && t.ClassName == body.Class && t.Day == body.Day && !t.IsDeleted, ct);

        if (existing is not null)
        {
            existing.PeriodsJson = TimetableMapping.SerializePeriods(body.Periods);
            await db.SaveChangesAsync(ct);
            return Result<TimetableDto>.Success(TimetableMapping.ToDto(existing));
        }

        var entry = new TimetableEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            ClassName = body.Class,
            Day = body.Day,
            PeriodsJson = TimetableMapping.SerializePeriods(body.Periods),
        };
        db.TimetableEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return Result<TimetableDto>.Success(TimetableMapping.ToDto(entry));
    }
}
