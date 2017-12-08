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

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("files")]
        public List<string> Files { get; set; } = new List<string>();

        [JsonProperty("changed_things")]
        public List<string> ChangedThings { get; set; } = new List<string>();

        [JsonProperty("highlighted")]
        public bool Highlighted
        {
            get => _highlighted;
            set => Set(() => Highlighted, ref _highlighted, value);
        }
    }
}