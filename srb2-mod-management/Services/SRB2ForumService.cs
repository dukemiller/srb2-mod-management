using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories;
using srb2_mod_management.Repositories.Interface;
using srb2_mod_management.Services.Interface;

namespace srb2_mod_management.Services
{
    public class Srb2ForumService : IModRetreiverService
    {
        private readonly IDownloadedModsRepository _modsRepository;
        private static readonly HttpClient Client = new HttpClient();
        private readonly ForumData _data;

        // 

        static Srb2ForumService()
        {
            Client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/46.0");
        }

        public Srb2ForumService(IDownloadedModsRepository modsRepository)
        {
            _modsRepository = modsRepository;
            _data = ForumData.Load();
        }

        // 

        public async Task<Page> RetrievePage(Category category, int page)
        {
            var retrievedPage = await _data.FindPage(category, page, () => GetPage(category, page));
            foreach(var release in retrievedPage.Releases)
                release.AlreadyDownloaded = _modsRepository.AlreadyContains(category, release.Id);
            return retrievedPage;
        }

        public async Task<Release> RetrieveRelease(ReleaseInfo release)
        {
            return await _data.FindRelease(release.Url, () => GetRelease(release.Url));
        }

        public async Task UpdateRelease(Release release) => await _data.UpdateRelease(release);

        public ReleaseInfo GetReleaseInfo(Mod mod, Category category)
        {
            var categoryKey = category.ToString().ToLower();
            return _data.Pages.ContainsKey(categoryKey)
                ? _data.Pages[categoryKey].Keys
                    .Select(key => _data.Pages[categoryKey][key])
                    .SelectMany(page => page.Releases)
                    .FirstOrDefault(releaseinfo => releaseinfo.Id == mod.Id)
                : null;
        }

        //

        private static async Task<Page> GetPage(Category category, int page)
        {
            var urlEnd = ((int) category).ToString();
            var url = $"https://mb.srb2.org/forumdisplay.php?f={urlEnd}";
            if (page+1 > 1)
                url += $"&order=desc&page={page+1}";

            var request = await Client.GetAsync(url);
            var response = await request.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(response);

            return new Page
            {
                LastChecked = DateTime.Now,
                Releases = document.DocumentNode
                    .SelectNodes("//table/tbody/tr").Skip(1)
                    .TakeWhile(n => !n.InnerText.Contains("Forum Rules"))
                    .Select(ToReleaseInfo).ToList()
            };
        }

        private static async Task<Release> GetRelease(string url)
        {
            var request = await Client.GetAsync(url);
            var response = await request.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(response);
            return ToRelease(url, document.DocumentNode.SelectSingleNode("//div[@class=\"page\"]/div/div/table/tbody[1]/tr/td/div"));
        }

        private static DateTime ParseCustomReplyTime(string text)
        {
            var format = text.Split(' ');
            var (amount, quantifier) = (int.TryParse(format[0], out var number) ? number : 0, format[1].ToLower());
            var date = DateTime.Now;
            switch (quantifier)
            {
                case "hours":
                    date = date.AddHours(-amount);
                    break;
                case "days":
                    date = date.AddDays(-amount);
                    break;
                case "weeks":
                    date = date.AddDays(-(amount * 7));
                    break;
            }
            return date;
        }

        private static ReleaseInfo ToReleaseInfo(HtmlNode node)
        {
            var name = CleanString(node.Descendants("a").First(a => a.Id.Contains("thread_title_")).InnerText);
            var views = int.TryParse(node.Descendants("td").Last().InnerText, NumberStyles.AllowThousands, new NumberFormatInfo(), out var number)
                ? number 
                : 0;

            var dateText = node.SelectSingleNode(".//td[@class='alt2']/div[@class='smallfont']/br")?.PreviousSibling
                ?.InnerText?.Trim();
            var date = dateText == null
                ? DateTime.Now
                : (dateText.Contains("Ago")
                        ? ParseCustomReplyTime(dateText)
                        : DateTime.Parse(dateText));

            var url = CleanUrl(node.SelectSingleNode(".//a[contains(@href, 'showthread')]").Attributes["href"].Value);
            var id = int.Parse(url.Split('=').Last());
            var starNode = node.SelectSingleNode(".//img[contains(@src, 'rating')]");
            var stars = starNode == null
                ? 0
                : int.Parse(Regex.Match(starNode.Attributes["src"].Value, "([1-5])").Groups[1].Value);

            return new ReleaseInfo
            {
                Id = id,
                LastReply = date,
                Name = name,
                Stars = stars,
                Url = url,
                Views = views
            };
        }
        
