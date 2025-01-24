namespace App.Commands.Handlers.Directories.MeasureUnit.Delete;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.MeasureUnit.FillByGuids([request.Guid]);
        await uow.ReceiptItem.FillByMeasureUnitGuids([request.Guid]);
        await uow.Balance.FillByMeasureUnitGuids([request.Guid]);
        await uow.ShipmentItem.FillByMeasureUnitGuids([request.Guid]);

        var measureUnit = uow.MeasureUnit.List.FirstOrDefault(x => x.Guid == request.Guid);
        MeasureUnit.DeleteRange([measureUnit], uow);
        return measureUnit.Guid;
    }
}

