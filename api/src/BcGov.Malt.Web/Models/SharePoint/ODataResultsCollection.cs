using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
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
}
