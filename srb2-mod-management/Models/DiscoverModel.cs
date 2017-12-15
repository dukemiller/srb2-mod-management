using srb2_mod_management.Enums;

namespace srb2_mod_management.Models
{
    public class DiscoverModel
    {
        public Category Category { get; set; }
        public ReleaseInfo ReleaseInfo { get; set; }
        public ComponentView RequestedView { get; set; }
    }
}