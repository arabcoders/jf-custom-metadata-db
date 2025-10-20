using CustomMetadataDB.Helpers;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using System.Text.RegularExpressions;

namespace CustomMetadataDB.Provider;

public class SeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
{
    protected readonly ILogger _logger;
    public string Name => Constants.PLUGIN_NAME;


    public SeasonProvider(ILogger<SeasonProvider> logger)
    {
        _logger = logger;
        Utils.Logger = logger;
    }

    public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"CMD Season GetMetadata: {JsonSerializer.Serialize(info)}");
        cancellationToken.ThrowIfCancellationRequested();
        var session = new MetadataResult<Season> { HasMetadata = false };

        if (string.IsNullOrEmpty(info.Path))
        {
            var ses = info.IndexNumber.ToString();
            for (int i = 21; i <= 26; i++)
            {
                if (!ses.StartsWith($"{i}"))
                {
                    continue;
                }
                var realSeason = int.Parse("20" + i.ToString());

                session.HasMetadata = true;
                session.Item = new Season { Name = $"Season {realSeason}", IndexNumber = realSeason };

                _logger.LogDebug($"GetMetadata: Fixing season number '{ses}' to {realSeason}.");
                return Task.FromResult(session);
            }

            return Task.FromResult(session);
        }

        // -- Parse last Part "Season XXXX"
        var lastPart = info.Path.Substring(info.Path.LastIndexOf('/') + 1);
        if (!lastPart.StartsWith("Season ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"GetMetadata: No season information found in path '{info.Path}'.");
            return Task.FromResult(session);
        }

        var match = Regex.Match(lastPart, @"Season\s*(\d+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            _logger.LogWarning($"GetMetadata: No season information found in path '{info.Path}'.");
            return Task.FromResult(session);
        }

        if (!int.TryParse(match.Groups[1].Value, out int seasonNumber))
        {
            _logger.LogWarning($"GetMetadata: Invalid season number '{match.Groups[1].Value}' in path '{info.Path}'.");
            return Task.FromResult(session);
        }

        _logger.LogDebug($"GetMetadata: Found season number {seasonNumber} in path '{info.Path}'.");

        session.HasMetadata = true;
        session.Item = new Season { Name = $"Season {seasonNumber}", IndexNumber = seasonNumber };

        return Task.FromResult(session);
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
}
