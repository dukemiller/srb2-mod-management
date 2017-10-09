using System.Collections.ObjectModel;
using System.Threading.Tasks;
using srb2_mod_management.Models;
using Category = srb2_mod_management.Enums.Category;

namespace srb2_mod_management.Repositories.Interface
{
    public interface IDownloadedModsRepository
    {
        ObservableCollection<Mod> Levels { get; }
        ObservableCollection<Mod> Characters { get; }
        ObservableCollection<Mod> Mods { get; }
        bool AlreadyContains(Category category, Release release);
        bool AlreadyContains(Category category, int id);
        Task Add(Category category, Mod mod);
        Task Remove(Category category, Mod mod);
    }
}