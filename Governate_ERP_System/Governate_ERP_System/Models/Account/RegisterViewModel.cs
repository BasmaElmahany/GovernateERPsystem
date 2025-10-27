using System.ComponentModel.DataAnnotations;

namespace Governate_ERP_System.Models.Account
{
    public class RegisterViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string DisplayName { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = "";
        public string? ReturnUrl { get; set; }
    }
}
