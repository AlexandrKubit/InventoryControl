using Exchange.Queries.Directories.MeasureUnit.Form;
using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Pages.Directories.MeasureUnit;
public partial class Form
{
    [Parameter]
    public string GuidString { get; set; }

    Model Unit;

    protected override async Task OnInitializedAsync()
    {
        await GetAsync();
    }

    public async Task GetAsync()
    {
        if (GuidString == Guid.Empty.ToString())
        {
            Unit = new Model();
        }
        else
        {
            var result = await HttpService.GetDataAsync<Request, Model>("/Directories/MeasureUnit/Form", new Request { Guid = Guid.Parse(GuidString) });
            if (result.IsOk)
                Unit = result.Data;
        }
    }

    public async Task SaveAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.MeasureUnit.Save.Request, Guid>("/Directories/MeasureUnit/Save", new Exchange.Commands.Directories.MeasureUnit.Save.Request { Guid = Unit.Guid, Name = Unit.Name });
        if(result.IsOk)
            Navigation.NavigateTo("/units/1");
    }

    public async Task DeleteAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.MeasureUnit.Delete.Request, Guid>("/Directories/MeasureUnit/Delete", new Exchange.Commands.Directories.MeasureUnit.Delete.Request { Guid = Unit.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/units/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await HttpService.GetDataAsync<Exchange.Commands.Directories.MeasureUnit.ChangeCondition.Request, Guid>("/Directories/MeasureUnit/ChangeCondition", new Exchange.Commands.Directories.MeasureUnit.ChangeCondition.Request { Guid = Unit.Guid });
        if (result.IsOk) 
            Navigation.NavigateTo("/units/1");
    }
}