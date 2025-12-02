using System.Collections.Generic;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IFileService
    {
        Task<string?> LoadProgramAsync(string filePath);
        Task SaveProgramAsync(string filePath, string code);
        List<string> GetRecentFiles();
        void AddRecentFile(string filePath);
        void ClearRecentFiles();
    }
}
