namespace App.Queries.Handlers.Directories.MeasureUnit.Form;

using App.Base.Mediator;
using Base;
using Dapper;
using Exchange.Queries.Directories.MeasureUnit.Form;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Form", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request)
    {
        var sql = @$"
            SELECT guid, name, condition
            FROM measure_units
            WHERE guid = @Guid
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryFirstAsync<Model>(sql, new { request.Guid });

        return model;
    }
}
