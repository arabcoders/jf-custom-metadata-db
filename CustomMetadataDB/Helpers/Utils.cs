using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using Jellyfin.Data.Enums;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CustomMetadataDB.Helpers;

public class Utils
{
    public static readonly JsonSerializerOptions JSON_OPTS = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    public static ILogger Logger { get; set; } = null;

    public static PersonInfo CreatePerson(string name, string provider_id)
    {
        return new()
        {
            Name = name,
            Type = PersonKind.Director,
            ProviderIds = new Dictionary<string, string> { { Constants.PLUGIN_EXTERNAL_ID, provider_id } },
        };
    }

    public static int ExtendId(string path)
    {
        // 1. Get the basename, remove the extension, and convert to lowercase
        string basename = Path.GetFileNameWithoutExtension(path).ToLower();

        // 2. Hash the basename using SHA-256
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(basename));

        // 3. Convert the SHA-256 hash to a hexadecimal string
        string hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // 4. Convert hexadecimal characters to ASCII values
        StringBuilder asciiValues = new StringBuilder();
        foreach (char c in hex)
        {
            asciiValues.Append(((int)c).ToString());
        }

        // 5. Get the final 4 digits, ensure it's a 4-digit integer
        string asciiString = asciiValues.ToString();
        string fourDigitString = asciiString.Length >= 4 ? asciiString.Substring(0, 4) : asciiString.PadRight(4, '9');
        int fourDigitNumber = int.Parse(fourDigitString);

        Logger?.LogDebug($"'basename: {basename}' - 'path: {path}' createID: '{fourDigitNumber}' : '{fourDigitString}'.");

