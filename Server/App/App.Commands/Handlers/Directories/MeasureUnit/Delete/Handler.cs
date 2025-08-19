namespace App.Commands.Handlers.Directories.MeasureUnit.Delete;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.MeasureUnit.Delete;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));
        await MeasureUnit.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

