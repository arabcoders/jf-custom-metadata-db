using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using CustomMetadataDB.Helpers;
using MediaBrowser.Controller.Configuration;
using System.Net;
using System.Net.Http;
using MediaBrowser.Common.Net;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace CustomMetadataDB
{
    public class SeriesProvider : AbstractProvider<SeriesProvider, Series, SeriesInfo>
    {
        public SeriesProvider(IServerConfigurationManager config, IHttpClientFactory httpClientFactory, IFileSystem fileSystem, ILogger<SeriesProvider> logger) :
        base(config, httpClientFactory, fileSystem, logger)
        { }
        internal override async Task<MetadataResult<Series>> GetMetadataImpl(SeriesInfo info, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MetadataResult<Series> result = new();
            _logger.LogDebug($"CMD Series GetMetadata: {info.Name} ({info.Path})");

            try
            {
                using var httpResponse = await QueryAPI("series",info.Name, cancellationToken).ConfigureAwait(false);

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
    }
}
