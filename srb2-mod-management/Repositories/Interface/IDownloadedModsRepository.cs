using System.Collections.ObjectModel;
using System.Threading.Tasks;
using srb2_mod_management.Models;
using Category = srb2_mod_management.Enums.Category;

namespace srb2_mod_management.Repositories.Interface
{
    public interface IDownloadedModsRepository
    {

        /// <summary>
        ///     The user downloaded level packs.
        /// </summary>
        ObservableCollection<Mod> Levels { get; }

        /// <summary>
        ///     The user downloaded characters.
        /// </summary>
        ObservableCollection<Mod> Characters { get; }

        /// <summary>
        ///     The user downloaded full mod packs.
        /// </summary>
        ObservableCollection<Mod> Mods { get; }

        /// <summary>
        ///     The user downloaded scripts.
        /// </summary>
        ObservableCollection<Mod> Scripts { get; }

        /// <summary>
        ///     Determine if a category already contains a specific type of release.
        /// </summary>
        bool Contains(Category category, Release release);

        /// <summary>
        ///     Determine if a category contains an item that has a specific id.
        /// </summary>
        bool Contains(Category category, int id);

        /// <summary>
        ///     Add the given mod via the category.
        /// </summary>
        Task Add(Category category, Mod mod);

        /// <summary>
        ///     Remove the given mod from the category.
        /// </summary>
        Task Remove(Category category, Mod mod);

        /// <summary>
        ///     Retrieve the mod corresponding to a release or id (if downloaded).
        /// </summary>
        Mod Find(Category category, ReleaseInfo info);
        Mod Find(int id);

        /// <summary>
        ///     Save to disk.
        /// </summary>
        Task Save();
    }
}