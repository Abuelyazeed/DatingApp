using System.Security.Claims;

namespace API.Extentions;

public static class ClaimsPrincipleExtentions
{
    public static string getUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.Name);
        
        if (username == null) throw new Exception("Cannot get username from token");
        
        return username;
    }
    
    public static int getUserId(this ClaimsPrincipal user)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new Exception("Cannot get user id from token"));

        return userId;
    }
}