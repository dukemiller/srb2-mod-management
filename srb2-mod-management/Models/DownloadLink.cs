using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    public class DownloadLink
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}