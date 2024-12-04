using System.Security.Claims;

namespace API.Extentions;

public static class ClaimsPrincipleExtentions
{
    public static string getUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (username == null) throw new Exception("Cannot get username from token");
        
        return username;
    }
}