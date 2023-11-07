using System;
using System.Collections.Generic;
using CustomMetadataDB.Helpers;
using CustomMetadataDB.Helpers.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace CustomMetadataDB
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => Constants.PLUGIN_NAME;
        public static Plugin Instance { get; private set; }
        public override Guid Id => Guid.Parse(Constants.PLUGIN_GUID);
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
        public IEnumerable<PluginPageInfo> GetPages()
        {
            var prefix = GetType().Namespace;

            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = prefix + ".Configuration.Web.custom_metadata_db.html",
            };
            yield return new PluginPageInfo
            {
                Name = $"{Name}.js",
                EmbeddedResourcePath = prefix + ".Configuration.Web.custom_metadata_db.js"
            };
        }
    }
}
