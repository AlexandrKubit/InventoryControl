namespace App.Queries.Handlers.Warehouse.Receipt.List;

using App.Base.Mediator;
using Exchange.Queries.Warehouse.Receipt.List;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Warehouse/Receipt/List", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request, IServiceProvider provider)
    {
        var model = new Model();

        var sql = @$"
            select d.guid, d.number, d.date::date, r.name as ResourceName, u.name as MeasureUnitName, i.quantity
            from receipts d
            left join receipt_items i on d.guid = i.receipt_guid
            left join resources r on i.resource_guid = r.guid
            left join measure_units u on i.measure_unit_guid = u.guid
            where d.date >= @Start and d.date <= @End
            {(request.Numbers.Count > 0 ? " and d.number = ANY(@Numbers)" : "")}
            {(request.ResourceGuids.Count > 0 ? " and r.guid = ANY(@ResourceGuids)" : "")}
            {(request.MeasureUnitGuids.Count > 0 ? " and u.guid = ANY(@MeasureUnitGuids)" : "")}
            order by d.date;
        ";

        using var connection = Connection.Get();
        List<Response> responses = (await connection.QueryAsync<Response>(sql, new {
            request.Start,
            request.End,
            request.Numbers,
            request.ResourceGuids, 
            request.MeasureUnitGuids
        })).ToList();

        if (responses.Any())
        {
            model.Receipts = responses
                .GroupBy(x => new { x.Guid, x.Date, x.Number })
                .Select(x => new Model.Receipt { Guid = x.Key.Guid, Date = x.Key.Date, Number = x.Key.Number })
                .ToList();

            model.Items = responses
                .Select(x => new Model.Item { ReceiptGuid = x.Guid, ResourceName = x.ResourceName, MeasureUnitName = x.MeasureUnitName, Quantity = x.Quantity })
                .ToList();
        }

        return model;
    }

    internal class Response
    {
        public Guid Guid { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string ResourceName { get; set; }
        public string MeasureUnitName { get; set; }
        public decimal Quantity { get; set; }
    }
}
