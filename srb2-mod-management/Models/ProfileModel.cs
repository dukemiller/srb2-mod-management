using srb2_mod_management.Enums;

namespace srb2_mod_management.Models
{
    public class ProfileModel
    {
        public ReleaseInfo ReleaseInfo { get; set; }
        public Category Category { get; set; }
        public bool Refresh { get; set; }
    }
}