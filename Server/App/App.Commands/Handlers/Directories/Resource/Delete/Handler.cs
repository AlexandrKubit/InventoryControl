namespace App.Commands.Handlers.Directories.Resource.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));
        await Resource.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

