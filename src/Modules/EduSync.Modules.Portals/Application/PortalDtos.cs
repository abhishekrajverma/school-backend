using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Portals.Application;

public sealed record GetStudentPortalProfileQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalFeesQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalAttendanceQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalExamsQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalAssignmentsQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalTimetableQuery : IRequest<Result<object>>;
public sealed record GetStudentPortalLibraryQuery : IRequest<Result<object>>;

public sealed record GetTeacherPortalProfileQuery : IRequest<Result<object>>;
public sealed record GetTeacherPortalLeavesQuery : IRequest<Result<object>>;
public sealed record GetTeacherPortalPayrollQuery : IRequest<Result<object>>;
public sealed record GetTeacherPortalTimetableQuery : IRequest<Result<object>>;

public sealed record GetParentPortalProfileQuery : IRequest<Result<object>>;
public sealed record GetParentPortalChildrenQuery : IRequest<Result<object>>;
public sealed record GetParentPortalChildFeesQuery(string ChildId) : IRequest<Result<object>>;
public sealed record GetParentPortalChildAttendanceQuery(string ChildId) : IRequest<Result<object>>;
public sealed record GetParentPortalChildTransportQuery(string ChildId) : IRequest<Result<object>>;
