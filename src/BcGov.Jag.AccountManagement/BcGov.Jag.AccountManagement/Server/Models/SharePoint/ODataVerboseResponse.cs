using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public abstract class ODataVerboseResponse<TData> where TData : new()
{
    private TData _data;

    [JsonPropertyName("d")]
    public TData Data
    {
        get => _data ??= new TData();
        set => _data = value;
    }
}
