using System.ComponentModel.DataAnnotations;

namespace Governate_ERP_System.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)] public string OldPassword { get; set; } = "";
        [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = "";
        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = "";
    }
}
