using System.Linq;
using BcGov.Jag.AccountManagement.Client.Components;
using BcGov.Jag.AccountManagement.Client.Data;
using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components;

namespace BcGov.Jag.AccountManagement.Client.Pages;

public partial class ManageUsers
{
    [Inject]
    protected IRepository Repository { get; set; } = default!;

    /// <summary>
    /// If true we are searching for user.
    /// </summary>
    private bool spinning = false;

    /// <summary>
    /// Represents the data to search for.
    /// </summary>
    private UserSearchModel searchModel = new UserSearchModel();

    /// <summary>
    /// Represents the data found.
    /// </summary>
    private DetailedUser user = new DetailedUser();

    private IList<ProjectMembershipModel> projectMembershipRows = Array.Empty<ProjectMembershipModel>();

    private IList<ProjectMembershipModel> projectMembershipChanges = Array.Empty<ProjectMembershipModel>();

    private async Task OnValidSubmit()
    {
        spinning = true;
        await Task.Delay(1);

        try
        {
            user = await Repository.LookupAsync(searchModel.Username);
            projectMembershipRows = user.ToViewModel();
            projectMembershipChanges = Array.Empty<ProjectMembershipModel>();
        }
        finally
        {
            spinning = false; // ensure we disable spinner even if there is an error
            await Task.Delay(1);
        }
    }

    private async Task GetChanges()
    {
        projectMembershipChanges = user.GetChanges(projectMembershipRows).ToList();
    }
}
