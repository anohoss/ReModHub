using System.Threading.Tasks;
using System.Windows;
using ReModHub.Settings;

namespace ReModHub.Appearance
{
    public interface IThemeCoordinator
    {
        void Initialize(Window mainWindow);
        Task ApplyAsync(AppSettings settings);
    }
}
