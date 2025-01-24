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
        App.Base.Mediator.Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(IUnitOfWork)));
        //App.Base.Mediator.Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(App.Queries.Asm)));
    }

    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IData>(sp => sp.GetService<UnitOfWork>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetService<UnitOfWork>());
        services.AddScoped<IUnitOFWorkBase>(sp => sp.GetService<UnitOfWork>());
        services.AddScoped<Context>();
    }
}
