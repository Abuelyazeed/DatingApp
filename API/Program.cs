using API.Extentions;
using API.Middleware;
using Microsoft.AspNetCore.Diagnostics;


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

app.Run();