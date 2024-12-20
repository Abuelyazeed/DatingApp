using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;
        
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        
        //We need to convert the text to a c# class, so we will use the json deserialize
        
        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        if (users == null) return;

        foreach (var user in users)
        {
            // using var hmac = new HMACSHA512();
            
            // user.UserName = user.UserName.ToLower();
            // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
            // user.PasswordSalt = hmac.Key;

            // context.Users.Add(user);
            
            await userManager.CreateAsync(user, "Pa$$w0rd");
        }
        // await context.SaveChangesAsync();
        
    }
}