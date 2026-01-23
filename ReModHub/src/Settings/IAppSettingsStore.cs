using System.Threading.Tasks;

namespace ReModHub.Settings
{
    public interface IAppSettingsStore
    {
        Task<AppSettings> LoadAsync();
        Task SaveAsync(AppSettings settings);
    }
}
