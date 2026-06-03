using EduSync.Infrastructure.Application;
using EduSync.Infrastructure.Jobs;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Storage;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.Configure<UploadOptions>(configuration.GetSection("Uploads"));
        services.Configure<ScheduledJobsOptions>(configuration.GetSection("ScheduledJobs"));
        services.AddSingleton<LocalFileStorageService>();
        services.AddSingleton<AzureBlobFileStorageService>();
        services.AddSingleton<S3CompatibleFileStorageService>();
        services.AddSingleton<IFileStorageService>(sp =>
        {
            var provider = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UploadOptions>>().Value.Provider;
            if (provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<AzureBlobFileStorageService>();
            }

            if (provider.Equals("S3", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<S3CompatibleFileStorageService>();
            }

            return sp.GetRequiredService<LocalFileStorageService>();
        });

        services.AddEduSyncPhase7(configuration);
        services.AddEduSyncPhase8(configuration);
        services.AddEduSyncPhase9(configuration);
        services.AddEduSyncPhase10(configuration);
        services.AddScoped<IFeeReminderJob, FeeReminderJob>();
        services.AddScoped<IFeeReminderScheduler, FeeReminderScheduler>();
        services.AddScoped<Reports.IReportExporter, Reports.ReportExporter>();
        services.AddHostedService<FeeReminderBackgroundService>();

        var commandTimeout = configuration.GetSection("Database").GetValue<int?>("CommandTimeoutSeconds") ?? 30;
        services.AddDbContext<EduSyncDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(EduSyncDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                    sql.CommandTimeout(commandTimeout);
                }));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Identity.Application.Commands.LoginCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Tenancy.Application.Commands.ProvisionTenantCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Students.Application.Commands.CreateStudentCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Staff.Application.CreateTeacherCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Parents.Application.CreateParentCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Academics.Application.CreateClassCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Admissions.Application.CreateAdmissionCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Attendance.Application.ListAttendanceQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Fees.Application.ListFeesQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Exams.Application.ListExamsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Timetable.Application.ListTimetableQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Notifications.Application.ListNotificationsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Payroll.Application.ListPayrollQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Leave.Application.ListLeaveRequestsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Library.Application.ListBooksQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Transport.Application.ListVehiclesQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Hostel.Application.ListHostelRoomsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Inventory.Application.ListInventoryItemsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Dashboard.Application.GetDashboardQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Portals.Application.GetStudentPortalProfileQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Uploads.Application.UploadFileCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Jobs.Application.ListJobRunsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Imports.Application.ImportStudentsCsvCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Events.Application.ListOutboxMessagesQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Audit.Application.ListAuditLogsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Webhooks.Application.ListWebhookSubscriptionsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Compliance.Application.ListRetentionPoliciesQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Modules.Identity.Application.Commands.OidcLoginCommand).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(Modules.Identity.Application.Commands.LoginCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Tenancy.Application.Commands.ProvisionTenantCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Students.Application.Commands.CreateStudentCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Staff.Application.CreateTeacherCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Parents.Application.CreateParentCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Academics.Application.CreateClassCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Admissions.Application.CreateAdmissionCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Attendance.Application.ListAttendanceQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Fees.Application.ListFeesQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Exams.Application.ListExamsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Timetable.Application.ListTimetableQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Notifications.Application.ListNotificationsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Payroll.Application.ListPayrollQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Leave.Application.ListLeaveRequestsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Library.Application.ListBooksQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Transport.Application.ListVehiclesQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Hostel.Application.ListHostelRoomsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Inventory.Application.ListInventoryItemsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Dashboard.Application.GetDashboardQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Portals.Application.GetStudentPortalProfileQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Uploads.Application.UploadFileCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Jobs.Application.ListJobRunsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Imports.Application.ImportStudentsCsvCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Events.Application.ListOutboxMessagesQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Audit.Application.ListAuditLogsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Webhooks.Application.ListWebhookSubscriptionsQuery).Assembly);
        services.AddValidatorsFromAssembly(typeof(Modules.Compliance.Application.ListRetentionPoliciesQuery).Assembly);

        return services;
    }
}
