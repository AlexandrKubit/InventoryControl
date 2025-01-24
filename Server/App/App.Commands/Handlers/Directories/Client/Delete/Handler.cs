namespace App.Commands.Handlers.Directories.Client.Delete;

using App.Base.Mediator;
using App.Commands.Base;
using Exchange.Commands.Directories.Client.Delete;
using System.Threading.Tasks;
using Domain.Entities.Directories;

[RequestRoute("/Directories/Client/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Client.FillByGuids([request.Guid]);
        await uow.Shipment.FillByClients([request.Guid]);

        var client = uow.Client.List.FirstOrDefault(x => x.Guid == request.Guid);
        Client.DeleteRange([client], uow);
        return client.Guid;
    }
}

