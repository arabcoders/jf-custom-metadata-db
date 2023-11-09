using CustomMetadataDB.Helpers;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
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

namespace CustomMetadataDB
{
    public abstract class AbstractProvider<B, T, E> : IRemoteMetadataProvider<T, E>
        where T : BaseItem, IHasLookupInfo<E>
        where E : ItemLookupInfo, new()
    {
        protected readonly IServerConfigurationManager _config;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger<B> _logger;
        protected readonly IFileSystem _fileSystem;

        public AbstractProvider(IServerConfigurationManager config, IHttpClientFactory httpClientFactory, IFileSystem fileSystem, ILogger<B> logger)
        {
            _config = config;
            _logger = logger;
            Utils.Logger = logger;
            _fileSystem = fileSystem;
            _httpClientFactory = httpClientFactory;

        }

        public virtual string Name { get; } = Constants.PLUGIN_NAME;

        public virtual Task<MetadataResult<T>> GetMetadata(E info, CancellationToken cancellationToken)
        {
            _logger.LogDebug("CMD GetMetadata: {Path}", info.Path);

            return GetMetadataImpl(info, cancellationToken);
        }

        internal abstract Task<MetadataResult<T>> GetMetadataImpl(E data, CancellationToken cancellationToken);

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(E searchInfo, CancellationToken cancellationToken)
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
        public virtual Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();

        protected Task<HttpResponseMessage> QueryAPI(string type, string name, CancellationToken cancellationToken, int limit = 1)
        {
            var apiUrl = Plugin.Instance.Configuration.ApiUrl;
            apiUrl += string.IsNullOrEmpty(new Uri(apiUrl).Query) ? "?" : "&";
            apiUrl += $"type={type}&limit={limit}&&query={HttpUtility.UrlEncode(name)}";

            return _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(new Uri(apiUrl), cancellationToken);
        }
    }
}
