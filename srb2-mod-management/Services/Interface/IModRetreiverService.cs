using System.Threading.Tasks;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;

namespace srb2_mod_management.Services.Interface
{
    public interface IModRetreiverService
    {
        Task<Page> RetrievePage(Category category, int page);
        Task<Release> RetrieveRelease(ReleaseInfo release);
        Task UpdateRelease(Release release);
        ReleaseInfo GetReleaseInfo(Mod mod, Category category);
    }
}