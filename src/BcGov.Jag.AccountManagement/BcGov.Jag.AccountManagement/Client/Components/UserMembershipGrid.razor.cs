using Microsoft.AspNetCore.Components;
using BcGov.Jag.AccountManagement.Shared;
namespace BcGov.Jag.AccountManagement.Client.Components;

public partial class UserMembershipGrid
{
    [EditorRequired]
    [Parameter]
    public IList<ProjectMembershipModel>? RowsData { get; set; }
}
