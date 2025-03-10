namespace Exchange.Queries.Directories.Client.Form;



public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
