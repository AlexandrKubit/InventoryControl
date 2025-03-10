using Exchange.Queries.Warehouse.Receipt.Form;
using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Pages.Warehouse.Receipt;
public partial class Form
{
    [Parameter]
    public string GuidString { get; set; }

    Model Model;

    protected override async Task OnInitializedAsync()
    {
        await GetAsync();
    }

    public async Task GetAsync()
    {
        var result = await HttpService.GetDataAsync<Request, Model>("/Warehouse/Receipt/Form", new Request { Guid = Guid.Parse(GuidString) });
        if (result.IsOk)
        {
            Model = result.Data;

            if (Model.Document == null)
                Model.Document = new Model.Receipt();
        }
    }

    public async Task SaveAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Warehouse.Receipt.Save.Request, Guid>("/Warehouse/Receipt/Save", new Exchange.Commands.Warehouse.Receipt.Save.Request
        {
            Guid = Model.Document.Guid,
            Date = Model.Document.Date,
            Number = Model.Document.Number,
            Items = Model.Items.Select(x => new Exchange.Commands.Warehouse.Receipt.Save.Request.Item
            {
                Guid = x.Guid,
                ReceiptGuid = Model.Document.Guid,
                ResourceGuid = x.ResourceGuid,
                MeasureUnitGuid = x.MeasureUnitGuid,
                Quantity = x.Quantity
            }).ToList()
        });

        if(result.IsOk)
            Navigation.NavigateTo("/receipts");
    }

    public async Task DeleteAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Warehouse.Receipt.Delete.Request, Guid>("/Warehouse/Receipt/Delete", new Exchange.Commands.Warehouse.Receipt.Delete.Request { Guid = Model.Document.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/receipts");
    }

    public void AddItem()
    {
        Model.Items.Add(new Model.Item
        {
            ResourceGuid = Model.Resources.FirstOrDefault()?.Guid ?? Guid.Empty,
            MeasureUnitGuid = Model.MeasureUnits.FirstOrDefault()?.Guid ?? Guid.Empty,
            Quantity = 0
        });
    }

    public void RemoveItem(Model.Item item)
    {
        Model.Items.Remove(item);
        StateHasChanged();
    }
}