using Exchange.Queries.Directories.MeasureUnit.List;
using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Pages.Directories.MeasureUnit;
public partial class Index
{
    [Parameter]
    public int Condition { get; set; }

    List<Model> Units = new List<Model>();

    protected override async Task OnInitializedAsync()
    {
        await GetListAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetListAsync();
    }

    public async Task GetListAsync()
    {
        var result = await HttpService.GetDataAsync<Request, List<Model>>("/Directories/MeasureUnit/List", new Request { Condition = Condition });
        if (result.IsOk)
            Units = result.Data;
    }
}