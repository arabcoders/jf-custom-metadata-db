﻿using Xunit;
using System;
using System.IO;
using CustomMetadataDB.Helpers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CustomMetadataDB.Tests;

public class UtilsTest
{
    private static readonly DTO[] _data = LoadFixtureData();
    private static readonly ILogger<UtilsTest> _logger = SetLogger();

    private static ILogger<UtilsTest> SetLogger()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<UtilsTest>();
    }

    private static DTO[] LoadFixtureData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "CustomMetadataDB.Tests.fixture.json";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new Exception($"Could not find resource {resourceName}");
        }

        using StreamReader reader = new StreamReader(stream);
        return JsonSerializer.Deserialize<DTO[]>(json: reader.ReadToEnd(), options: Utils.JSON_OPTS) ?? Array.Empty<DTO>();
    }

    [Theory]
    [InlineData("Show title One", "jp-814d9672e1")]
    [InlineData("Show title 1", "jp-814d9672e1")]
    [InlineData("Show title 2", "jp-814d9672e2")]
    [InlineData("Show title Two", "jp-814d9672e2")]
    [InlineData("Show Title Wrong", "")]
    public void Test_get_series_id_from_json(string dir, string expected)
    {
        var dirLower = dir.ToLower();
        var result = "";

        for (int i = 0; i < _data.Length; i++)
        {
            var item = _data[i];
            foreach (var match in item.Matcher)
            {
                if (match.ToLower() != dirLower)
                {
                    continue;
                }

                result = item.Id;
                break;
            }

            if (!string.IsNullOrEmpty(result))
            {
                break;
            }

            // -- try title.
            if (item.Title.ToLower() == dirLower)
            {
                result = item.Id;
                break;
            }
        }

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Test_file_to_info()
    {
        var path = "/home/media/test/201012 foobar ep24 - ahmed [foo].mkv";
        var item = Utils.FileToInfo(path);
        Assert.Equal(110125248, item.IndexNumber);
        Assert.Equal(2020, item.ParentIndexNumber);
        Assert.Equal(2020, item.Year);
        Assert.Equal("ep24 - ahmed", item.Name);
        Assert.Equal($"{item.IndexNumber}", item.ProviderIds[Constants.PLUGIN_EXTERNAL_ID]);
    }

    [Fact]
    public void Test_ToEpisode()
    {
        var path = "/home/media/test/201012 foobar ep24 - ahmed [foo].mkv";
        var result = Utils.ToEpisode(Utils.FileToInfo(path));

        Assert.True(result.HasMetadata);

        var item = result.Item;

        Assert.Equal(110125248, item.IndexNumber);
        Assert.Equal(2020, item.ParentIndexNumber);
        Assert.Equal("ep24 - ahmed", item.Name);
        Assert.Equal($"{item.IndexNumber}", item.ProviderIds[Constants.PLUGIN_EXTERNAL_ID]);
    }

    [Theory]
    [InlineData("Show Title - S01E66 - episode title [foo].mkv")]
    [InlineData("episode title ep066 [DVD].mkv")]
    [InlineData("S01E66 episode title [foo].mkv")]
    public void Test_toStandard_Naming(string filename)
    {
        var path = $"/home/media/Show Title/Season 01/{filename}";
        var result = Utils.ToEpisode(Utils.FileToInfo(path));

        Assert.True(result.HasMetadata);

        var item = result.Item;

        Assert.Equal(66, item.IndexNumber);
        Assert.Equal(1, item.ParentIndexNumber);
        Assert.Equal("episode title", item.Name);
        Assert.Equal($"{item.IndexNumber}", item.ProviderIds[Constants.PLUGIN_EXTERNAL_ID]);
    }

    [Theory]
    [InlineData("Show Title - S01E66-E67 - episode title [foo].mkv", 67)]
    [InlineData("episode title ep066-ep100 [DVD].mkv", 100)]
    public void Test_toStandard_Naming_Multi(string filename, int indexEnd)
    {
        var path = $"/home/media/Show Title/Season 01/{filename}";
        var result = Utils.ToEpisode(Utils.FileToInfo(path));

        Assert.True(result.HasMetadata);

        var item = result.Item;

        Assert.Equal(66, item.IndexNumber);
        Assert.Equal(1, item.ParentIndexNumber);
        Assert.Equal(indexEnd, item.IndexNumberEnd);
    }

    [Theory]
    [InlineData("230629 Yoake no LOVE it! - BAT [720p h264 MP4 CM Cut].mkv", 4957)]
    [InlineData("240629 Yoake no LOVE it! - FOO [720p h264 MP4 CM Cut].mkv", 4955)]
    [InlineData("240629 Yoake no LOVE it! - BAR [820p h264 MP4 CM Cut].mkv", 5451)]
    [InlineData("2406 NO [820p h264 MP4 CM Cut].mkv", 9852)]
    [InlineData("2306 OK [820p h264 MP4 CM Cut].mkv", 1025)]
    [InlineData("2206 TEST [820p h264 MP4 CM Cut].mkv", 5398)]
    public void Test_ExtendId(string filename, int expected)
    {
        Assert.Equal(expected, Utils.ExtendId(filename));
    }
}
