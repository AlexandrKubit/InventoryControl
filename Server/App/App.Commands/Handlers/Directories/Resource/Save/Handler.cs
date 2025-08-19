namespace App.Commands.Handlers.Directories.Resource.Save;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.Save;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));

        if (request.Guid == Guid.Empty)
        {
            var resources = await Resource.CreateRange([request.Name], data);
            return resources.First().Guid;
        }
        else
        {
            await Resource.UpdateRange([new Resource.UpdateArg(request.Guid, request.Name)], data);
            return request.Guid;
        }
    }
}

