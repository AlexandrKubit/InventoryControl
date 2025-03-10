using System.Net.Http.Json;
using UI.Services;
using R = Exchange.Queries.Warehouse.Shipment;

namespace UI.Pages.Warehouse.Shipment;
public partial class Index
{
    R.Filters.Model Filters = null;
    R.List.Model List = null;

    public DateTime Start = DateTime.Now.AddDays(-7).Date;
    public DateTime End = DateTime.Now.AddDays(7).Date;
    List<string> SelectedNumbers = new List<string>();
    List<Guid> SelectedClientGuids = new List<Guid>();
    List<Guid> SelectedResourceGuids = new List<Guid>();
    List<Guid> SelectedMeasureUnitGuids = new List<Guid>();

    protected override async Task OnInitializedAsync()
    {
        await GetFiltersAsync();
        await GetListAsync();
    }

    public async Task GetFiltersAsync()
    {
        var result = await HttpService.GetDataAsync<R.Filters.Request, R.Filters.Model>("/Warehouse/Shipment/Filters", new R.Filters.Request());
        if (result.IsOk)
            Filters = result.Data;
    }

    public async Task GetListAsync()
    {
        var result = await HttpService.GetDataAsync<R.List.Request, R.List.Model>("/Warehouse/Shipment/List", new R.List.Request { Start = Start, End = End, Numbers = SelectedNumbers, ClientGuids = SelectedClientGuids, ResourceGuids = SelectedResourceGuids, MeasureUnitGuids = SelectedMeasureUnitGuids });
        if (result.IsOk)
            List = result.Data;
    }
}