namespace Exchange.Queries.Warehouse.Shipment.List;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<string> Numbers { get; set; }
    public List<Guid> ClientGuids { get; set; }
    public List<Guid> ResourceGuids { get; set; }
    public List<Guid> MeasureUnitGuids { get; set; }
}
