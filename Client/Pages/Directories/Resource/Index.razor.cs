using Exchange.Queries.Directories.Resource.List;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

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
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Resource/List", new Request { Condition = Condition });
        Resources = await result.Content.ReadFromJsonAsync<List<Model>>();
    }
}