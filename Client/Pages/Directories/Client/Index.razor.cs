using Exchange.Queries.Directories.Client.List;
using Microsoft.AspNetCore.Components;
using UI.Services;

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
        Clients = await HttpService.GetDataAsync<Request, List<Model>>("/Directories/Client/List", new Request { Condition = Condition });
    }
}