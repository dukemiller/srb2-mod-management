namespace srb2_mod_management.Repositories.Interface
{
    public interface ISettingsRepository
    {
        string GamePath { get; set; }
        string GameExe { get; }
        bool OpenGl { get; set; }
        void Save();
        bool PathValid();
    }
}