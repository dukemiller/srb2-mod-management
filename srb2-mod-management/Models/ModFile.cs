using System;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    /// <summary>
    ///     A file that's associated with a Mod.
    /// </summary>
    [Serializable]
    public class ModFile : ObservableObject
    {
        private bool _disabled;

        // 
        
        /// <summary>
        ///     The entire path of the file, e.g. '{C:\...\Mod.wad}'
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        ///     User determined flag for Whether or not the file should be included on launch.
        /// </summary>
        [JsonProperty("disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Disabled
        {
            get => _disabled;
            set => Set(() => Disabled, ref _disabled, value);
        }

        /// <summary>
        ///     The filename excluding the path, e.g. 'C:\...\{Mod.wad}'
        /// </summary>
        [JsonIgnore]
        public string Name => System.IO.Path.GetFileName(Path);

        /// <summary>
        ///     Determining if the file is a file that should be included when launching the game.
        /// </summary>
        [JsonIgnore]
        public bool IsModFile => new[] {".wad", ".lua"}.Any(ext => Path.ToLower().EndsWith(ext));
    }
}