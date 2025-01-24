namespace App.Commands.Handlers.Directories.Client.Save;

using App.Base.Mediator;
using App.Commands.Base;
using Exchange.Commands.Directories.Client.Save;
using System.Threading.Tasks;
using Domain.Entities.Directories;

[RequestRoute("/Directories/Client/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Client.FillByNames([request.Name]);

        if (request.Guid == Guid.Empty)
        {
            return Client.CreateRange([new Client.CreateArg(request.Name, request.Address)], uow).First().Guid;
        }
        else
        {
            await uow.Client.FillByGuids([request.Guid]);
            var client = uow.Client.List.FirstOrDefault(x => x.Guid == request.Guid);
            Client.UpdateRange([new Client.UpdateArg(client) with {
                Name = request.Name,
                Address = request.Address
            }], uow);

            return client.Guid;
        }
    }
}

