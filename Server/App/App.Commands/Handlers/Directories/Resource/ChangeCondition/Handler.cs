namespace App.Commands.Handlers.Directories.Resource.ChangeCondition;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.ChangeCondition;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.Resources.EnsureByGuids([request.Guid]);
        var resource = data.Resources.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (resource.Condition == Resource.Conditions.Work)
            await Resource.ToArchiveRange([request.Guid], data);
        else
            await Resource.ToWorkRange([request.Guid], data);

        return resource.Guid;
    }
}