        private static Release ToRelease(string url, HtmlNode node)
        {
            var id = int.Parse(url.Split('=').Last());

            var descriptionNode = node.SelectSingleNode(".//div[@id='moddesc']");

            var name = CleanString(node
                .SelectSingleNode(".//div/strong")
                .InnerText);

            var description = CleanString(descriptionNode
                .SelectSingleNode(".//div[contains(@id, 'post_message')]")
                .InnerText);

            var released = DateTime.Parse(
                node.SelectNodes(".//td[@class='alt2']")
                    .Skip(3).First()
                    .SelectSingleNode(".//strong/following-sibling::text()").InnerText.Split(' ')[1]
            );

            var screenshots = node
                .SelectNodes(".//img[@class='thumbnail' or boolean(@onload)]")
                ?.Select(img => CleanUrl(img.Attributes["src"].Value))
                .ToList() ?? new List<string>();
            
            var changedThings = node.SelectNodes(".//span//img/following-sibling::text()").Select(text => CleanString(text.InnerText)).ToList();

            var updateText = node.SelectSingleNode(".//table/tr[2]/td[2]/strong/following-sibling::text()").InnerText.Split(' ')[1];
            var updated = updateText.ToLower() == "never"
                ? released
                : DateTime.TryParse(updateText, out var updateDate)
                    ? updateDate
                    : released;

            var links = descriptionNode.SelectNodes(".//table[@cellspacing='3']/tr").Select(tr =>
                new DownloadLink
                {
                    Filename = CleanString(tr.SelectSingleNode(".//a").InnerText),
                    Size = CleanString(tr.SelectSingleNode(".//a").NextSibling.InnerText).Split(',')[0] + ")",
                    Link = CleanUrl(tr.SelectSingleNode(".//a").Attributes["href"].Value)
                }).ToList();

            return new Release
            {
                Id = id,
                Name = name,
                Description = description,
                Downloads = links,
                Released = released,
                LastUpdated = updated,
                Screenshots = screenshots,
                ChangedThings = changedThings,
                Retrieved = DateTime.Now
            };
        }

        private static string CleanUrl(string content)
        {
            if (content == null)
                return null;
            content = HttpUtility.HtmlDecode(content).TrimStart();
            content = Regex.Replace(content, @"(?:(s=\w+)&)|(&thumb=1)", "");
            if (!content.StartsWith("http"))
                content = "https://mb.srb2.org/" + content;
            return content;
        }

        private static string CleanString(string content)
        {
            if (content == null)
                return null;
            content = HttpUtility.HtmlDecode(content);
            content = content.TrimStart('\n').Trim();
            content = Regex.Replace(content, @"([[<](\w+)[]>].*[[<]\/\2[]>])", "");
            return content;
        }

    }

    [Serializable]
    internal class ForumData
    {
        // {category: {page number: Page}}
        [JsonProperty("pages")]
        public Dictionary<string, Dictionary<int, Page>> Pages { get; set; } =
            new Dictionary<string, Dictionary<int, Page>>();

        // {url: Release}
        [JsonProperty("releases")]
        public Dictionary<string, Release> Releases { get; set; } = new Dictionary<string, Release>();

        [JsonIgnore]
        private static readonly string SettingsPath =
            Path.Combine(SettingsRepository.ApplicationDirectory, "forum_data.json");

        public async Task<Page> FindPage(Category category, int page, Func<Task<Page>> getPage)
        {
            var key = category.ToString().ToLower();

            if (!Pages.ContainsKey(key))
                Pages[key] = new Dictionary<int, Page>();

            if (!Pages[key].ContainsKey(page))
                Pages[key][page] = new Page();

            var data = Pages[key][page];

            if (data.Releases.Count == 0 || (DateTime.Now  - data.LastChecked).Days > 14)
            {
                Pages[key][page] = await getPage();
                Pages[key][page].LastChecked = DateTime.Now;
                await Save();
            }

            return Pages[key][page];
        }

        public async Task<Release> FindRelease(string url, Func<Task<Release>> getRelease)
        {

            // No release, retrieve it
            if (!Releases.ContainsKey(url))
            {
                Releases[url] = await getRelease();
                await Save();
            }

            // Potentially old, update it
            if ((DateTime.Now - Releases[url].Retrieved).Days > 30)
            {
                Releases[url] = await getRelease();
                await Save();
            }

            return Releases[url];
        }

        public async Task UpdateRelease(Release release)
        {
            var key = Releases.Keys.First(releaseUrl => releaseUrl.Contains(release.Id.ToString()));
            Releases[key] = release;
            await Save();
        }

        public async Task Save()
        {
            using (var stream = new StreamWriter(SettingsPath))
                await stream.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ForumData Load()
        {
            if (File.Exists(SettingsPath))
                using (var stream = new StreamReader(SettingsPath))
                    return JsonConvert.DeserializeObject<ForumData>(stream.ReadToEnd());

            return new ForumData();
        }
    }
}