        return fourDigitNumber;
    }

    public static MetadataResult<Series> ToSeries(DTO data)
    {
        Logger?.LogDebug($"Processing {data}.");

        var item = new Series();

        if (string.IsNullOrEmpty(data.Id))
        {
            Logger?.LogInformation($"No Id found for {data}.");
            return ErrorOut();
        }

        item.SetProviderId(Constants.PLUGIN_EXTERNAL_ID, data.Id);

        if (string.IsNullOrEmpty(data.Title))
        {
            Logger?.LogInformation($"No Title found for {data}.");
            return ErrorOut();
        }
        item.Name = data.Title;

        if (!string.IsNullOrEmpty(data.Title))
        {
            item.Overview = data.Description;
        }

        if (data.Rating != null)
        {
            item.CommunityRating = data.Rating;
        }

        if (!string.IsNullOrEmpty(data.RatingGuide))
        {
            item.OfficialRating = data.RatingGuide;
        }

        if (data.Genres.Length > 0)
        {
            item.Genres = data.Genres;
        }

        if (null != data.Premiere && data.Premiere is DateTime time)
        {
            var date = time;
            item.PremiereDate = date;
            item.ProductionYear = int.Parse(date.ToString("yyyy"));
        }

        return new() { HasMetadata = true, Item = item };
    }

    public static EpisodeInfo FileToInfo(string file)
    {
        //-- get only the file stem
        string filename = Path.GetFileNameWithoutExtension(file);

        Match matcher = null;

        for (int i = 0; i < Constants.EPISODE_MATCHERS.Length; i++)
        {
            matcher = Constants.EPISODE_MATCHERS[i].Match(filename);
            if (!matcher.Success)
            {
                continue;
            }
            break;
        }

        if (!matcher.Success)
        {
            Logger?.LogError($"No match found for '{file}'.");
            return new EpisodeInfo();
        }

        string series = matcher.Groups["series"].Success ? matcher.Groups["series"].Value : "";
        string year = matcher.Groups["year"].Success ? matcher.Groups["year"].Value : "";
        year = (year.Length == 2) ? "20" + year : year;
        string month = matcher.Groups["month"].Success ? matcher.Groups["month"].Value : "";
        string day = matcher.Groups["day"].Success ? matcher.Groups["day"].Value : "";
        string episode = matcher.Groups["episode"].Success ? matcher.Groups["episode"].Value : "";

        string season = matcher.Groups["season"].Success ? matcher.Groups["season"].Value : "";
        season = season == "" ? year : season;

        string broadcastDate = (year != "" && month != "" && day != "") ? year + "-" + month + "-" + day : "";

        string title = matcher.Groups["title"].Success ? matcher.Groups["title"].Value : "";
        if (title != "")
        {
            if (!string.IsNullOrEmpty(series) && title != series && title.ToLower().Contains(series.ToLower()))
            {
                title = title.Replace(series, "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            if (title == "" && title == series && broadcastDate != "")
            {
                title = broadcastDate;
            }

            // -- replace double spaces with single space
            title = Regex.Replace(title, @"\[.+?\]", " ").Trim('-').Trim();
            title = Regex.Replace(title, @"\s+", " ");
            title = title.Trim().Trim('-').Trim();

            if (matcher.Groups["epNumber"].Success)
            {
                title = matcher.Groups["epNumber"].Value + " - " + title;
            }
            else if (title != "" && broadcastDate != "" && broadcastDate != title)
            {
                title = $"{broadcastDate.Replace("-", "")} ~ {title}";
            }
        }

        if (episode == "")
        {
            episode = $"1{month}{day}{ExtendId(file)}";
        }

        episode = (episode == "") ? int.Parse('1' + month + day).ToString() : episode;
        if (season == "" && year != "")
        {
            season = year;
        }

        EpisodeInfo item = new()
        {
            IndexNumber = int.Parse(episode),
            Name = title,
            Path = file,
            Year = "" != year ? int.Parse(year) : null,
            ParentIndexNumber = "" == season ? 1 : int.Parse(season)
        };

        item.SetProviderId(Constants.PLUGIN_EXTERNAL_ID, item.IndexNumber.ToString());

        // -- Set the PremiereDate if we have a year, month and day
        if (year != "" && month != "" && day != "")
        {
            item.PremiereDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
        }

        if (!item.IndexNumber.HasValue || item.IndexNumber.GetValueOrDefault() == 0)
        {
            Logger?.LogError($"No episode number found for '{file}'.");
            return new EpisodeInfo();
        }

        // -- Multiple episode handling.
        if (!item.PremiereDate.HasValue)
        {
            var match = Constants.MULTI_EPISODE_MATCHER.Match(filename);
            if (match.Success)
            {
                var matchIndexEnd = int.Parse(match.Groups["end"].Value);
                if (matchIndexEnd < item.IndexNumber)
                {
                    Logger?.LogError($"Invalid multi-episode range in '{filename}'.");
                }
                else
                {
                    item.IndexNumberEnd = matchIndexEnd;
                }
            }
        }

        var indexEnd = item.IndexNumberEnd.HasValue ? $"-E{item.IndexNumberEnd}" : "";
        Logger?.LogInformation($"Parsed '{Path.GetFileName(file)}' as 'S{item.ParentIndexNumber}E{item.IndexNumber}{indexEnd}': '{item.Name}'.");

        return item;
    }


    public static MetadataResult<Episode> ToEpisode(EpisodeInfo data, string file = null)
    {
        if (data.Path == "")
        {
            Logger?.LogInformation($"No metadata found for '{data.Path}'.");
            return ErrorOutEpisode();
        }

        Logger?.LogDebug($"Processing {data}.");

        Episode item = new()
        {
            Name = data.Name,
            IndexNumber = data.IndexNumber,
            IndexNumberEnd = data.IndexNumberEnd,
            ParentIndexNumber = data.ParentIndexNumber,
            Path = data.Path,
        };

        if (data.PremiereDate is DateTime time)
        {
            item.PremiereDate = time;
            item.ProductionYear = time.Year;
            item.SortName = $"{time:yyyyMMdd}-{item.IndexNumber} -{item.Name}";
            item.ForcedSortName = $"{time:yyyyMMdd}-{item.IndexNumber} -{item.Name}";
            item.ParentIndexNumber = time.Year;
        }

        if (string.IsNullOrEmpty(item.SortName))
        {
            item.SortName = $"{item.ParentIndexNumber:0000}{item.IndexNumber:0000} - {item.Name}";
            item.ForcedSortName = item.SortName;
        }

        data.TryGetProviderId(Constants.PLUGIN_EXTERNAL_ID, out string id);
        if (id != "")
        {
            item.SetProviderId(Constants.PLUGIN_EXTERNAL_ID, id);
        }

        if (!string.IsNullOrEmpty(file))
        {
            var nfoData = GetEpisodeNfo(file);
            if (nfoData.TryGetValue("title", out var nfo_title))
            {
                item.Name = nfo_title;
            }

            if (nfoData.TryGetValue("overview", out var nfo_overview))
            {
                item.Overview = nfo_overview;
            }

            if (nfoData.TryGetValue("aired", out var aired) && DateTime.TryParse(aired, out var date))
            {
                item.PremiereDate = date;
                item.ProductionYear = date.Year;
            }

            if (nfoData.TryGetValue("season", out var nfo_season) && int.TryParse(nfo_season, out var season))
            {
                item.ParentIndexNumber = season;
            }

            if (nfoData.TryGetValue("episode", out var nfo_episode) && int.TryParse(nfo_episode, out var episode))
            {
                item.IndexNumber = episode;
            }
        }

        return new() { HasMetadata = true, Item = item };
    }

    private static MetadataResult<Series> ErrorOut() => new() { HasMetadata = false, Item = new Series() };
    private static MetadataResult<Episode> ErrorOutEpisode() => new() { HasMetadata = false, Item = new Episode() };

    public static Dictionary<string, string> GetEpisodeNfo(string path)
    {
        var info = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(path))
        {
            return info;
        }

        try
        {
            string nfoPath = Path.ChangeExtension(path, ".nfo");

            if (!File.Exists(nfoPath))
            {
                return info;
            }

            Logger?.LogInformation($"GetEpisodeNfo() - nfoFile: {nfoPath}");

            string rawXml = File.ReadAllText(nfoPath);

            // Fix invalid ampersands
            rawXml = Regex.Replace(rawXml, @"&(?![A-Za-z]+[0-9]*;|#[0-9]+;|#x[0-9a-fA-F]+;)", "&amp;");
            // Remove junk self-closing tags on their own lines
            rawXml = Regex.Replace(rawXml, @"^\s*<.*/>\s*$", "", RegexOptions.Multiline);

            XElement root;
            try
            {
                var doc = XDocument.Parse(rawXml);
                root = doc.Descendants("episodedetails").FirstOrDefault();
                if (root == null)
                {
                    Logger?.LogError($"GetEpisodeNfo() - No <episodedetails> found in: {nfoPath}");
                    return info;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError($"GetEpisodeNfo() - Failed to parse XML: {ex.Message}");
                return info;
            }

            var keys = new List<(string xmlKey, string infoKey)>
            {
                ("title", "title"),
                ("season", "season"),
                ("episode", "episode"),
                ("aired", "aired"),
                ("plot", "overview")
            };

            foreach (var (xmlKey, infoKey) in keys)
            {
                var element = root.Element(xmlKey);
                if (element != null)
                {
                    string value = element.Value.Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        info[infoKey] = value;
                    }
                }
            }

            return info;
        }
        catch (Exception ex)
        {
            Logger?.LogError($"GetEpisodeNfo() - Exception: {ex.Message}");
            Logger?.LogError(ex.ToString());
            return info;
        }
    }
}
