using System.Text;
using EduSync.Api.Extensions;
using EduSync.Api.Hangfire;
using EduSync.Api.Middleware;
using EduSync.Api.GraphQL;
using EduSync.Api.OpenTelemetry;
using EduSync.Api.SignalR;
using Hangfire;
using Hangfire.Dashboard;
using EduSync.Infrastructure;
using EduSync.Infrastructure.Authorization;
using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Middleware;
using EduSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEduSyncOpenTelemetry(builder.Configuration);
builder.Services.AddEduSyncSignalR(builder.Configuration);
builder.Services.AddEduSyncGraphQL(builder.Configuration);

var hangfireConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;
if (builder.Configuration.GetValue<bool>("ScheduledJobs:UseHangfire"))
{
    builder.Services.AddEduSyncHangfire(hangfireConnection);
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddEduSyncAuthorization();
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddCheck("region", () =>
    {
        var region = builder.Configuration["MultiRegion:CurrentRegion"] ?? "ap-south-1";
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"region:{region}");
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

var capacity = app.Configuration.GetSection("Capacity").Get<CapacityOptions>() ?? new CapacityOptions();
if (capacity.SingleDatabase && app.Configuration.GetValue<bool>("Database:UseReadReplica"))
{
    throw new InvalidOperationException(
        "Capacity:SingleDatabase is true but Database:UseReadReplica is enabled. Disable the read replica for single-database deployments.");
}

app.Logger.LogInformation(
    "Capacity profile: {Schools} schools, {Concurrent}/school concurrent, {ParentDau}/school parent DAU, singleDatabase={SingleDb}",
    capacity.TargetSchools,
    capacity.MaxConcurrentUsersPerSchool,
    capacity.ParentDailyActivePerSchool,
    capacity.SingleDatabase);

await SeedData.InitializeAsync(app.Services);

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiVersionMiddleware>();
app.UseMiddleware<RegionResolutionMiddleware>();
app.UseMiddleware<ChaosMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantRateLimitMiddleware>();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (builder.Configuration.GetValue<bool>("ScheduledJobs:UseHangfire"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment)],
    });
    HangfireServiceExtensions.RegisterFeeReminderRecurringJob(builder.Configuration);
}

var api = app.MapGroup("/api");
api.MapEduSyncEndpoints();

var apiV1 = app.MapGroup("/api/v1");
apiV1.MapEduSyncEndpoints();

app.MapEduSyncSignalR(builder.Configuration);
app.MapEduSyncGraphQL(builder.Configuration);

app.Run();

public partial class Program;
