using System.Threading.Tasks;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;

namespace srb2_mod_management.Services.Interface
{
    public interface IModRetreiverService
    {
        /// <summary>
        ///     Retrieve the paged list of content at page {page}.
        /// </summary>
        Task<Page> RetrievePage(Category category, int page);

        /// <summary>
        ///     From the given release information, return the release.
        /// </summary>
        Task<Release> RetrieveRelease(ReleaseInfo release);

        /// <summary>
        ///     Update the release via it's source.
        /// </summary>
        Task UpdateRelease(Release release);

        /// <summary>
        ///     Retrieve the matching release info for a given mod in a cateogry.
        /// </summary>
        ReleaseInfo GetReleaseInfo(Mod mod, Category category);
    }
}