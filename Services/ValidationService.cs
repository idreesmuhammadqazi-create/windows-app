using PseudoRun.Desktop.Validator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public class ValidationService : IValidationService
    {
        private readonly SyntaxValidator _validator;

        public ValidationService()
        {
            _validator = new SyntaxValidator();
        }

        public Task<List<ValidationError>> ValidateAsync(string code)
        {
            return Task.Run(() => _validator.Validate(code));
        }
    }
}
