using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace srb2_mod_management.Models
{
    [Serializable]
    public class Mod: ObservableObject
    {
        private bool _highlighted;

        private ObservableCollection<ModFile> _files = new ObservableCollection<ModFile>();

        private string _name;

        private List<string> _changedThings = new List<string>();

        // 

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set => Set(() => Name, ref _name, value);
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("files")]
        public ObservableCollection<ModFile> Files
        {
            get => _files;
            set => Set(() => Files, ref _files, value);
        }

        [JsonIgnore]
        public List<ModFile> ModFiles => Files.Where(file => file.IsModFile).ToList();

        [JsonProperty("changed_things")]
        public List<string> ChangedThings
        {
            get => _changedThings;
            set => Set(() => ChangedThings, ref _changedThings, value);
        }

        [JsonProperty("highlighted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Highlighted
        {
            get => _highlighted;
            set => Set(() => Highlighted, ref _highlighted, value);
        }

        [JsonProperty("user_added", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsUserAdded { get; set; }
    }
}