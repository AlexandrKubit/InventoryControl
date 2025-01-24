namespace App.Commands.Handlers.Directories.MeasureUnit.ChangeCondition;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.ChangeCondition;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.MeasureUnit.FillByGuids([request.Guid]);
        var measureUnit = uow.MeasureUnit.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (measureUnit.Condition == MeasureUnit.Conditions.Work)
            measureUnit.ToArchive();
        else
            measureUnit.ToWork();

        return measureUnit.Guid;
    }
}

