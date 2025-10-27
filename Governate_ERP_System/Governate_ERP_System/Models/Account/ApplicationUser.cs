using Microsoft.AspNetCore.Identity;

namespace Governate_ERP_System.Models.Account
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
