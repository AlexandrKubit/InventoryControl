namespace Exchange.Commands.Warehouse.Receipt.Delete;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
