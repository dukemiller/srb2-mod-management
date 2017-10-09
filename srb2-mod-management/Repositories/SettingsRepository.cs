using System;
using System.IO;
using Newtonsoft.Json;
using srb2_mod_management.Repositories.Interface;

namespace srb2_mod_management.Repositories
{
    public class SettingsRepository: ISettingsRepository
    {
        /// <summary>
        ///     The path to the folder containing all settings and configuration files.
        /// </summary>
        [JsonIgnore]
        public static string ApplicationDirectory => Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "srb2_mod_management");

        [JsonIgnore]
        public static string SettingsPath => Path.Combine(ApplicationDirectory, "settings.json");

        [JsonIgnore]
        public string GameExe => Path.Combine(GamePath, "srb2win.exe");

        [JsonProperty("game_path")]
        public string GamePath { get; set; } = "";

        public bool PathValid() => File.Exists(GameExe);

        public void Save()
        {
            using (var stream = new StreamWriter(SettingsPath))
                stream.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static SettingsRepository Load()
        {
            if (!Directory.Exists(ApplicationDirectory))
                Directory.CreateDirectory(ApplicationDirectory);

            if (File.Exists(SettingsPath))
                using (var stream = new StreamReader(SettingsPath))
                    return JsonConvert.DeserializeObject<SettingsRepository>(stream.ReadToEnd());

            return new SettingsRepository();
        }
    }
}