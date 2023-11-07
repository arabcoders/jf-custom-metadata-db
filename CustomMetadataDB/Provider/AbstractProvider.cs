using CustomMetadataDB.Helpers;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            return Task.FromResult(GetMetadataImpl(info));
        }

        internal abstract MetadataResult<T> GetMetadataImpl(E data);

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(E searchInfo, CancellationToken cancellationToken) => throw new NotImplementedException();
        public virtual Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
