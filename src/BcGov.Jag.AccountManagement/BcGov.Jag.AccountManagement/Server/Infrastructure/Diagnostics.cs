using System.Diagnostics;

namespace BcGov.Jag.AccountManagement.Server.Infrastructure;

public static class Diagnostics
{
    public static readonly ActivitySource Source = new ActivitySource("account-management", "1.0.0");
}
