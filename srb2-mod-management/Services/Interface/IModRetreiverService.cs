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
        ///     Replace the release.
        /// </summary>
        Task ReplaceRelease(Release release);

        Task UpdateRelease(ReleaseInfo release);

        /// <summary>
        ///     Retrieve the matching release info for a given mod in a cateogry.
        /// </summary>
        ReleaseInfo GetReleaseInfo(Mod mod, Category category);
    }
}