using Microsoft.AspNetCore.Components;
using BcGov.Jag.AccountManagement.Client.Data;
namespace BcGov.Jag.AccountManagement.Client.Components;

public partial class UserMembershipGrid
{
    [EditorRequired]
    [Parameter]
    public IList<ProjectMembershipModel>? RowsData { get; set; }
}
