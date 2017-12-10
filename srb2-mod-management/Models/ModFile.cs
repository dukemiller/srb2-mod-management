using System;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    [Serializable]
    public class ModFile : ObservableObject
    {
        private bool _disabled;

        // 

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Disabled
        {
            get => _disabled;
            set => Set(() => Disabled, ref _disabled, value);
        }

        [JsonIgnore]
        public string Name => System.IO.Path.GetFileName(Path);

        [JsonIgnore]
        public bool IsModFile => new[] {".wad", ".lua"}.Any(ext => Path.ToLower().EndsWith(ext));
    }
}