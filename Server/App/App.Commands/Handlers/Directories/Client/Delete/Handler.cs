namespace App.Commands.Handlers.Directories.Client.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Client.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Client.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

