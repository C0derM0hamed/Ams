using Microsoft.AspNetCore.Identity;

namespace AmsApi.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; }
    }
}
