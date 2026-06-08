using EduSync.Api.Extensions;

using EduSync.Infrastructure.Persistence;

using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Tenancy.Domain;

using EduSync.Modules.Identity.Authorization;

using Microsoft.EntityFrameworkCore;

using EduSync.Modules.Tenancy.Application.Commands;

using EduSync.Modules.Tenancy.Application.Dtos;

using EduSync.Modules.Tenancy.Application.Queries;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class TenantEndpoints

{

    public static RouteGroupBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/tenants").WithTags("Tenancy");



        group.MapPost("/provision", async (ProvisionTenantRequest body, ISender sender) =>

        {

            var result = await sender.Send(new ProvisionTenantCommand(

                body.SchoolName,

                body.Slug,

                body.AdminEmail,

                body.AdminPassword,

                body.AdminName,

                body.PlanId));

            return result.ToHttpResult();

        }).AllowAnonymous();



        group.MapGet("/by-slug/{slug}", async (string slug, ISender sender) =>

        {

            var result = await sender.Send(new GetTenantBySlugQuery(slug));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).AllowAnonymous();



        group.MapGet("/current", async (ITenantContext tenantContext, ISender sender) =>

        {

            if (!tenantContext.TenantId.HasValue)

            {

                return Results.Json(

                    new { code = "FORBIDDEN", message = "Tenant context is required." },

                    statusCode: StatusCodes.Status403Forbidden);

            }



            var result = await sender.Send(new GetCurrentTenantQuery(tenantContext.TenantId.Value));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TenantsRead);



        group.MapGet("/{id}", async (string id, EduSyncDbContext db) =>

        {

            var tenant = await db.Tenants.AsNoTracking()
                .Include(t => t.Subscription)
                .FirstOrDefaultAsync(t => t.ExternalId == id || t.Slug == id);

            if (tenant is null)

            {

                return Results.Json(

                    new { code = "NOT_FOUND", message = "Tenant not found." },

                    statusCode: StatusCodes.Status404NotFound);

            }



            return Results.Ok(new

            {

                id = tenant.ExternalId,

                slug = tenant.Slug,

                name = tenant.Name,

                logoUrl = tenant.LogoUrl,

                status = TenantStatusMapper.ToApiStatus(tenant.Status),
                schoolEmail = tenant.SchoolEmail,
                planKey = tenant.Subscription?.PlanId,

            });

        }).AllowAnonymous();



        return group;

    }

}


