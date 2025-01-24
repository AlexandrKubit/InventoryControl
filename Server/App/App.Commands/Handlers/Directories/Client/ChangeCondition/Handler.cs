namespace App.Commands.Handlers.Directories.Client.ChangeCondition;

using App.Base.Mediator;
using App.Commands.Base;
using Exchange.Commands.Directories.Client.ChangeCondition;
using System.Threading.Tasks;
using Domain.Entities.Directories;

[RequestRoute("/Directories/Client/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        await uow.Client.FillByGuids([request.Guid]);
        var client = uow.Client.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (client.Condition == Client.Conditions.Work)
            client.ToArchive();
        else
            client.ToWork();

        return client.Guid;
    }
}

