using System;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    public class DownloadLink: IEquatable<DownloadLink>
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        // 

        public bool Equals(DownloadLink other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Filename, other.Filename) && string.Equals(Size, other.Size) && string.Equals(Link, other.Link);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DownloadLink) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Filename != null ? Filename.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Size != null ? Size.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Link != null ? Link.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}