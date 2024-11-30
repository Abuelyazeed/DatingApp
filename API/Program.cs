using API.Data;
using API.Extentions;
using API.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAppilicationService(builder.Configuration);
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
    await context.Database.MigrateAsync(); //look for migrations and updates the database
    await Seed.SeedUsers(context); //Add initial data

}
catch (Exception ex)
{
   var logger = services.GetRequiredService<ILogger<Program>>();
   logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();