using BcGov.Jag.AccountManagement.Server.HealthChecks;
using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class AccessTokenLoader : IAccessTokenLoader
{
    private static readonly Exception? _noException = null;

    private readonly ProjectConfigurationCollection _projects;
    private readonly ISamlAuthenticator _samlAuthenticator;
    private readonly IOAuthClientFactory _oAuthClientFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="AccessTokenLoader" /> class.</summary>
    /// <param name="projects">The projects.</param>
    /// <param name="samlAuthenticator">The saml authenticator.</param>
    /// <param name="oAuthClientFactory">The o authentication client factory.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="projects"/>
    /// or
    /// <paramref name="samlAuthenticator"/>
    /// or
    /// <paramref name="oAuthClientFactory"/>
    /// or
    /// <paramref name="serviceProvider"/> is null.
    /// </exception>
    public AccessTokenLoader(
        ProjectConfigurationCollection projects, 
        ISamlAuthenticator samlAuthenticator, 
        IOAuthClientFactory oAuthClientFactory,
        IServiceProvider serviceProvider)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
        _oAuthClientFactory = oAuthClientFactory ?? throw new ArgumentNullException(nameof(oAuthClientFactory));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>Gets the each projects access token.</summary>
    /// <returns>A list of project resources and any exceptions thrown while getting the access token.</returns>
    public async Task<List<Tuple<ProjectResource, Exception>>> GetAccessTokensAsync()
    {
        List<Task<Tuple<ProjectResource, Exception>>> tasks = new List<Task<Tuple<ProjectResource, Exception>>>();

        foreach (var project in _projects)
        {
            foreach (ProjectResource resource in project.Resources)
            {
                tasks.Add(GetAccessTokenAsync(project, resource));
            }
        }

        await Task.WhenAll(tasks.Select(_ => Task.Run(() => _)));

        return tasks.Select(_ => _.Result).ToList();
    }


    private async Task<Tuple<ProjectResource, Exception>> GetAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        if (resource == null) throw new ArgumentNullException(nameof(resource));

        switch (resource.Type)
        {
            case ProjectType.Dynamics:
                return await GetOAuthAccessTokenAsync(project, resource);

            case ProjectType.SharePoint:
                return await GetSamlAccessTokenAsync(resource);

            default:
                Exception exception = new InvalidProjectTypeException(resource.Type);
                return Tuple.Create(resource, exception);
        }
    }


    private async Task<Tuple<ProjectResource, Exception>> GetOAuthAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        if (resource == null) throw new ArgumentNullException(nameof(resource));

        var options = new OAuthOptions
        {
            AuthorizationUri = resource.AuthorizationUri,
            ClientId = resource.ClientId,
            ClientSecret = resource.ClientSecret,
            Username = resource.Username,
            Password = resource.Password,
            Resource = resource.Resource
        };

        try
        {
            IOAuthClient oAuthClient = _oAuthClientFactory.Create(project);
            ITokenCache<OAuthOptions, Token> tokenCache = _serviceProvider.GetRequiredService<ITokenCache<OAuthOptions, Token>>();

            ITokenService tokenService = new OAuthTokenService(oAuthClient, tokenCache);

            // IOAuthClient does not cache tokens
            var token = await tokenService.GetTokenAsync(options, CancellationToken.None);
            return Tuple.Create(resource, _noException);
        }
        catch (Exception e)
        {
            return Tuple.Create(resource, e);
        }
    }

    private async Task<Tuple<ProjectResource, Exception>> GetSamlAccessTokenAsync(ProjectResource resource)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));

        string relyingParty = resource.RelyingPartyIdentifier;
        string username = resource.Username;
        string password = resource.Password;
        string stsUri = resource.AuthorizationUri.ToString();

        try
        {
            var token = await _samlAuthenticator.GetStsSamlTokenAsync(relyingParty, username, password, stsUri, cached: true);
            return Tuple.Create(resource, _noException);
        }
        catch (Exception e)
        {
            return Tuple.Create(resource, e);
        }
    }

}
