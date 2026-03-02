namespace App.Queries.Handlers.Warehouse.Shipment.Filters;

using App.Base.Mediator;
using Base;
using Dapper;
using Exchange.Queries.Warehouse.Shipment.Filters;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Filters", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request)
    {
        var model = new Model();

        var sql = @$"
            SELECT number
            FROM shipments s;

            select distinct c.guid, c.name
            from shipments s
            join clients c on s.client_guid = c.guid;

            select distinct r.guid, r.name
            from shipment_items i
            join resources r on i.resource_guid = r.guid;

            select distinct u.guid, u.name
            from shipment_items i
            join measure_units u on i.measure_unit_guid = u.guid;
        ";

        using var connection = Connection.Get();
        using var multi = await connection.QueryMultipleAsync(sql);

        model.Numbers = multi.Read<string>().ToList();
        model.Clients = multi.Read<Model.Select>().ToList();
        model.Resources = multi.Read<Model.Select>().ToList();
        model.MeasureUnits = multi.Read<Model.Select>().ToList();

        return model;
    }
}
