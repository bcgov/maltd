using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public class ODataResultsCollection<TResult>
{
    private List<TResult> _results;

    [JsonPropertyName("results")]
    public List<TResult> Results
    {
        get => _results ??= new List<TResult>();
        set => _results = value;
    }
}
