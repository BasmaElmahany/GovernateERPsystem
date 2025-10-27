using Microsoft.AspNetCore.Identity;


namespace Governate_ERP_System.Infrastructure.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
