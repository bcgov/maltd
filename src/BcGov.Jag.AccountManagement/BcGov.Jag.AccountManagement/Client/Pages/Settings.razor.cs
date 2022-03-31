using System.Net.Http.Headers;
using BcGov.Jag.AccountManagement.Client.Components;
using BcGov.Jag.AccountManagement.Client.Data;
using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components;
using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Client.Pages;

public partial class Settings
{
    [Inject]
    protected IRepository Repository { get; set; } = default!;

    private bool spinning = false;

    private bool? successValidate = null;

    private ProjectResource resourceModel = new ProjectResource();

    private string dynamicsUrl = null;

    private string authorizationUrl = null;

    private async Task Validate()
    {
        spinning = true;
        resourceModel.Resource = new System.Uri(dynamicsUrl);
        resourceModel.AuthorizationUri = new System.Uri(authorizationUrl);
        resourceModel.Type = ProjectType.Dynamics;
        resourceModel.RelyingPartyIdentifier = "";
        try
        {
            await Repository.validateProjectConfigAsync(resourceModel);
            successValidate = true;
            spinning = false;
        } catch( Exception ex ) {
            Console.WriteLine("exception occur "+ex);
            successValidate = false;
            spinning = false;
        }
    }
}