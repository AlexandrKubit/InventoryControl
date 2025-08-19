namespace App.Commands.Handlers.Directories.MeasureUnit.Save;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.Save;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));

        if (request.Guid == Guid.Empty)
        {
            var units = await MeasureUnit.CreateRange([request.Name], data);
            return units.First().Guid;
        }
        else
        {
            await MeasureUnit.UpdateRange([new MeasureUnit.UpdateArg(request.Guid, request.Name)], data);
            return request.Guid;
        }
    }
}

