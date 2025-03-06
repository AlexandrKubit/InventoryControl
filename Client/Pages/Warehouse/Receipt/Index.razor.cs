using System.Net.Http.Json;
using R = Exchange.Queries.Warehouse.Receipt;

namespace UI.Pages.Warehouse.Receipt;
public partial class Index
{
    R.Filters.Model Filters = null;
    R.List.Model List = null;

    public DateTime Start = DateTime.Now.AddDays(-7).Date;
    public DateTime End = DateTime.Now.AddDays(7).Date;
    List<string> SelectedNumbers = new List<string>();
    List<Guid> SelectedResourceGuids = new List<Guid>();
    List<Guid> SelectedMeasureUnitGuids = new List<Guid>();

    protected override async Task OnInitializedAsync()
    {
        await GetFiltersAsync();
        await GetListAsync();
    }

    public async Task GetFiltersAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Receipt/Filters", new R.Filters.Request());
        Filters = await result.Content.ReadFromJsonAsync<R.Filters.Model>();
    }

    public async Task GetListAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Warehouse/Receipt/List", new R.List.Request { Start = Start, End = End, Numbers = SelectedNumbers, ResourceGuids = SelectedResourceGuids, MeasureUnitGuids = SelectedMeasureUnitGuids });
        List = await result.Content.ReadFromJsonAsync<R.List.Model>();
    }
}