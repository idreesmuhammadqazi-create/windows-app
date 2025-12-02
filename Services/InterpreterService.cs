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
        private readonly IInputService _inputService;

        public InterpreterService(IInputService inputService)
        {
            _inputService = inputService;
        }

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

                // Execute with input handler
                var interpreter = new PseudocodeInterpreter(
                    inputHandler: async (varName, varType) => await _inputService.GetInputAsync(varName, varType));

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

                // Execute in debug mode with input handler
                var interpreter = new PseudocodeInterpreter(
                    inputHandler: async (varName, varType) => await _inputService.GetInputAsync(varName, varType),
                    debugMode: true);

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
