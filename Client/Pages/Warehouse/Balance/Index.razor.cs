using UI.Services;
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
        Filters = await HttpService.GetDataAsync<B.Filters.Request, B.Filters.Model>("/Warehouse/Balance/Filters", new B.Filters.Request());
    }

    public async Task GetListAsync()
    {
        List = await HttpService.GetDataAsync<B.List.Request, B.List.Model>("/Warehouse/Balance/List", new B.List.Request { ResourceGuids = SelectedResourceGuids, MeasureUnitGuids = SelectedMeasureUnitGuids });
    }
}