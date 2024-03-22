using CustomMetadataDB.Helpers;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MediaBrowser.Controller.Entities.TV;

namespace CustomMetadataDB;

public class SeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>
{
    protected readonly IServerConfigurationManager _config;
    protected readonly IHttpClientFactory _httpClientFactory;
    protected readonly ILogger<SeriesProvider> _logger;

    public SeriesProvider(IHttpClientFactory httpClientFactory, ILogger<SeriesProvider> logger)
    {
        _logger = logger;
        Utils.Logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public string Name => Constants.PLUGIN_NAME;

    public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        MetadataResult<Series> result = new();
        _logger.LogDebug($"CMD Series GetMetadata: {info.Name} ({info.Path})");

        try
        {
            using var httpResponse = await QueryAPI("series", info.Name, cancellationToken).ConfigureAwait(false);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogInformation($"CMD Series GetMetadata: {info.Name} ({info.Path}) - Status Code: {httpResponse.StatusCode}");
                return result;
            }

            DTO[] seriesRootObject = await JsonSerializer.DeserializeAsync<DTO[]>(
                utf8Json: await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                options: Utils.JSON_OPTS,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            _logger.LogDebug($"CMD Series GetMetadata Result: {seriesRootObject}");
            return Utils.ToSeries(seriesRootObject[0]);
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode.HasValue && exception.StatusCode.Value == HttpStatusCode.NotFound)
            {
                return result;
            }

            throw;
        }
    }

    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug($"CMD Series GetMetadata: {searchInfo.Name} ({searchInfo.Path})");

        var result = new List<RemoteSearchResult>();

        try
        {
            using var httpResponse = await QueryAPI("series", searchInfo.Name, cancellationToken, limit: 20).ConfigureAwait(false);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogInformation($"CMD Series GetMetadata: {searchInfo.Name} ({searchInfo.Path}) - Status Code: {httpResponse.StatusCode}");
                return result;
            }

            DTO[] seriesRootObject = await JsonSerializer.DeserializeAsync<DTO[]>(
                utf8Json: await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                options: Utils.JSON_OPTS,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            foreach (var series in seriesRootObject)
            {
                result.Add(new RemoteSearchResult
                {
                    Name = series.Title,
                    ProviderIds = new Dictionary<string, string> { { Constants.PLUGIN_EXTERNAL_ID, series.Id } },
                });
            }

            _logger.LogDebug($"CMD Series GetMetadata Result: {result}");
            return result;
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode.HasValue && exception.StatusCode.Value == HttpStatusCode.NotFound)
            {
                return result;
            }

            throw;
        }
    }

    protected Task<HttpResponseMessage> QueryAPI(string type, string name, CancellationToken cancellationToken, int limit = 1)
    {
        var apiUrl = Plugin.Instance.Configuration.ApiUrl;
        apiUrl += string.IsNullOrEmpty(new Uri(apiUrl).Query) ? "?" : "&";
        apiUrl += $"type={type}&limit={limit}&&query={HttpUtility.UrlEncode(name)}";

        return _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(new Uri(apiUrl), cancellationToken);
    }

    public virtual Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
}
