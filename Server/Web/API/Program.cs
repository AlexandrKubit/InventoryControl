namespace API;
using Common.DI;
using Web.API.Middleware;

public static class Program
{
    public static void Main(string[] args)
    {
        InitializeApplication();

        var builder = WebApplication.CreateBuilder(args);
        RegisterServices(builder);

        var app = builder.Build();
        Configure(app);

        app.Run();
    }

    private static void InitializeApplication()
    {
        Initializer.InitializeMediator();
        Initializer.InitializeDomain();
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.AddQueries();
        builder.AddInfrastructure();
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.ToString());
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                .SetIsOriginAllowed(x => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });
    }

    private static void Configure(WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseCors("AllowAll");
        app.UseMediatorEndpoints();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
