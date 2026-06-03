using EduSync.Modules.Staff.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Staff.Application;

public sealed record ListTeachersQuery(PaginationQuery Pagination) : IRequest<Result<PaginatedList<TeacherDto>>>;
public sealed record GetTeacherByIdQuery(string ExternalId) : IRequest<Result<TeacherDto>>;
public sealed record CreateTeacherCommand(CreateTeacherRequest Request) : IRequest<Result<TeacherDto>>;
public sealed record UpdateTeacherCommand(string ExternalId, UpdateTeacherRequest Request) : IRequest<Result<TeacherDto>>;
public sealed record DeleteTeacherCommand(string ExternalId) : IRequest<Result>;
