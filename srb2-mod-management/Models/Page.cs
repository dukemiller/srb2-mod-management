using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    /// <summary>
    ///     A single page (usually represented as a forum page) of ReleaseInfos.
    /// </summary>
    public class Page
    {
        [JsonProperty("release_descriptions")]
        public List<ReleaseInfo> Releases { get; set; } = new List<ReleaseInfo>();

        [JsonProperty("last_checked")]
        public DateTime LastChecked { get; set; }
    }
}