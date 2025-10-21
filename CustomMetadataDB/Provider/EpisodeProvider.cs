using CustomMetadataDB.Helpers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CustomMetadataDB.Provider;

public class EpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>, IHasItemChangeMonitor
{
    protected readonly ILogger<EpisodeProvider> _logger;

    public EpisodeProvider(ILogger<EpisodeProvider> logger)
    {
        _logger = logger;
        Utils.Logger = logger;
    }

    public string Name => Constants.PLUGIN_NAME;

    public bool HasChanged(BaseItem item, IDirectoryService directoryService)
    {
        try
        {
            FileSystemMetadata fileInfo = directoryService.GetFile(item.Path);

            if (false == fileInfo.Exists)
            {
                _logger.LogWarning($"CMD HasChanged: '{item.Path}' not found.");
                return true;
            }

            if (fileInfo.CreationTimeUtc.ToUniversalTime() > item.DateLastSaved.ToUniversalTime())
            {
                _logger.LogInformation($"CMD HasChanged: '{item.Path}' has changed");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CMD HasChanged: For path '{item.Path}' failed. '{ex.Message}'.");
        }

        return true;
    }

    public Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
    {
        MetadataResult<Episode> result = new() { HasMetadata = false };

        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug($"CMD Episode GetMetadata Lookup: '{info.Name}' '({info.Path})'");

        var item = Utils.FileToInfo(info.Path);
        if (item.Path == "")
        {
            _logger.LogWarning($"CMD Episode GetMetadata: No metadata found for '{info.Path}'.");
            return Task.FromResult(result);
        }

        return Task.FromResult(Utils.ToEpisode(item, info.Path));
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken) => throw new NotImplementedException();

    public virtual Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
}
