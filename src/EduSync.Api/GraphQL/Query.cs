using EduSync.Modules.Dashboard.Application;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Application.Queries;
using EduSync.SharedKernel.Pagination;
using HotChocolate.Authorization;
using MediatR;

namespace EduSync.Api.GraphQL;

[QueryType]
[Authorize]
public class EduSyncQuery
{
    [Authorize(Policy = Permissions.StudentsRead)]
    public async Task<IReadOnlyList<StudentDto>> GetStudents(
        int page,
        int pageSize,
        string? search,
        ISender sender,
        CancellationToken cancellationToken)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        var result = await sender.Send(
            new ListStudentsQuery(PaginationQuery.FromHttp(page, pageSize, search, null, null)),
            cancellationToken);
        return result.IsSuccess ? result.Value!.Items : [];
    }

    [Authorize(Policy = Permissions.StudentsRead)]
    public async Task<StudentDto?> GetStudentById(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStudentByIdQuery(id), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = Permissions.DashboardRead)]
    public async Task<DashboardResponseDto?> GetDashboard(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDashboardQuery(), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }
}
