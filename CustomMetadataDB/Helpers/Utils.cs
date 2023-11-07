using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
        public static MetadataResult<Series> ToSeries(DTO data)
        {
            Logger?.LogInformation($"Processing {data}.");

            var item = new Series();

            if (string.IsNullOrEmpty(data.Id))
            {
                Logger?.LogInformation($"No Id found for {data}.");
                return ErrorOut();
            }

            item.SetProviderId(Constants.PLUGIN_NAME, data.Id);

            if (string.IsNullOrEmpty(data.Title))
            {
                Logger?.LogInformation($"No Title found for {data}.");
                return ErrorOut();
            }
            item.Name = data.Title;

            if (!string.IsNullOrEmpty(data.Title))
            {
                item.Overview = data.Description;
            }

            if (data.Rating != null)
            {
                item.CommunityRating = data.Rating;
            }

            if (!string.IsNullOrEmpty(data.RatingGuide))
            {
                item.OfficialRating = data.RatingGuide;
            }

            if (data.Genres.Length > 0)
            {
                item.Genres = data.Genres;
            }

            if (null != data.Premiere && data.Premiere is DateTime time)
            {
                var date = time;
                item.PremiereDate = date;
                item.ProductionYear = int.Parse(date.ToString("yyyy"));
            }

            return new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = item
            };
        }

        private static MetadataResult<Series> ErrorOut()
        {
            return new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = new Series()
            };
        }
    }
}
