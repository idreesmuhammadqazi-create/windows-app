using PseudoRun.Desktop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IExamplesService
    {
        Task<List<Example>> GetAllExamplesAsync();
        Task<List<Example>> GetExamplesByCategoryAsync(string category);
        Task<List<Example>> GetExamplesByDifficultyAsync(string difficulty);
        Task<List<string>> GetAllCategoriesAsync();
        Task<List<string>> GetAllDifficultiesAsync();
    }
}
