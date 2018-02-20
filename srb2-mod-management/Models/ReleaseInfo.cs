using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    /// <summary>
    ///     Meta information about a release, retrieved from a listing source.
    /// </summary>
    public class ReleaseInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; } = -1;

        [JsonIgnore]
        public ObservableCollection<Star> StarCollection
        {
            get
            {
                var stars = new ObservableCollection<Star>();
                foreach (var _ in Enumerable.Range(0, (int) Math.Round(Rating)))
                    stars.Add(new Star());
                return stars;
            }
        }

        [JsonProperty("last_updated")]
        public DateTime LastReply { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonIgnore]
        public bool AlreadyDownloaded { get; set; }
    }
}