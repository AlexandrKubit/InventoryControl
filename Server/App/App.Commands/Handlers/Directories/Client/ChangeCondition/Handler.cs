namespace App.Commands.Handlers.Directories.Client.ChangeCondition;

using App.Base.Mediator;
using Exchange.Commands.Directories.Client.ChangeCondition;
using System.Threading.Tasks;
using Domain.Entities.Directories;
using Domain.Base;

[RequestRoute("/Directories/Client/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));
        await data.Client.FillByGuids([request.Guid]);
        var client = data.Client.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (client.Condition == Client.Conditions.Work)
           await Client.ToArchiveRange([request.Guid], data);
        else
            await Client.ToWorkRange([request.Guid], data);

        return client.Guid;
    }
}

