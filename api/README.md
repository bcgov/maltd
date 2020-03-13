# Management of Accounts and Licensing Tool for Dynamics/Sharepoint API

Get the OAS3 Swagger GUI page

```
https://localhost:44323/swagger/index.html
```

Get OAS3 specification file

```
https://localhost:44323/swagger/v1/swagger.json
```

# Configuration

In dotnet, there are multiple options for configuring applications. For non-Visual Studio developers, using environment variables are preferred.

## Environment Variables

Below, _n_ and _m_ are place holders for array indexes, 0, 1, 2, etc

| Variable Name                                                 | Description                                                          |
| ------------------------------------------------------------- | -------------------------------------------------------------------- |
| PROJECTS\_\__n_\_\_NAME                                       | The unique name of the project. This will be displayed to the users. |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_RESOURCE               | The URL of the resource. Dynamics or Sharepoint site urls            |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_TYPE                   | The type of the resource, Dynamics or Sharepoint                     |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_AUTHORIZATIONURI       | The URL where authenication occurs, STS/SAML or OAuth endpoints      |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_USERNAME               | The authentication username                                          |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_PASSWORD               | The authentication password                                          |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_CLIENTID               | Dynamics only. The client id for OAuth                               |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_CLIENTSECRET           | Dynamics only. The client secret for OAuth                           |
| PROJECTS\_\__n_\_\_RESOURCES\_\__m_\_\_RELYINGPARTYIDENTIFIER | Sharepoint only. The relying party identifier for SAML auth          |
| LDAP\_\_SERVER                                                | The host name of the Active Directory server                         |
| LDAP\_\_DISTINGUISHEDNAME                                     | The distinguished name of the directory to search                    |
| LDAP\_\_USERNAME                                              | The username of the user to authenicate to Active Directory with     |
| LDAP\_\_PASSWORD                                              | The password of the user to authenicate to Active Directory with     |

All values are required except for these cases,

- Dynamics connections require CLIENTID and CLIENTSECRET.
- Sharepoint connections require RELYINGPARTYIDENTIFIER.

## User Secrets

In development mode, sensitive credentials can be used without adding to a configuration file that may accidently added
to source control. See [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows)
for more information. User secrets are only used for local development. At present, no secrets are required.

You will need to run these commands from the `BcGov.Malt.Web` directory.

```
dotnet user-secrets set Projects:0:Name                         "<name-0>"
dotnet user-secrets set Projects:0:Resources:0:Resource         "<name-0-resource-1>"
dotnet user-secrets set Projects:0:Resources:0:Type             "Dynamics"
dotnet user-secrets set Projects:0:Resources:0:AuthorizationUri "https://sts4.gov.bc.ca/adfs/oauth2/token/"
dotnet user-secrets set Projects:0:Resources:0:ClientId         "<client-id>"
dotnet user-secrets set Projects:0:Resources:0:ClientSecret     "<client-secret>"
dotnet user-secrets set Projects:0:Resources:0:Username         "<username>"
dotnet user-secrets set Projects:0:Resources:0:Password         "<password>"

dotnet user-secrets set Projects:0:Resources::Resource                "<name-0-resource-2>"
dotnet user-secrets set Projects:0:Resources::Type                    "Sharepoint"
dotnet user-secrets set Projects:0:Resources:1:AuthorizationUri       "https://sts4.gov.bc.ca/adfs/oauth2/token/"
dotnet user-secrets set Projects:0:Resources:1:RelyingPartyIdentifier "urn:..."
dotnet user-secrets set Projects:0:Resources:1:Username               "<username>"
dotnet user-secrets set Projects:0:Resources:1:Password               "<password>"

dotnet user-secrets set LDAP:Server ""
dotnet user-secrets set LDAP:DistinguishedName ""
dotnet user-secrets set LDAP:Username ""
dotnet user-secrets set LDAP:Password ""
```

## Logging

In development, using Seq can be useful for viewing / searching logs. This section describes how to configure your enviroment to
send logs to [Seq](https://datalust.co/seq). This solution uses the [Serilog Seq Sink](https://github.com/serilog/serilog-sinks-seq).
HEre are example using user secrets.

**Note**: currently to configure logging using either the `appsettings.Development.json` file or `user secrets`,
the solution needs to be DEBUG build.

```
dotnet user-secrets set Serilog:Using:0                                 "Serilog.Sinks.Seq"
dotnet user-secrets set Serilog:MinimumLevel:Default                    "Verbose"

dotnet user-secrets set Serilog:MinimumLevel:Override:Microsoft         "Warning"
dotnet user-secrets set Serilog:MinimumLevel:Override:System            "Warning"

dotnet user-secrets set Serilog:WriteTo:0:Name                          "Seq"
dotnet user-secrets set Serilog:WriteTo:0:Args:serverUrl                "http://localhost:5341"
dotnet user-secrets set Serilog:WriteTo:0:Args:apiKey                   "none"
dotnet user-secrets set Serilog:WriteTo:0:Args:restrictedToMinimumLevel "Verbose"

dotnet user-secrets set Serilog:Enrich:0                                "FromLogContext"
dotnet user-secrets set Serilog:Enrich:1                                "WithMachineName"
dotnet user-secrets set Serilog:Enrich:2                                "WithThreadId"
```
