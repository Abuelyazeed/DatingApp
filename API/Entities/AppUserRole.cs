using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUserRole : IdentityUserRole<int>
{
    //This acts as a join table between the AppUser and AppRole
    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;

}