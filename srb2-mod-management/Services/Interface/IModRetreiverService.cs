using System.Threading.Tasks;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;

namespace srb2_mod_management.Services.Interface
{
    /// <summary>
    ///     Retreive mods from an external source, giving paged results.
    /// </summary>
    public interface IModRetreiverService
    {
        /// <summary>
        ///     Request the paged list of content at page {page}.
        /// </summary>
        Task<Page> RequestPage(Category category, int page);

        /// <summary>
        ///     From the given release information, request the release.
        /// </summary>
        Task<Release> RequestRelease(ReleaseInfo release);

        /// <summary>
        ///     Find and replace the release matching {release}'s id with the arguments {release}.
        /// </summary>
        Task ReplaceRelease(Release release);

        /// <summary>
        ///     Attempt an update for the given releaseinfo.
        /// </summary>
        Task UpdateRelease(ReleaseInfo release);

        /// <summary>
        ///     Retrieve the matching release info for a given mod in a category.
        /// </summary>
        ReleaseInfo GetReleaseInfo(Mod mod, Category category);
    }
}