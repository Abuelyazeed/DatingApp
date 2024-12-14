using API.Data;
using API.Entities;
using API.Extentions;
using API.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

using var serviceScope = app.Services.CreateScope(); //create new scope from the app's (DI) container
var services = serviceScope.ServiceProvider; //use the ServiceProvider to access services defined in the app's DI container
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync(); //look for migrations and updates the database
    await Seed.SeedUsers(userManager); //Add initial data

}
catch (Exception ex)
{
   var logger = services.GetRequiredService<ILogger<Program>>();
   logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();