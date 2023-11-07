using MediaBrowser.Model.Plugins;

namespace CustomMetadataDB.Helpers.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiRefUrl { get; set; }
    }
}
