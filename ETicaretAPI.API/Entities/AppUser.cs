using Microsoft.AspNetCore.Identity;

namespace ETicaretAPI.API.Entities
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
