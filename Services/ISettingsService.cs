using PseudoRun.Desktop.Models;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        Task SaveSettingsAsync(AppSettings settings);
    }
}
