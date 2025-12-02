using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PseudoRun.Desktop.Interpreter;

namespace PseudoRun.Desktop.Validator
{
    public class SyntaxValidator
    {
        public List<ValidationError> Validate(string code)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(code))
            {
                return errors;
            }

            try
            {
                // Tokenize
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(code);

                // Parse
                var parser = new Parser(tokens);
                parser.Parse();

                // If we get here, code is syntactically valid
                return errors;
            }
            catch (Exception error)
            {
                // Extract line number from error message if present
                var errorMessage = error.Message;
                int line = 1;

                var lineMatch = Regex.Match(errorMessage, @"line (\d+)");
                if (lineMatch.Success)
                {
                    line = int.Parse(lineMatch.Groups[1].Value);
                }

                errors.Add(new ValidationError
                {
                    Line = line,
                    Message = errorMessage,
                    Type = ErrorType.Syntax
                });

                return errors;
            }
        }
    }
}
