using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    [Serializable]
    public class Mod: ObservableObject
    {
        private bool _highlighted;

        private List<ModFile> _files = new List<ModFile>();

        // 

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("files")]
        public List<ModFile> Files
        {
            get => _files;
            set => Set(() => Files, ref _files, value);
        }

        [JsonProperty("changed_things")]
        public List<string> ChangedThings { get; set; } = new List<string>();

        [JsonProperty("highlighted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Highlighted
        {
            get => _highlighted;
            set => Set(() => Highlighted, ref _highlighted, value);
        }
    }
}