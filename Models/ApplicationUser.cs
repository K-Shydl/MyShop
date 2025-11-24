using Microsoft.AspNetCore.Identity;

namespace MyShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        // додаткові поля (за потреби)
        public decimal Balance { get; set; } = 50m;
        public string? FullName { get; set; }
    }
}
