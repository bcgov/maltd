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

    private bool? successSave = null;

    private string errorMessage = string.Empty;

    private async Task OnValidSubmit()
    {
        errorMessage = string.Empty;
        successSave = null;
        spinning = true;
        await Task.Delay(1);

        try
        {
            var foundUserResult = await Repository.LookupAsync(searchModel.Username);

            if (foundUserResult.IsSuccess)
            {
                var foundUser = foundUserResult.Value;
                user = foundUser;
                projectMembershipRows = user.ToViewModel();
                projectMembershipChanges = Array.Empty<ProjectMembershipModel>();
            }
            else
            {
                errorMessage = foundUserResult.Errors.Count != 0
                    ? foundUserResult.Errors[0].Message
                    : "Could not determine the error cause"; // this shouldn't happen

                OnFailure();
            }
        }
        catch (Exception exception)
        {
            errorMessage = $"Error: {exception.Message}";
            OnFailure();
        }
        finally
        {
            spinning = false; // ensure we disable spinner even if there is an error
            await Task.Delay(1);
        }
    }

    private void OnFailure()
    {
        user = new DetailedUser();
        projectMembershipRows = Array.Empty<ProjectMembershipModel>();
        projectMembershipChanges = Array.Empty<ProjectMembershipModel>();
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
