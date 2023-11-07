using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using CustomMetadataDB.Helpers;

namespace CustomMetadataDB
{
    public class SeriesExternalId : IExternalId
    {
        public bool Supports(IHasProviderIds item) => item is Series;
        public string ProviderName => Constants.PLUGIN_NAME;
        public string Key => Constants.PLUGIN_NAME;
        public ExternalIdMediaType? Type => ExternalIdMediaType.Series;
        public string UrlFormatString => Plugin.Instance.Configuration.ApiRefUrl;
    }
}
