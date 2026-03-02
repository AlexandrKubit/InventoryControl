namespace App.Commands.Handlers.Directories.Resource.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Resource.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

