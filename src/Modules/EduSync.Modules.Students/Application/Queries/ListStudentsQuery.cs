using EduSync.Modules.Students.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Students.Application.Queries;

public sealed record ListStudentsQuery(PaginationQuery Pagination)
    : IRequest<Result<PaginatedList<StudentDto>>>;
