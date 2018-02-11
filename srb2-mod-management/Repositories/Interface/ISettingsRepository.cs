using srb2_mod_management.Models;

namespace srb2_mod_management.Repositories.Interface
{
    /// <summary>
    ///     A container of the settings for the user.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        ///     The game launch options.
        /// </summary>
        GameOptions Options { get; set; }

        /// <summary>
        ///     Save the repository to disk.
        /// </summary>
        void Save();

        /// <summary>
        ///     Discern whether the path to the executable is valid.
        /// </summary>
        bool PathValid();
    }
}