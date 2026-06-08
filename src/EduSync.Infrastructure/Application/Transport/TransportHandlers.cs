using System.Text.Json;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Transport.Application;
using EduSync.Modules.Transport.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Transport;

internal static class TransportMapping
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static VehicleDto ToDto(Vehicle v) => new(
        v.ExternalId, v.VehicleNumber, v.VehicleType, v.Capacity,
        v.DriverName, v.DriverPhone, v.DriverLicense, v.RouteExternalId, v.RouteName,
        v.InsuranceExpiry.ToString("yyyy-MM-dd"), v.FitnessExpiry.ToString("yyyy-MM-dd"),
        v.CurrentStudents, v.Status, v.GpsStatus, v.LastLocation);

    public static TransportRouteDto ToDto(TransportRoute r) => new(
        r.ExternalId, r.RouteName, r.VehicleExternalId, r.VehicleNumber, r.DriverName,
        r.StartPoint, r.EndPoint, r.TotalStops, r.TotalStudents, r.Fare,
        r.MorningTime, r.EveningTime, r.Status, r.Distance, ParseStops(r.StopsJson));

    public static IReadOnlyList<RouteStopDto>? ParseStops(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<RouteStopDto>>(json, Json); }
        catch { return null; }
    }

    public static string? SerializeStops(IReadOnlyList<RouteStopDto>? stops) =>
        stops is null || stops.Count == 0 ? null : JsonSerializer.Serialize(stops, Json);
}

public sealed class ListVehiclesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListVehiclesQuery, Result<PaginatedList<VehicleDto>>>
{
    public async Task<Result<PaginatedList<VehicleDto>>> Handle(ListVehiclesQuery request, CancellationToken ct)
    {
        var query = db.Vehicles.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        query = query.OrderBy(x => x.VehicleNumber);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(TransportMapping.ToDto).ToList();
        return Result<PaginatedList<VehicleDto>>.Success(
            PaginatedList<VehicleDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetVehicleByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDto>>
{
    public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken ct)
    {
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return v is null ? Result<VehicleDto>.Failure(Error.NotFound("Vehicle not found."))
            : Result<VehicleDto>.Success(TransportMapping.ToDto(v));
    }
}

public sealed class CreateVehicleCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
{
    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<VehicleDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.InsuranceExpiry, out var ins) || !DateOnly.TryParse(b.FitnessExpiry, out var fit))
            return Result<VehicleDto>.Failure(Error.Validation("Invalid expiry dates."));
        string? routeName = null;
        if (!string.IsNullOrWhiteSpace(b.RouteId))
        {
            var route = await db.TransportRoutes.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ExternalId == b.RouteId, ct);
            routeName = route?.RouteName;
        }
        var v = new Vehicle
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            VehicleNumber = b.VehicleNumber, VehicleType = b.VehicleType, Capacity = b.Capacity,
            DriverName = b.DriverName, DriverPhone = b.DriverPhone, DriverLicense = b.DriverLicense,
            RouteExternalId = b.RouteId, RouteName = routeName,
            InsuranceExpiry = ins, FitnessExpiry = fit, Status = b.Status, GpsStatus = "offline",
        };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync(ct);
        return Result<VehicleDto>.Success(TransportMapping.ToDto(v));
    }
}

