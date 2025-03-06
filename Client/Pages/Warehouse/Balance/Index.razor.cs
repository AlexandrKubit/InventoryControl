using System.Net.Http.Json;
using B = Exchange.Queries.Warehouse.Balance;

namespace UI.Pages.Warehouse.Balance;
public partial class Index
{
    B.Filters.Model Filters = null;
    B.List.Model List = null;

    List<Guid> SelectedResourceGuids = new List<Guid>();
    List<Guid> SelectedMeasureUnitGuids = new List<Guid>();

    protected override async Task OnInitializedAsync()
    {
        await GetFiltersAsync();
        await GetListAsync();
    }

    public async Task GetFiltersAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Balance/Filters", new B.Filters.Request());
        Filters = await result.Content.ReadFromJsonAsync<B.Filters.Model>();
    }

    public async Task GetListAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Balance/List", new B.List.Request { ResourceGuids = SelectedResourceGuids, MeasureUnitGuids = SelectedMeasureUnitGuids });
        List = await result.Content.ReadFromJsonAsync<B.List.Model>();
    }
}