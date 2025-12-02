using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IInputService
    {
        Task<string> GetInputAsync(string variableName, string variableType);
    }
}
