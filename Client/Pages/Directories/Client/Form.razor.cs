using Exchange.Queries.Directories.Client.Form;
using Microsoft.AspNetCore.Components;
using UI.Services;

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
            var result = await HttpService.GetDataAsync<Request, Model>("/Directories/Client/Form", new Request { Guid = Guid.Parse(GuidString) });
            if (result.IsOk)
                Client = result.Data;
        }
    }

    public async Task SaveAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Client.Save.Request, Guid>("/Directories/Client/Save", new Exchange.Commands.Directories.Client.Save.Request { Guid = Client.Guid, Address = Client.Address, Name = Client.Name });
        if(result.IsOk)
            Navigation.NavigateTo("/clients/1");
    }

    public async Task DeleteAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Client.Delete.Request, Guid>("/Directories/Client/Delete", new Exchange.Commands.Directories.Client.Delete.Request { Guid = Client.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/clients/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Client.ChangeCondition.Request, Guid>("/Directories/Client/ChangeCondition", new Exchange.Commands.Directories.Client.ChangeCondition.Request { Guid = Client.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/clients/1");
    }
}