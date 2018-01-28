using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    public class Release
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("screenshots")]
        public List<string> Screenshots { get; set; } = new List<string>();

        [JsonProperty("downloads")]
        public List<DownloadLink> Downloads { get; set; } = new List<DownloadLink>();

        [JsonProperty("changed_things")]
        public List<string> ChangedThings { get; set; } = new List<string>();

        [JsonProperty("update_available")]
        public bool UpdateAvailable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("released")]
        public DateTime Released { get; set; }

        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty("retrieved")]
        public DateTime Retrieved { get; set; }
    }
}