namespace Exchange.Commands.Warehouse.Receipt.Delete;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
