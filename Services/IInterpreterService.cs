using PseudoRun.Desktop.Interpreter;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IInterpreterService
    {
        IAsyncEnumerable<string> ExecuteAsync(string code, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> ExecuteDebugAsync(string code, CancellationToken cancellationToken = default);
        void Stop();
    }
}
