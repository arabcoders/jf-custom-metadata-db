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

namespace CustomMetadataDB;

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
        _logger.LogDebug($"DEP HasChanged: {item.Path}");

        FileSystemMetadata fileInfo = directoryService.GetFile(item.Path);
        var result = fileInfo.Exists && fileInfo.LastWriteTimeUtc.ToUniversalTime() > item.DateLastSaved.ToUniversalTime();

        string status = result ? "Has Changed" : "Has Not Changed";

        _logger.LogDebug($"DEP HasChanged Result: {status}");

        return result;
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

        return Task.FromResult(Utils.ToEpisode(item));
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken) => throw new NotImplementedException();

    public virtual Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => throw new NotImplementedException();
}
