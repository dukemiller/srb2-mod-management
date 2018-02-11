using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    /// <summary>
    ///     The entire mod for srb2, encompassing details about itself and the files that it uses to work.
    /// </summary>
    [Serializable]
    public class Mod: ObservableObject
    {
        private bool _highlighted;

        private ObservableCollection<ModFile> _files = new ObservableCollection<ModFile>();

        private string _name;

        private List<string> _changedThings = new List<string>();

        // 

        /// <summary>
        ///     The name of the mod.
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set => Set(() => Name, ref _name, value);
        }

        /// <summary>
        ///     The unique identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        ///     Every file on disk associated with this mod.
        /// </summary>
        [JsonProperty("files")]
        public ObservableCollection<ModFile> Files
        {
            get => _files;
            set => Set(() => Files, ref _files, value);
        }

        /// <summary>
        ///     All files that will be added when launching.
        /// </summary>
        [JsonIgnore]
        public List<ModFile> ModFiles => Files.Where(file => file.IsModFile).ToList();

        /// <summary>
        ///     Everything that the mod states that it changes.
        /// </summary>
        [JsonProperty("changed_things")]
        public List<string> ChangedThings
        {
            get => _changedThings;
            set => Set(() => ChangedThings, ref _changedThings, value);
        }

        /// <summary>
        ///     Whether or not the mod is highlighted.
        /// </summary>
        [JsonProperty("highlighted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Highlighted
        {
            get => _highlighted;
            set => Set(() => Highlighted, ref _highlighted, value);
        }

        /// <summary>
        ///     If the mod was added locally or not.
        /// </summary>
        [JsonProperty("user_added", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsUserAdded { get; set; }
    }
}