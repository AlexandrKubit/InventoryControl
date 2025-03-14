using Exchange.Queries.Directories.Resource.Form;
using Microsoft.AspNetCore.Components;
using UI.Services;

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
            var result = await HttpService.GetDataAsync<Request, Model>("/Directories/Resource/Form", new Request { Guid = Guid.Parse(GuidString) });
            if (result.IsOk)
                Resource = result.Data;
        }
    }

    public async Task SaveAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Resource.Save.Request, Guid>("/Directories/Resource/Save", new Exchange.Commands.Directories.Resource.Save.Request { Guid = Resource.Guid, Name = Resource.Name });
        if (result.IsOk) 
            Navigation.NavigateTo("/resources/1");
    }

    public async Task DeleteAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Resource.Delete.Request, Guid>("/Directories/Resource/Delete", new Exchange.Commands.Directories.Resource.Delete.Request { Guid = Resource.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/resources/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.Resource.ChangeCondition.Request, Guid>("/Directories/Resource/ChangeCondition", new Exchange.Commands.Directories.Resource.ChangeCondition.Request { Guid = Resource.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/resources/1");
    }
}