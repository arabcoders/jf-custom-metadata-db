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

namespace CustomMetadataDB;
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
        _logger.LogDebug($"CMD Season GetMetadata: {JsonSerializer.Serialize(info)}");

        cancellationToken.ThrowIfCancellationRequested();

        var result = new MetadataResult<Season>
        {
            HasMetadata = true,
            Item = new Season
            {
                Name = info.Name,
                IndexNumber = info.IndexNumber
            }
        };

        return Task.FromResult(result);
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
}
