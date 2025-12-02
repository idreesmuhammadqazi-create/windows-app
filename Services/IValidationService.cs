using PseudoRun.Desktop.Validator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public interface IValidationService
    {
        Task<List<ValidationError>> ValidateAsync(string code);
    }
}
