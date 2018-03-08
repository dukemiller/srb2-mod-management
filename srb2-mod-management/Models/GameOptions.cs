using System;
using System.IO;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace srb2_mod_management.Models
{
    /// <summary>
    ///     All commandline options to pass to the executable.
    /// </summary>
    [Serializable]
    public class GameOptions: ObservableObject
    {
        private string _gameExe = "";

        private bool _openGl = true;

        private bool _noSound;

        private bool _noMusic;

        private bool _windowed;

        private bool _skipIntro;

        private bool _ultimateMode;

        // 


        /// <summary>
        ///     Path to the SRB2Win executable.
        /// </summary>
        [JsonProperty("game_exe")]
        public string GameExe
        {
            get => _gameExe;
            set => Set(() => GameExe, ref _gameExe, value);
        }

        /// <summary>
        ///     Flag for determining if OpenGL should be enabled.
        /// </summary>
        [JsonProperty("opengl")]
        public bool OpenGl
        {
            get => _openGl;
            set => Set(() => OpenGl, ref _openGl, value);
        }

        /// <summary>
        ///     Disable sound.
        /// </summary>
        [JsonProperty("no_sound")]
        public bool NoSound
        {
            get => _noSound;
            set => Set(() => NoSound, ref _noSound, value);
        }

        /// <summary>
        ///     Disable music.
        /// </summary>
        [JsonProperty("no_music")]
        public bool NoMusic
        {
            get => _noMusic;
            set => Set(() => NoMusic, ref _noMusic, value);
        }

        /// <summary>
        ///     Start in windowed mode.
        /// </summary>
        [JsonProperty("windowed")]
        public bool Windowed
        {
            get => _windowed;
            set => Set(() => Windowed, ref _windowed, value);
        }

        /// <summary>
        ///     Skip the opening intro.
        /// </summary>
        [JsonProperty("skip_intro")]
        public bool SkipIntro
        {
            get => _skipIntro;
            set => Set(() => SkipIntro, ref _skipIntro, value);
        }

        /// <summary>
        ///     Start the game in ultimate mode.
        /// </summary>
        [JsonProperty("ultimate_mode")]
        public bool UltimateMode
        {
            get => _ultimateMode;
            set => Set(() => UltimateMode, ref _ultimateMode, value);
        }

        /// <summary>
        ///     Path to the game folder.
        /// </summary>
        [JsonIgnore]
        public string GamePath => Path.GetDirectoryName(GameExe);
        
        /// <summary>
        ///     Build the commandline argument that would be passed to the executable.
        /// </summary>
        public string BuildArguments(IEnumerable<Mod> mods)
        {
            var command = string.Join(" ",
                mods.SelectMany(mod => mod.Files)
                    .Where(file => !file.Disabled && file.IsModFile)
                    .Select(file => $"\"{file.Path}\"")
                    .Distinct()
            );

            var arguments = $"-file {command}";

            if (OpenGl)
                arguments += " -opengl";

            if (NoSound)
                arguments += " -nosound";

            if (NoMusic)
                arguments += " -nomusic";

            if (Windowed)
                arguments += " -win";

            if (SkipIntro)
                arguments += " -skipintro";

            if (UltimateMode)
                arguments += " -ultimatemode";

            return arguments;
        }
    }
}