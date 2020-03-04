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

```
PROJECTS__0__NAME
PROJECTS__0__RESOURCES__0__RESOURCE
PROJECTS__0__RESOURCES__0__TYPE
PROJECTS__0__RESOURCES__0__AUTHORIZATIONURI
PROJECTS__0__RESOURCES__0__CLIENTID        
PROJECTS__0__RESOURCES__0__CLIENTSECRET    
PROJECTS__0__RESOURCES__0__USERNAME        
PROJECTS__0__RESOURCES__0__PASSWORD        

LDAP__SERVER
LDAP__DISTINGUISHEDNAME
LDAP__USERNAME
LDAP__PASSWORD

```

## JSON

```json
{
   "Projects":[
      {
         "Name":"<name>",
         "Resources":[
            {
               "Type":"Dynamics",
               "Resource":"",
               "AuthorizationUri":"",
               "ClientId":"",
               "ClientSecret":"",
               "Username":"",
               "Password":""
            },
            {
               "Type":"SharePoint",
               "Resource":"",
               "AuthorizationUri":"",
               "ClientId":"",
               "ClientSecret":"",
               "Username":"",
               "Password":""
            }
         ]
      }
   ],

   "LDAP": {
      "Server": "",
      "DistinguishedName": "",
      "Username":"",
      "Password":""
   }
}
```

## User Secrets

In development mode, sensitive credentials can be used without adding to a configuration file that may accidently added
to source control. See [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows)
for more information. User secrets are only used for local development. At present, no secrets are required.

You will need to run these commands from the `BcGov.Malt.Web` directory.

```
dotnet user-secrets set Projects:0:Name             "<name-0>"
dotnet user-secrets set Projects:0:Resources:0:Resource         "<name-0-resource-1>"
dotnet user-secrets set Projects:0:Resources:0:Type             "<name-0-type-1>"
dotnet user-secrets set Projects:0:Resources:0:AuthorizationUri "https://sts4.gov.bc.ca/adfs/oauth2/token/"
dotnet user-secrets set Projects:0:Resources:0:ClientId         "<client-id>"
dotnet user-secrets set Projects:0:Resources:0:ClientSecret     "<client-secret>"
dotnet user-secrets set Projects:0:Resources:0:Username         "<username>"
dotnet user-secrets set Projects:0:Resources:0:Password         "<password>"

dotnet user-secrets set Projects:0:Resources::Resource         "<name-0-resource-2>"
dotnet user-secrets set Projects:0:Resources::Type             "<name-0-type-2>"
dotnet user-secrets set Projects:0:Resources:1:AuthorizationUri "https://sts4.gov.bc.ca/adfs/oauth2/token/"
dotnet user-secrets set Projects:0:Resources:1:ClientId         "<client-id>"
dotnet user-secrets set Projects:0:Resources:1:ClientSecret     "<client-secret>"
dotnet user-secrets set Projects:0:Resources:1:Username         "<username>"
dotnet user-secrets set Projects:0:Resources:1:Password         "<password>"

dotnet user-secrets set Projects:1:Name             "<name-1>"
dotnet user-secrets set Projects:1:Resources:0:Resource         "<name-1-resource-1>"
dotnet user-secrets set Projects:1:Resources:0:AuthorizationUri "<client-id>"
dotnet user-secrets set Projects:1:Resources:0:ClientId         "<client-id>"
dotnet user-secrets set Projects:1:Resources:0:ClientSecret     "<client-secret>"
dotnet user-secrets set Projects:1:Resources:0:Username         "<username>"
dotnet user-secrets set Projects:1:Resources:0:Password         "<password>"

dotnet user-secrets set LDAP:Server ""
dotnet user-secrets set LDAP:DistinguishedName ""
dotnet user-secrets set LDAP:Username ""
dotnet user-secrets set LDAP:Password ""
```

