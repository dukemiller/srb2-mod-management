using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories.Interface;
using Category = srb2_mod_management.Enums.Category;

namespace srb2_mod_management.Repositories
{
    [Serializable]
    public class DownloadedModsRepository : IDownloadedModsRepository
    {
        public static string SettingsPath => Path.Combine(SettingsRepository.ApplicationDirectory,
            "downloaded_mods.json");

        [JsonProperty("levels")]
        public ObservableCollection<Mod> Levels { get; set; } = new ObservableCollection<Mod>();

        [JsonProperty("characters")]
        public ObservableCollection<Mod> Characters { get; set; } = new ObservableCollection<Mod>();

        [JsonProperty("mods")]
        public ObservableCollection<Mod> Mods { get; set; } = new ObservableCollection<Mod>();

        [JsonProperty("scripts")]
        public ObservableCollection<Mod> Scripts { get; } = new ObservableCollection<Mod>();

        public bool AlreadyContains(Category category, Release release) => GetCollectionForCategory(category).Any(mod => mod?.Id == release.Id);

        public bool AlreadyContains(Category category, int id) => GetCollectionForCategory(category).Any(mod => mod?.Id == id);

        public bool AlreadyContains(Category category, ReleaseInfo release) => GetCollectionForCategory(category).Any(mod => mod.Id == release.Id);

        public async Task Add(Category category, Mod mod)
        {
            GetCollectionForCategory(category).Add(mod);
            await Save();
        }

        public async Task Remove(Category category, Mod mod)
        {
            var collection = GetCollectionForCategory(category);
            collection.Remove(collection.First(m => m.Id == mod.Id));
            await Save();
        }

        private ObservableCollection<Mod> GetCollectionForCategory(Category category)
        {
            switch (category)
            {
                
                default:
                case Category.Level:
                    return Levels;
                case Category.Character:
                    return Characters;
                case Category.Mod:
                    return Mods;
                case Category.Script:
                    return Scripts;
            }
        }

        public async Task Save()
        {
            using (var stream = new StreamWriter(SettingsPath))
                await stream.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static DownloadedModsRepository Load()
        {
            if (File.Exists(SettingsPath))
                using (var stream = new StreamReader(SettingsPath))
                    return JsonConvert.DeserializeObject<DownloadedModsRepository>(stream.ReadToEnd());

            return new DownloadedModsRepository();
        }
    }
}