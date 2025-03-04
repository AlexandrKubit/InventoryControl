using Exchange.Queries.Directories.Client.Form;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace UI.Pages.Directories.Client;
public partial class Form
{
    [Parameter]
    public string GuidString { get; set; }

    Model Client;

    protected override async Task OnInitializedAsync()
    {
        await GetAsync();
    }

    public async Task GetAsync()
    {
        if (GuidString == Guid.Empty.ToString())
        {
            Client = new Model();
        }
        else
        {
            var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Client/Form", new Request { Guid = Guid.Parse(GuidString) });
            Client = await result.Content.ReadFromJsonAsync<Model>();
        }
    }

    public async Task SaveAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Client/Save", new Exchange.Commands.Directories.Client.Save.Request { Guid = Client.Guid, Address = Client.Address, Name = Client.Name });
        Navigation.NavigateTo("/clients/1");
    }

    public async Task DeleteAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Client/Delete", new Exchange.Commands.Directories.Client.Delete.Request { Guid = Client.Guid});
        Navigation.NavigateTo("/clients/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Client/ChangeCondition", new Exchange.Commands.Directories.Client.ChangeCondition.Request { Guid = Client.Guid });
        Navigation.NavigateTo("/clients/1");
    }
}