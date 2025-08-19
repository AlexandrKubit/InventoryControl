namespace App.Commands.Handlers.Directories.MeasureUnit.ChangeCondition;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.ChangeCondition;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));
        await data.MeasureUnit.FillByGuids([request.Guid]);
        var unit = data.MeasureUnit.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (unit.Condition == MeasureUnit.Conditions.Work)
            await MeasureUnit.ToArchiveRange([request.Guid], data);
        else
            await MeasureUnit.ToWorkRange([request.Guid], data);

        return unit.Guid;
    }
}

