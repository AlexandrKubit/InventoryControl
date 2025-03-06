namespace Exchange.Queries.Warehouse.Shipment.Form;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
