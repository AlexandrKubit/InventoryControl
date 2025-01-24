namespace API;
using Common.DI;
using Web.API.Middleware;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        RegisterServices(builder);

        var app = builder.Build();
        Configure(app);

        app.Run();
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMediator();
        builder.Services.AddInfrastructure();
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.ToString());
        });
    }

    private static void Configure(WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseDomain();
        app.UseMediatorEndpoints();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
