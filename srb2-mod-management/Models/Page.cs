using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    public class Page
    {
        [JsonProperty("release_descriptions")]
        public List<ReleaseInfo> Releases { get; set; } = new List<ReleaseInfo>();

        [JsonProperty("last_checked")]
        public DateTime LastChecked { get; set; }
    }
}