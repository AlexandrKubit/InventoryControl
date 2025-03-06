namespace Exchange.Queries.Warehouse.Receipt.Form;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
