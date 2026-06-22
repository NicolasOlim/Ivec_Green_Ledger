namespace ApiIveco.DTO
{
    /// <summary>
    /// DTO para a requisição de login.
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}