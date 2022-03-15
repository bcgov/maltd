using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BcGov.Jag.AccountManagement.Client.Components;

public partial class UserMembershipRow : IAsyncDisposable
{
    [Inject]
    IJSRuntime? JS { get; set; }

    [EditorRequired]
    [Parameter]
    public ProjectMembershipModel RowData { get; set; }

    private IJSObjectReference? module;

    private ElementReference inputElement;

    /// <summary>
    /// Represents if all of the projects resources are selected or unselected.
    /// If not all resources have the same state, the project state will be indeterminate.
    /// </summary>
    private bool? ProjectChecked { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateProjectChecked();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS!.InvokeAsync<IJSObjectReference>("import", "./scripts.js");
        }

        await module!.InvokeVoidAsync("setElementProperty", inputElement, "indeterminate", ProjectChecked is null);

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    private Task OnProjectChange(ChangeEventArgs e)
    {
        // set all the checkboxes the same
        ProjectChecked = (bool)e.Value!;

        if (RowData.Dynamics.HasValue) RowData.Dynamics = ProjectChecked;
        if (RowData.SharePoint.HasValue) RowData.SharePoint = ProjectChecked;

        return Task.CompletedTask;
    }

    private Task OnDynamicsChange(ChangeEventArgs e)
    {
        RowData.Dynamics = (bool)e.Value!;
        UpdateProjectChecked();
        return Task.CompletedTask;
    }

    private Task OnSharePointChange(ChangeEventArgs e)
    {
        RowData.SharePoint = (bool)e.Value!;
        UpdateProjectChecked();
        return Task.CompletedTask;
    }

    private void UpdateProjectChecked()
    {
        bool? previousValue = ProjectChecked;

        if (RowData.Dynamics.HasValue && RowData.SharePoint.HasValue)
        {
            // consider both properties
            bool dynamicsChecked = RowData.Dynamics.Value;
            bool sharePointChecked = RowData.SharePoint.Value;

            if (dynamicsChecked && sharePointChecked)
            {
                ProjectChecked = true; // both checked
            }
            else if (!dynamicsChecked && !sharePointChecked)
            {
                ProjectChecked = false; // both not checked
            }
            else
            {
                ProjectChecked = null; // only one is checked
            }
        }
        else if (RowData.Dynamics.HasValue && !RowData.SharePoint.HasValue)
        {
            ProjectChecked = RowData.Dynamics.Value; // consider only dynamics
        }
        else if (!RowData.Dynamics.HasValue && RowData.SharePoint.HasValue)
        {
            ProjectChecked = RowData.SharePoint.Value;// consider only sharepoint
        }

        if (ProjectChecked != previousValue)
        {
            StateHasChanged();
        }
    }
}
