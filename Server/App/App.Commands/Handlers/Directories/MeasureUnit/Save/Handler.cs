namespace App.Commands.Handlers.Directories.MeasureUnit.Save;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.Save;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.MeasureUnit.FillByNames([request.Name]);
        if (request.Guid == Guid.Empty)
        {
            return MeasureUnit.CreateRange([request.Name], uow).First().Guid;
        }
        else
        {
            await uow.MeasureUnit.FillByGuids([request.Guid]);
            var measureUnit = uow.MeasureUnit.List.FirstOrDefault(x => x.Guid == request.Guid);

            MeasureUnit.UpdateRange([new MeasureUnit.UpdateArg(measureUnit) with {
                Name = request.Name,
            }], uow);

            return measureUnit.Guid;
        }
    }
}

