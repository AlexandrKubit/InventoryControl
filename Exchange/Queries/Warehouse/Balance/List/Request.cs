namespace Exchange.Queries.Warehouse.Balance.List;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public List<Guid> ResourceGuids { get; set; }
    public List<Guid> MeasureUnitGuids { get; set; }
}
