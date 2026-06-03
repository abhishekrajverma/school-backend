using System.Net.Http.Headers;
using System.Net.Http.Json;
using EduSync.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace EduSync.IntegrationTests;

public sealed class TenantIsolationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Your_strong_password123!")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:DefaultConnection", _sql.GetConnectionString());
                builder.UseSetting("Redis:Enabled", "false");
                builder.UseSetting("SignalR:Enabled", "false");
                builder.UseSetting("ScheduledJobs:UseHangfire", "false");
                builder.UseSetting("Outbox:Enabled", "false");
                builder.UseSetting("OpenTelemetry:Enabled", "false");
                builder.UseSetting("Chaos:Enabled", "false");
                builder.UseSetting("Audit:Enabled", "false");
                builder.UseSetting("Retention:Enabled", "false");
                builder.UseSetting("GraphQL:Enabled", "false");
            });

        _client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        await db.Database.MigrateAsync();
        await SeedData.InitializeAsync(_factory.Services);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _sql.DisposeAsync();
    }

    [Fact]
    public async Task Student_role_cannot_list_students()
    {
        var login = await _client!.PostAsJsonAsync("/api/auth/login", new
        {
            email = "arjun.s@school.edu",
            password = "student123",
        });
        login.EnsureSuccessStatusCode();
        var tokens = await login.Content.ReadFromJsonAsync<LoginPayload>();
        tokens.Should().NotBeNull();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/students");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        request.Headers.Add("X-Tenant-Id", "demo-school-001");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TenantA_user_cannot_read_TenantB_student()
    {
        var login = await _client!.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@school.edu",
            password = "admin123",
        });
        login.EnsureSuccessStatusCode();
        var tokens = await login.Content.ReadFromJsonAsync<LoginPayload>();
        tokens.Should().NotBeNull();

        var otherTenantRequest = new HttpRequestMessage(HttpMethod.Get, "/api/students/1");
        otherTenantRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        otherTenantRequest.Headers.Add("X-Tenant-Id", "non-existent-tenant");

        var response = await _client.SendAsync(otherTenantRequest);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    private sealed record LoginPayload(string AccessToken, string? RefreshToken, int ExpiresIn);
}
