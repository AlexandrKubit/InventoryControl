using Exchange.Queries.Warehouse.Shipment.Form;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using UI.Services;

namespace UI.Pages.Warehouse.Shipment;
public partial class Form
{
    [Parameter]
    public string GuidString { get; set; }

    Model Model;
    List<ItemRow> Rows;

    protected override async Task OnInitializedAsync()
    {
        await GetAsync();
    }

    public async Task GetAsync()
    {
        var result = await HttpService.GetDataAsync<Request, Model>("/Warehouse/Shipment/Form", new Request { Guid = Guid.Parse(GuidString) });
        if (result.IsOk)
        {
            Model = result.Data;

            if (Model.Document == null)
            {
                Model.Document = new Model.Shipment();
                Model.Document.ClientGuid = Model.Clients.FirstOrDefault()?.Guid ?? Guid.Empty;
            }

            Rows = new List<ItemRow>();

            foreach (var b in Model.Balances)
            {
                Rows.Add(new ItemRow
                {
                    Guid = Guid.Empty,
                    ResourceGuid = b.ResourceGuid,
                    MeasureUnitGuid = b.MeasureUnitGuid,
                    CurrentQuantity = 0,
                    MaxQuantity = b.Quantity
                });
            }

            foreach (var i in Model.Items)
            {
                var row = Rows.FirstOrDefault(x => x.ResourceGuid == i.ResourceGuid && x.MeasureUnitGuid == i.MeasureUnitGuid);
                if (row == null)
                {
                    row = new ItemRow { ResourceGuid = i.ResourceGuid, MeasureUnitGuid = i.MeasureUnitGuid, CurrentQuantity = 0, MaxQuantity = i.Quantity };
                    Rows.Add(row);
                }

                row.Guid = i.Guid;
                row.CurrentQuantity = i.Quantity;
            }

            Rows = Rows.OrderByDescending(x => x.CurrentQuantity).ToList();
        }
    }


    public async Task<Guid> SaveAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Warehouse.Shipment.Save.Request, Guid>("/Warehouse/Shipment/Save", new Exchange.Commands.Warehouse.Shipment.Save.Request
        {
            Guid = Model.Document.Guid,
            Date = Model.Document.Date,
            Number = Model.Document.Number,
            ClientGuid = Model.Document.ClientGuid,
            Items = Rows.Where(x => x.CurrentQuantity > 0).Select(x => new Exchange.Commands.Warehouse.Shipment.Save.Request.Item
            {
                Guid = x.Guid,
                ShipmentGuid = Model.Document.Guid,
                ResourceGuid = x.ResourceGuid,
                MeasureUnitGuid = x.MeasureUnitGuid,
                Quantity = x.CurrentQuantity
            }).ToList()
        });

        if (result.IsOk)
            return result.Data;
        else
            return Guid.Empty;
    }

    public async Task SaveAsyncWithNavigate()
    {
        var guid = await SaveAsync();
        if(guid != Guid.Empty)
            Navigation.NavigateTo("/shipments");
    }

    public async Task DeleteAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Warehouse.Shipment.Delete.Request, Guid>("/Warehouse/Shipment/Delete", new Exchange.Commands.Warehouse.Shipment.Delete.Request { Guid = Model.Document.Guid});
        if(result.IsOk)
            Navigation.NavigateTo("/shipments");
    }

    public async Task CangeConditionAsync()
    {
        if (Model.Document.Condition != 2)
            Model.Document.Guid = await SaveAsync();

        if (Model.Document.Guid != Guid.Empty)
        {
            var result = await HttpService.GetDataAsync<Exchange.Commands.Warehouse.Shipment.ChangeCondition.Request, Guid>("/Warehouse/Shipment/ChangeCondition", new Exchange.Commands.Warehouse.Shipment.ChangeCondition.Request { Guid = Model.Document.Guid });
            if (result.IsOk)
                Navigation.NavigateTo("/shipments");
        }
    }

    public class ItemRow()
    {
        public Guid Guid { get; set; }
        public Guid ResourceGuid { get; set; }
        public Guid MeasureUnitGuid { get; set; }

        private decimal currentQuantity;
        public decimal CurrentQuantity
        {
            get { return currentQuantity; }
            set { currentQuantity = value > MaxQuantity ? MaxQuantity : value; }
        }

        public decimal MaxQuantity { get; set; }
    }
}