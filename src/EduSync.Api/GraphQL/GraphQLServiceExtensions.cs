namespace EduSync.Api.GraphQL;

public static class GraphQLServiceExtensions
{
    public static IServiceCollection AddEduSyncGraphQL(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue("GraphQL:Enabled", true))
        {
            return services;
        }

        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<EduSyncQuery>()
            .ModifyRequestOptions(options => options.IncludeExceptionDetails = false);

        return services;
    }

    public static WebApplication MapEduSyncGraphQL(this WebApplication app, IConfiguration configuration)
    {
        if (!configuration.GetValue("GraphQL:Enabled", true))
        {
            return app;
        }

        app.MapGraphQL("/graphql");
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/graphql-ui", () => Results.Redirect("/graphql"));
        }

        return app;
    }
}
