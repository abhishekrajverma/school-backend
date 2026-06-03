using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Timetable.Application;

public sealed record TimetablePeriodDto(
    string Time,
    string Subject,
    string Teacher,
    string Room);

public sealed record TimetableDto(
    string Id,
    string Class,
    string Day,
    IReadOnlyList<TimetablePeriodDto> Periods);

public sealed record UpsertTimetableRequest(
    string Class,
    string Day,
    IReadOnlyList<TimetablePeriodDto> Periods);

public sealed record ListTimetableQuery(string? ClassName, string? Day) : IRequest<Result<IReadOnlyList<TimetableDto>>>;
public sealed record GetTimetableByIdQuery(string ExternalId) : IRequest<Result<TimetableDto>>;
public sealed record UpsertTimetableCommand(UpsertTimetableRequest Request) : IRequest<Result<TimetableDto>>;
