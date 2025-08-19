namespace App.Commands.Handlers.Directories.Client.Save;

using App.Base.Mediator;
using Exchange.Commands.Directories.Client.Save;
using System.Threading.Tasks;
using Domain.Entities.Directories;
using Domain.Base;

[RequestRoute("/Directories/Client/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));

        if (request.Guid == Guid.Empty)
        {
            var clients = await Client.CreateRange([new Client.CreateArg(request.Name, request.Address)], data);
            return clients.First().Guid;
        }
        else
        {
            await Client.UpdateRange([new Client.UpdateArg(request.Guid, request.Name, request.Address)], data);
            return request.Guid;
        }
    }
}