public sealed class ListRoutesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListRoutesQuery, Result<PaginatedList<TransportRouteDto>>>
{
    public async Task<Result<PaginatedList<TransportRouteDto>>> Handle(ListRoutesQuery request, CancellationToken ct)
    {
        var query = db.TransportRoutes.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        query = query.OrderBy(x => x.RouteName);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(TransportMapping.ToDto).ToList();
        return Result<PaginatedList<TransportRouteDto>>.Success(
            PaginatedList<TransportRouteDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetRouteByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetRouteByIdQuery, Result<TransportRouteDto>>
{
    public async Task<Result<TransportRouteDto>> Handle(GetRouteByIdQuery request, CancellationToken ct)
    {
        var r = await db.TransportRoutes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return r is null ? Result<TransportRouteDto>.Failure(Error.NotFound("Route not found."))
            : Result<TransportRouteDto>.Success(TransportMapping.ToDto(r));
    }
}

public sealed class CreateRouteCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateRouteCommand, Result<TransportRouteDto>>
{
    public async Task<Result<TransportRouteDto>> Handle(CreateRouteCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<TransportRouteDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        string? vehicleNumber = null;
        if (!string.IsNullOrWhiteSpace(b.VehicleId))
        {
            var vehicle = await db.Vehicles.AsNoTracking()
                .FirstOrDefaultAsync(v => v.ExternalId == b.VehicleId, ct);
            vehicleNumber = vehicle?.VehicleNumber;
        }
        var stops = b.Stops ?? [];
        var route = new TransportRoute
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            RouteName = b.RouteName, VehicleExternalId = b.VehicleId, VehicleNumber = vehicleNumber,
            DriverName = b.DriverName, StartPoint = b.StartPoint, EndPoint = b.EndPoint,
            TotalStops = stops.Count, Fare = b.Fare, MorningTime = b.MorningTime, EveningTime = b.EveningTime,
            Status = b.Status, Distance = b.Distance, StopsJson = TransportMapping.SerializeStops(stops),
        };
        db.TransportRoutes.Add(route);
        await db.SaveChangesAsync(ct);
        return Result<TransportRouteDto>.Success(TransportMapping.ToDto(route));
    }
}

internal static class AssignmentMapping
{
    public static TransportAssignmentDto ToDto(TransportAssignment a) => new(
        a.ExternalId, a.StudentExternalId, a.StudentName, a.RouteExternalId,
        a.PickupStopOrder, a.Shift, a.EnrolledSince.ToString("yyyy-MM-dd"), a.Status, a.SeatNumber);
}

public sealed class ListTransportAssignmentsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListTransportAssignmentsQuery, Result<PaginatedList<TransportAssignmentDto>>>
{
    public async Task<Result<PaginatedList<TransportAssignmentDto>>> Handle(ListTransportAssignmentsQuery request, CancellationToken ct)
    {
        var query = db.TransportAssignments.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.RouteId)) query = query.Where(x => x.RouteExternalId == request.RouteId);
        if (!string.IsNullOrWhiteSpace(request.StudentId)) query = query.Where(x => x.StudentExternalId == request.StudentId);
        query = query.OrderByDescending(x => x.EnrolledSince);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(AssignmentMapping.ToDto).ToList();
        return Result<PaginatedList<TransportAssignmentDto>>.Success(
            PaginatedList<TransportAssignmentDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetTransportAssignmentByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetTransportAssignmentByIdQuery, Result<TransportAssignmentDto>>
{
    public async Task<Result<TransportAssignmentDto>> Handle(GetTransportAssignmentByIdQuery request, CancellationToken ct)
    {
        var a = await db.TransportAssignments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return a is null ? Result<TransportAssignmentDto>.Failure(Error.NotFound("Assignment not found."))
            : Result<TransportAssignmentDto>.Success(AssignmentMapping.ToDto(a));
    }
}

public sealed class CreateTransportAssignmentCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateTransportAssignmentCommand, Result<TransportAssignmentDto>>
{
    public async Task<Result<TransportAssignmentDto>> Handle(CreateTransportAssignmentCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<TransportAssignmentDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.EnrolledSince, out var enrolled))
            return Result<TransportAssignmentDto>.Failure(Error.Validation("Invalid enrolled date."));
        var route = await db.TransportRoutes
            .FirstOrDefaultAsync(r => r.ExternalId == b.RouteId && !r.IsDeleted, ct);
        if (route is null) return Result<TransportAssignmentDto>.Failure(Error.NotFound("Route not found."));

        var assignment = new TransportAssignment
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            StudentExternalId = b.StudentId, StudentName = b.StudentName,
            RouteExternalId = b.RouteId, PickupStopOrder = b.PickupStopOrder,
            Shift = b.Shift, EnrolledSince = enrolled, Status = b.Status, SeatNumber = b.SeatNumber,
        };
        db.TransportAssignments.Add(assignment);
        route.TotalStudents++;
        var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.RouteExternalId == b.RouteId, ct);
        if (vehicle is not null) vehicle.CurrentStudents++;
        await db.SaveChangesAsync(ct);
        return Result<TransportAssignmentDto>.Success(AssignmentMapping.ToDto(assignment));
    }
}

