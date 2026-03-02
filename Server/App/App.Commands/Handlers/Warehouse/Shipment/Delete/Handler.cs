namespace App.Commands.Handlers.Warehouse.Shipment.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.Delete;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Document.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

