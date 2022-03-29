using System.Diagnostics;

namespace BcGov.Jag.AccountManagement.Server.Infrastructure;

public static class DiagnosticTrace
{
    public const string Source = "BcGov.Jag.AccountManagement";
    private static readonly ActivitySource _source = new ActivitySource(Source, "1.0.0");

    public static Activity? StartActivity(string name) => _source.StartActivity(name);
}
