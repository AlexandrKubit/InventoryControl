namespace Common.DI;

using App.Base.Mediator;
using App.Queries.Base;
using Domain.Base;
using Infrastructure.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddQueries(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Connection.SetCommectionString(connectionString);
    }

    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddScoped<UnitOfWork>(serviceProvider => new UnitOfWork(serviceProvider, connectionString));
        builder.Services.AddScoped<IData>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetService<UnitOfWork>());
    }
}
