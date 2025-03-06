using Exchange.Queries.Directories.Client.List;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace UI.Pages.Directories.Client;
public partial class Index
{
    [Parameter]
    public int Condition { get; set; }

    List<Model> Clients = new List<Model>();

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
        var result = await httpClient.PostAsJsonAsync($"{Settings.Url}/Directories/Client/List", new Request { Condition = Condition });
        Clients = await result.Content.ReadFromJsonAsync<List<Model>>();
    }
}