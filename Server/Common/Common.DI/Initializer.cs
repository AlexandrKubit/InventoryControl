namespace Common.DI;

using App.Base.Mediator;
using App.Commands.Base;
using System.Reflection;


public static class Initializer
{
    // регистрация хендлеров/валидаторов
    public static void InitializeMediator()
    {
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(ForDI)));
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(App.Queries.Base.Connection)));
    }

    // вызов статических конструкторов
    public static void InitializeDomain()
    {
        var type = typeof(Domain.Base.BaseEntity);
        var asm = Assembly.GetAssembly(typeof(Domain.Base.BaseEntity));
        var types = asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));
    }
}
