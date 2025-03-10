namespace Exchange.Commands.Warehouse.Shipment.ChangeCondition;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
