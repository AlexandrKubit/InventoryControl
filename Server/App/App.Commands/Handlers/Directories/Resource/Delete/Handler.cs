namespace App.Commands.Handlers.Directories.Resource.Delete;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        
        await uow.Resource.FillByGuids([request.Guid]);
        await uow.ReceiptItem.FillByResourceGuids([request.Guid]);
        await uow.Balance.FillByResourceGuids([request.Guid]);
        await uow.ShipmentItem.FillByResourceGuids([request.Guid]);

        var resource = uow.Resource.List.FirstOrDefault(x => x.Guid == request.Guid);
        Resource.DeleteRange([resource], uow);
        return resource.Guid;
    }
}

