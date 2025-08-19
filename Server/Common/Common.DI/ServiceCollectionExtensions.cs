namespace Common.DI;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Base;
using Infrastructure.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddMediator(this IServiceCollection services)
    {
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(ForDI)));
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(App.Queries.Base.Connection)));
    }

    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IData>(sp => sp.GetService<UnitOfWork>());
        services.AddScoped<IUnitOFWorkBase>(sp => sp.GetService<UnitOfWork>());
        services.AddScoped<Context>();
    }
}
