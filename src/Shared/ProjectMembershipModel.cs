namespace BcGov.Jag.AccountManagement.Shared;
public class ProjectMembershipModel
{
    public string ProjectName { get; set; }
    public bool? Dynamics { get; set; }
    public bool? SharePoint { get; set; }

    public string DynamicsMessage { get; set; } = string.Empty;
    public string SharePointMessage { get; set; } = string.Empty;

    public bool Selectable
    {
        get
        {
            // dynamics had error and sharepoint erorr not not exists
            if (DynamicsMessage != String.Empty && (SharePointMessage != string.Empty || SharePoint is null)) return false;

            return true;
        }
    }
}
