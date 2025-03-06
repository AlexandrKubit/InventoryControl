using Exchange.Queries.Warehouse.Shipment.Form;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

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
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Shipment/Form", new Request { Guid = Guid.Parse(GuidString) });
        Model = await result.Content.ReadFromJsonAsync<Model>();
        if (Model.Document == null)
        {
            Model.Document = new Model.Shipment();
            Model.Document.ClientGuid = Model.Clients.FirstOrDefault()?.Guid ?? Guid.Empty;
        }

        Rows = new List<ItemRow>();

        foreach(var b in Model.Balances)
        {
            Rows.Add(new ItemRow
            {
                Guid = Guid.Empty,
                ResourceGuid= b.ResourceGuid,
                MeasureUnitGuid = b.MeasureUnitGuid,
                CurrentQuantity = 0,
                MaxQuantity = b.Quantity 
            });
        }

        foreach(var i in Model.Items)
        {
            var row = Rows.FirstOrDefault(x => x.ResourceGuid == i.ResourceGuid && x.MeasureUnitGuid == i.MeasureUnitGuid);
            if(row == null)
            {
                row = new ItemRow { ResourceGuid = i.ResourceGuid, MeasureUnitGuid = i.MeasureUnitGuid, CurrentQuantity = 0, MaxQuantity = 0 };
                Rows.Add(row);
            }

            row.Guid = i.Guid;
            row.CurrentQuantity = i.Quantity;
        }

        Rows = Rows.OrderByDescending(x=> x.CurrentQuantity).ToList();
    }

    public async Task SaveAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Shipment/Save", new Exchange.Commands.Warehouse.Shipment.Save.Request
        {
            Guid = Model.Document.Guid,
            Date = Model.Document.Date,
            Number = Model.Document.Number,
            ClientGuid = Model.Document.ClientGuid,
            Items = Rows.Where(x=> x.CurrentQuantity > 0).Select(x => new Exchange.Commands.Warehouse.Shipment.Save.Request.Item
            {
                Guid = x.Guid,
                ShipmentGuid = Model.Document.Guid,
                ResourceGuid = x.ResourceGuid,
                MeasureUnitGuid = x.MeasureUnitGuid,
                Quantity = x.CurrentQuantity
            }).ToList()
        });
        Navigation.NavigateTo("/shipments");
    }

    public async Task DeleteAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Shipment/Delete", new Exchange.Commands.Warehouse.Shipment.Delete.Request { Guid = Model.Document.Guid});
        Navigation.NavigateTo("/shipments");
    }


    public void ChangeQuantity(ChangeEventArgs args, ItemRow row)
    {
        decimal quantity = 0;

        decimal.TryParse((string)args.Value, out quantity);
        if (quantity >= row.MaxQuantity)
            quantity = row.MaxQuantity;

        row.CurrentQuantity = quantity;
        StateHasChanged();
    }

    public class ItemRow()
    {
        public Guid Guid { get; set; }
        public Guid ResourceGuid { get; set; }
        public Guid MeasureUnitGuid { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal MaxQuantity { get; set; }
    }
}