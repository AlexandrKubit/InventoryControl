namespace App.Queries.Handlers.Warehouse.Balance.Filters;

using App.Base.Mediator;
using Base;
using Dapper;
using Exchange.Queries.Warehouse.Balance.Filters;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Balance/Filters", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request)
    {
        var model = new Model();

        var sql = @$"
            SELECT distinct b.resource_guid as Guid, r.name as Name
            FROM balances b
            join resources r on b.resource_guid = r.guid
            where b.quantity > 0;

            SELECT distinct b.measure_unit_guid as Guid, u.name as Name
            FROM balances b
            join measure_units u on b.measure_unit_guid = u.guid
            where b.quantity > 0;
        ";

        using var connection = Connection.Get();
        using var multi = await connection.QueryMultipleAsync(sql);

        model.Resources = multi.Read<Model.Select>().ToList();
        model.MeasureUnits = multi.Read<Model.Select>().ToList();

        return model;
    }
}
