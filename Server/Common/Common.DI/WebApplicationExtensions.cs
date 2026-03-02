using App.Base.Mediator;
using Microsoft.AspNetCore.Builder;

namespace Common.DI;

public static class WebApplicationExtensions
{
    public static void UseMediatorEndpoints(this WebApplication app)
    {
        Mediator.RegisterEndpoints(app);
    }
}