public sealed class UpdateVehicleCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateVehicleCommand, Result<VehicleDto>>
{
    public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken ct)
    {
        var v = await db.Vehicles.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (v is null) return Result<VehicleDto>.Failure(Error.NotFound("Vehicle not found."));
        var b = request.Request;
        if (b.VehicleNumber is not null) v.VehicleNumber = b.VehicleNumber;
        if (b.VehicleType is not null) v.VehicleType = b.VehicleType;
        if (b.Capacity.HasValue) v.Capacity = b.Capacity.Value;
        if (b.DriverName is not null) v.DriverName = b.DriverName;
        if (b.DriverPhone is not null) v.DriverPhone = b.DriverPhone;
        if (b.DriverLicense is not null) v.DriverLicense = b.DriverLicense;
        if (b.RouteId is not null)
        {
            v.RouteExternalId = b.RouteId;
            var route = await db.TransportRoutes.AsNoTracking().FirstOrDefaultAsync(r => r.ExternalId == b.RouteId, ct);
            v.RouteName = route?.RouteName;
        }
        if (b.InsuranceExpiry is not null && DateOnly.TryParse(b.InsuranceExpiry, out var ins)) v.InsuranceExpiry = ins;
        if (b.FitnessExpiry is not null && DateOnly.TryParse(b.FitnessExpiry, out var fit)) v.FitnessExpiry = fit;
        if (b.Status is not null) v.Status = b.Status;
        await db.SaveChangesAsync(ct);
        return Result<VehicleDto>.Success(TransportMapping.ToDto(v));
    }
}

public sealed class DeleteVehicleCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteVehicleCommand, Result>
{
    public async Task<Result> Handle(DeleteVehicleCommand request, CancellationToken ct)
    {
        var v = await db.Vehicles.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (v is null) return Result.Failure(Error.NotFound("Vehicle not found."));
        v.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class UpdateRouteCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateRouteCommand, Result<TransportRouteDto>>
{
    public async Task<Result<TransportRouteDto>> Handle(UpdateRouteCommand request, CancellationToken ct)
    {
        var route = await db.TransportRoutes.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (route is null) return Result<TransportRouteDto>.Failure(Error.NotFound("Route not found."));
        var b = request.Request;
        if (b.RouteName is not null) route.RouteName = b.RouteName;
        if (b.VehicleId is not null)
        {
            route.VehicleExternalId = b.VehicleId;
            var vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.ExternalId == b.VehicleId, ct);
            route.VehicleNumber = vehicle?.VehicleNumber;
        }
        if (b.DriverName is not null) route.DriverName = b.DriverName;
        if (b.StartPoint is not null) route.StartPoint = b.StartPoint;
        if (b.EndPoint is not null) route.EndPoint = b.EndPoint;
        if (b.Fare.HasValue) route.Fare = b.Fare.Value;
        if (b.MorningTime is not null) route.MorningTime = b.MorningTime;
        if (b.EveningTime is not null) route.EveningTime = b.EveningTime;
        if (b.Distance is not null) route.Distance = b.Distance;
        if (b.Status is not null) route.Status = b.Status;
        if (b.Stops is not null)
        {
            route.StopsJson = TransportMapping.SerializeStops(b.Stops);
            route.TotalStops = b.Stops.Count;
        }
        await db.SaveChangesAsync(ct);
        return Result<TransportRouteDto>.Success(TransportMapping.ToDto(route));
    }
}

public sealed class DeleteRouteCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteRouteCommand, Result>
{
    public async Task<Result> Handle(DeleteRouteCommand request, CancellationToken ct)
    {
        var route = await db.TransportRoutes.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (route is null) return Result.Failure(Error.NotFound("Route not found."));
        route.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
