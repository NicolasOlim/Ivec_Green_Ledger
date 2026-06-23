using System;
using System.Threading.Tasks;

namespace ApiIveco.Services
{
    public class EmailValidationService : IEmailValidationService
    {
        public Task<(bool isValid, string message)> ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult((false, "O e-mail é obrigatório."));

            // Remove espaços e verifica se termina com @iveco.com (case-insensitive)
            email = email.Trim();
            if (email.EndsWith("@iveco.com", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult((true, "E-mail válido (domínio IVECO)."));
            }
            else
            {
                return Task.FromResult((false, "Apenas e-mails com domínio @iveco.com são permitidos."));
            }
        }
    }
}