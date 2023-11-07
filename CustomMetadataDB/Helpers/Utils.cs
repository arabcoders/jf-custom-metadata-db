using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CustomMetadataDB.Helpers
{
    public class Utils
    {
        public static readonly JsonSerializerOptions JSON_OPTS = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };

        public static ILogger Logger { get; set; } = null;

        public static PersonInfo CreatePerson(string name, string provider_id)
        {
            return new PersonInfo
            {
                Name = name,
                Type = PersonType.Director,
                ProviderIds = new Dictionary<string, string> { { Constants.PLUGIN_NAME, provider_id } },
            };
        }
        public static MetadataResult<Series> YTDLJsonToSeries(DTO data)
        {
            Logger?.LogInformation($"Processing {data}.");
            var item = new Series();
            var result = new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = item
            };
            result.Item.ProviderIds.Add(Constants.PLUGIN_NAME, data.Id);
            return result;
        }
    }
}
