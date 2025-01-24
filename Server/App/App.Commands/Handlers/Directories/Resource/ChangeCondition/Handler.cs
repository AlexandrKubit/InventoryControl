namespace App.Commands.Handlers.Directories.Resource.ChangeCondition;

using App.Base.Mediator;
using App.Commands.Base;
using Exchange.Commands.Directories.Resource.ChangeCondition;
using System.Threading.Tasks;
using Domain.Entities.Directories;

[RequestRoute("/Directories/Resource/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        await uow.Resource.FillByGuids([request.Guid]);
        var resource = uow.Resource.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (resource.Condition == Resource.Conditions.Work)
            resource.ToArchive();
        else
            resource.ToWork();

        return resource.Guid;
    }
}

