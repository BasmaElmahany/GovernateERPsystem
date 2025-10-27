using System.ComponentModel.DataAnnotations;

namespace Governate_ERP_System.Models.Account
{
    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
