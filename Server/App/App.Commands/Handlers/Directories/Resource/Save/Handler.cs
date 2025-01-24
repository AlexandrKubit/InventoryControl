namespace App.Commands.Handlers.Directories.Resource.Save;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Directories;
using Exchange.Commands.Directories.Resource.Save;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Resource.FillByNames([request.Name]);
        if (request.Guid == Guid.Empty)
        {
            return Resource.CreateRange([request.Name], uow).First().Guid;
        }
        else
        {
            await uow.Resource.FillByGuids([request.Guid]);
            var resource = uow.Resource.List.FirstOrDefault(x => x.Guid == request.Guid);
            Resource.UpdateRange([new Resource.UpdateArg(resource) with {
                Name = request.Name,
            }], uow);

            return resource.Guid;
        }
    }
}

