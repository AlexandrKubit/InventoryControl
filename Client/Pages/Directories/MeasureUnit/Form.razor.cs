using Exchange.Queries.Directories.MeasureUnit.Form;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

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
            var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/MeasureUnit/Form", new Request { Guid = Guid.Parse(GuidString) });
            Unit = await result.Content.ReadFromJsonAsync<Model>();
        }
    }

    public async Task SaveAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/MeasureUnit/Save", new Exchange.Commands.Directories.MeasureUnit.Save.Request { Guid = Unit.Guid, Name = Unit.Name });
        Navigation.NavigateTo("/units/1");
    }

    public async Task DeleteAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/MeasureUnit/Delete", new Exchange.Commands.Directories.MeasureUnit.Delete.Request { Guid = Unit.Guid});
        Navigation.NavigateTo("/units/1");
    }

    public async Task ChangeConditionAsync()
    {
        var result = await httpClient.PostAsJsonAsync($"http://localhost:5000/Directories/MeasureUnit/ChangeCondition", new Exchange.Commands.Directories.MeasureUnit.ChangeCondition.Request { Guid = Unit.Guid });
        Navigation.NavigateTo("/units/1");
    }
}