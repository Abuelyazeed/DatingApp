using API.Extentions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        
        var resultContext = await next();
        //After action has been executed

        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var userId = resultContext.HttpContext.User.getUserId();
        
        //Use dependency injection to get an instance of IUserRepository from the service container.
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        //Fetch user
        var user = await repo.GetUserByIdAsync(userId);
        if (user == null) return;
        user.LastActive = DateTime.Now;
        await repo.SaveAllAsync();
    }
}