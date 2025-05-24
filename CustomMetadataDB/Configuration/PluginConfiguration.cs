using MediaBrowser.Model.Plugins;

namespace CustomMetadataDB.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiRefUrl { get; set; }
    }
}
