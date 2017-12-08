namespace srb2_mod_management.Repositories.Interface
{
    public interface ISettingsRepository
    {
        /// <summary>
        ///     Path to the game folder.
        /// </summary>
        string GamePath { get; set; }

        /// <summary>
        ///     Path to the SRB2Win executable.
        /// </summary>
        string GameExe { get; }

        /// <summary>
        ///     Flag for determining if OpenGL should be enabled.
        /// </summary>
        bool OpenGl { get; set; }

        /// <summary>
        ///     Disable sound.
        /// </summary>
        bool NoSound { get; set; }

        /// <summary>
        ///     Disable music.
        /// </summary>
        bool NoMusic { get; set; }

        /// <summary>
        ///     Save the repository to disk.
        /// </summary>
        void Save();

        /// <summary>
        ///     Discern whether the path to the executable is valid.
        /// </summary>
        /// <returns></returns>
        bool PathValid();
    }
}