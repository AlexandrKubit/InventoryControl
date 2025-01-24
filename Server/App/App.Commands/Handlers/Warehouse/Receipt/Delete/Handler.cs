namespace App.Commands.Handlers.Warehouse.Receipt.Delete;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Warehouse.Receipt;
using Exchange.Commands.Warehouse.Receipt.Delete;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Receipt/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Receipt.FillByGuids([request.Guid]);
        await uow.ReceiptItem.FillByReceiptGuids([request.Guid]);
        var args = uow.ReceiptItem.List.Where(x => x.ReceiptGuid == request.Guid).Select(x => (x.ResourceGuid, x.MeasureUnitGuid));
        await uow.Balance.FillByResourceMeasureUnit(args);

        var receipt = uow.Receipt.List.FirstOrDefault(x => x.Guid == request.Guid);
        Document.DeleteRange([receipt], uow);
        return receipt.Guid;
    }
}

