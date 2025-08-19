namespace App.Commands.Handlers.Directories.Client.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Client.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));
        await Client.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

