using App.Base.Mediator;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace Common.DI;
public static class WebApplicationExtensions
{
    public static void UseDomain(this WebApplication app)
    {
        var type = typeof(Domain.Base.BaseEntity);
        var asm = Assembly.GetAssembly(typeof(Domain.Base.BaseEntity));
        var types = asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));
    }

    public static void UseMediatorEndpoints(this WebApplication app)
    {
        Mediator.RegisterEndpoints(app);
    }
}
