using Exchange.Queries.Directories.MeasureUnit.List;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

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
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Directories/MeasureUnit/List", new Request { Condition = Condition });
        Units = await result.Content.ReadFromJsonAsync<List<Model>>();
    }
}