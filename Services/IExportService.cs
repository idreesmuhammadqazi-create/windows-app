using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IExportService
    {
        Task ExportToPdfAsync(string code, string outputPath);
        Task ExportToDocxAsync(string code, string outputPath);
    }
}
