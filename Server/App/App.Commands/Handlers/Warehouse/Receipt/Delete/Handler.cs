namespace App.Commands.Handlers.Warehouse.Receipt.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Receipt;
using Exchange.Commands.Warehouse.Receipt.Delete;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Receipt/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Document.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

