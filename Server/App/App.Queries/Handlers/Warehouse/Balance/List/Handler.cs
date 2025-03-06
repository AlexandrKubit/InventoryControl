namespace App.Queries.Handlers.Warehouse.Balance.List;

using App.Base.Mediator;
using Exchange.Queries.Warehouse.Balance.List;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Warehouse/Balance/List", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request, IServiceProvider provider)
    {
        var model = new Model();

        var sql = @$"
            SELECT r.name as ResourceName, u.name as MeasureUnitName, b.quantity
            FROM balances b
            join resources r on b.resource_guid = r.guid
            join measure_units u on b.measure_unit_guid = u.guid
            where b.quantity > 0
            {(request.ResourceGuids.Count > 0 ? "and r.guid = ANY(@ResourceGuids)" : "")}
            {(request.MeasureUnitGuids.Count > 0 ? "and u.guid = ANY(@MeasureUnitGuids)" : "")}
            order by r.name, u.name
        ";

        using var connection = Connection.Get();
        model.Balances = (await connection.QueryAsync<Model.Balance>(sql, new {request.ResourceGuids, request.MeasureUnitGuids})).ToList();

        return model;
    }
}
