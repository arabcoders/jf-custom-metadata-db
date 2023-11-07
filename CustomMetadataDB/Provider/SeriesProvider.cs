using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using CustomMetadataDB.Helpers;
using MediaBrowser.Controller.Configuration;
using System.Net.Http;

namespace CustomMetadataDB
{
    public class SeriesProvider : AbstractProvider<SeriesProvider, Series, SeriesInfo>
    {
        public SeriesProvider(IServerConfigurationManager config, IHttpClientFactory httpClientFactory, IFileSystem fileSystem, ILogger<SeriesProvider> logger) :
        base(config, httpClientFactory, fileSystem, logger)
        { }
        internal override MetadataResult<Series> GetMetadataImpl(SeriesInfo info)
        {
            MetadataResult<Series> result = new();
            _logger.LogDebug($"CMD Series GetMetadata: {info.Name} ({info.Path})");
            _logger.LogDebug($"CMD Series GetMetadata Result: {result}");
            return Utils.YTDLJsonToSeries(new DTO());
        }
    }
}
