using Exchange.Queries.Directories.Resource.List;
using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Pages.Directories.Resource;
public partial class Index
{
    [Parameter]
    public int Condition { get; set; }

    List<Model> Resources = new List<Model>();

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
        var result = await HttpService.GetDataAsync<Request, List<Model>>("/Directories/Resource/List", new Request { Condition = Condition });
        if (result.IsOk)
            Resources = result.Data;
    }
}