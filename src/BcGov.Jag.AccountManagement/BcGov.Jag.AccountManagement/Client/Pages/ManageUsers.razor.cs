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

    private bool? notFound = null;

    private bool? successSave = null;

    private async Task OnValidSubmit()
    {
        notFound = null;
        successSave = null;
        spinning = true;
        await Task.Delay(1);

        try
        {
            var foundUser = await Repository.LookupAsync(searchModel.Username);
            if (foundUser is not null)
            {
                notFound = false;
                user = foundUser;
                projectMembershipRows = user.ToViewModel();
                projectMembershipChanges = Array.Empty<ProjectMembershipModel>();
            }
            else
            {
                notFound = true;
                user = new DetailedUser();
                projectMembershipRows = Array.Empty<ProjectMembershipModel>();
                projectMembershipChanges = Array.Empty<ProjectMembershipModel>();
            }
        }
        finally
        {
            spinning = false; // ensure we disable spinner even if there is an error
            await Task.Delay(1);
        }
    }

    private async Task SaveChanges()
    {
        spinning = true;
        successSave = null;
        Console.WriteLine("spinning set as true");
        try
        {
            projectMembershipChanges = user.GetChanges(projectMembershipRows).ToList();
            await Repository.UpdateUserProjectsAsync(searchModel.Username, projectMembershipChanges);
            successSave = true;
            spinning = false;
        } catch( Exception ex ) {
            Console.WriteLine("exception occur "+ex);
            successSave = false;
            spinning = false;
        }
    }
}
