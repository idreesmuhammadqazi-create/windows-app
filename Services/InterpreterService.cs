using PseudoRun.Desktop.Interpreter;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public class InterpreterService : IInterpreterService
    {
        private CancellationTokenSource? _cancellationTokenSource;

        public async IAsyncEnumerable<string> ExecuteAsync(
            string code,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                // Tokenize
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(code);

                // Parse
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                // Execute
                var interpreter = new PseudocodeInterpreter();

                await foreach (var output in interpreter.Execute(ast, _cancellationTokenSource.Token))
                {
                    yield return output;
                }
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public async IAsyncEnumerable<string> ExecuteDebugAsync(
            string code,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                // Tokenize
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(code);

                // Parse
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                // Execute in debug mode
                var interpreter = new PseudocodeInterpreter(debugMode: true);

                await foreach (var output in interpreter.Execute(ast, _cancellationTokenSource.Token))
                {
                    yield return output;
                }
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
