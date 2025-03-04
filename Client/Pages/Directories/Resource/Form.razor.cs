using Exchange.Queries.Directories.Resource.Form;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace UI.Pages.Directories.Resource;
public partial class Form
{
    [Parameter]
    public string GuidString { get; set; }

    Model Resource;

    protected override async Task OnInitializedAsync()
    {
        await GetAsync();
    }

    public async Task GetAsync()
    {
        if (GuidString == Guid.Empty.ToString())
        {
            Resource = new Model();
        }
        else
        {
            var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Resource/Form", new Request { Guid = Guid.Parse(GuidString) });
            Resource = await result.Content.ReadFromJsonAsync<Model>();
        }
    }

    public async Task SaveAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Resource/Save", new Exchange.Commands.Directories.Resource.Save.Request { Guid = Resource.Guid, Name = Resource.Name });
        Navigation.NavigateTo("/resources/1");
    }

    public async Task DeleteAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Resource/Delete", new Exchange.Commands.Directories.Resource.Delete.Request { Guid = Resource.Guid});
        Navigation.NavigateTo("/resources/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/Resource/ChangeCondition", new Exchange.Commands.Directories.Resource.ChangeCondition.Request { Guid = Resource.Guid });
        Navigation.NavigateTo("/resources/1");
    }
}