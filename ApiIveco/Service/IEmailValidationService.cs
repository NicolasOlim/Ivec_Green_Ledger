using System.Threading.Tasks;

namespace ApiIveco.Services
{
    public interface IEmailValidationService
    {
        Task<(bool isValid, string message)> ValidateEmailAsync(string email);
    }
}