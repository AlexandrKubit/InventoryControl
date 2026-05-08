namespace App.Commands.Handlers.Directories.Client.Save;

using App.Base.Mediator;
using Exchange.Commands.Directories.Client.Save;
using System.Threading.Tasks;
using Domain.Entities.Directories;
using Domain.Base;

[RequestRoute("/Directories/Client/Save", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        if (request.Guid == Guid.Empty)
        {
            // Создание нового клиента
            var arg = new Client.CreateArg(request.Name, request.Address);
            var clients = await Client.CreateRange([arg], data);
            return clients.First().Guid;
        }
        else
        {
            // Обновление существующего клиента
            var arg = new Client.UpdateArg(
                request.Guid, 
                request.Name, 
                request.Address
            );
            await Client.UpdateRange([arg], data);
            return request.Guid;
        }
    }
}

