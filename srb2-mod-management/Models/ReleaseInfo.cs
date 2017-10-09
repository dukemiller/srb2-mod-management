using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    public class ReleaseInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("stars")]
        public int Stars { get; set; }

        [JsonIgnore]
        public ObservableCollection<Star> StarCollection
        {
            get
            {
                var stars = new ObservableCollection<Star>();
                foreach (var _ in Enumerable.Range(0, Stars))
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