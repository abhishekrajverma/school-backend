using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Transport.Application;

public sealed record RouteStopDto(string Name, string Time, int Order);

public sealed record VehicleDto(
    string Id, string VehicleNumber, string VehicleType, int Capacity,
    string DriverName, string DriverPhone, string DriverLicense,
    string? RouteId, string? RouteName, string InsuranceExpiry, string FitnessExpiry,
    int CurrentStudents, string Status, string GpsStatus, string? LastLocation);

public sealed record TransportRouteDto(
    string Id, string RouteName, string? VehicleId, string? VehicleNumber, string DriverName,
    string StartPoint, string EndPoint, int TotalStops, int TotalStudents, decimal Fare,
    string MorningTime, string EveningTime, string Status, string? Distance,
    IReadOnlyList<RouteStopDto>? Stops);

public sealed record CreateVehicleRequest(
    string VehicleNumber, string VehicleType, int Capacity,
    string DriverName, string DriverPhone, string DriverLicense,
    string? RouteId, string InsuranceExpiry, string FitnessExpiry, string Status);

public sealed record CreateRouteRequest(
    string RouteName, string? VehicleId, string DriverName, string StartPoint, string EndPoint,
    decimal Fare, string MorningTime, string EveningTime, string? Distance,
    IReadOnlyList<RouteStopDto>? Stops, string Status);

public sealed record ListVehiclesQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<VehicleDto>>>;

public sealed record GetVehicleByIdQuery(string ExternalId) : IRequest<Result<VehicleDto>>;
public sealed record CreateVehicleCommand(CreateVehicleRequest Request) : IRequest<Result<VehicleDto>>;

public sealed record ListRoutesQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<TransportRouteDto>>>;

public sealed record GetRouteByIdQuery(string ExternalId) : IRequest<Result<TransportRouteDto>>;
public sealed record CreateRouteCommand(CreateRouteRequest Request) : IRequest<Result<TransportRouteDto>>;

public sealed record TransportAssignmentDto(
    string Id,
    string StudentId,
    string StudentName,
    string RouteId,
    int PickupStopOrder,
    string Shift,
    string EnrolledSince,
    string Status,
    string? SeatNumber);

public sealed record CreateTransportAssignmentRequest(
    string StudentId,
    string StudentName,
    string RouteId,
    int PickupStopOrder,
    string Shift,
    string EnrolledSince,
    string? SeatNumber,
    string Status);

public sealed record ListTransportAssignmentsQuery(PaginationQuery Pagination, string? RouteId, string? StudentId)
    : IRequest<Result<PaginatedList<TransportAssignmentDto>>>;

public sealed record GetTransportAssignmentByIdQuery(string ExternalId) : IRequest<Result<TransportAssignmentDto>>;
public sealed record CreateTransportAssignmentCommand(CreateTransportAssignmentRequest Request)
    : IRequest<Result<TransportAssignmentDto>>;
