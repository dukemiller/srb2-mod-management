using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    [Serializable]
    public class Mod: ObservableObject
    {
        private bool _promoted;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("files")]
        public List<string> Files { get; set; } = new List<string>();

        [JsonProperty("changed_things")]
        public List<string> ChangedThings { get; set; } = new List<string>();

        [JsonProperty("promoted")]
        public bool Promoted
        {
            get => _promoted;
            set => Set(() => Promoted, ref _promoted, value);
        }
    }
}