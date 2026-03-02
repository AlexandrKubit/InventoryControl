namespace App.Queries.Handlers.Directories.Client.List;

using App.Base.Mediator;
using Base;
using Dapper;
using Exchange.Queries.Directories.Client.List;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/List", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, IEnumerable<Model>>
{
    public async Task<IEnumerable<Model>> HandleAsync(Request request)
    {
        var sql = @$"
            SELECT guid, name, address
            FROM clients
            WHERE condition = @Condition
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryAsync<Model>(sql, new { request.Condition });

        return model;
    }
}
