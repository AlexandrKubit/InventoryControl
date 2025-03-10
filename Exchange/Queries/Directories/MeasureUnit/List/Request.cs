namespace Exchange.Queries.Directories.MeasureUnit.List;



public class Request : IRequest<IEnumerable<Model>>
{
    public int Condition { get; set; }
}
