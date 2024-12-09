using API.Data;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        //Get sourceuserid
        var sourceUserId = User.getUserId();
        
        //User trying to like himself
        if(sourceUserId == targetUserId) return BadRequest("You can not like yourself");

        var existingLike = await likesRepository.GetUserLike(sourceUserId, targetUserId);

        if (existingLike != null)
        {
            likesRepository.DeleteLike(existingLike);
        }
        else
        {
            likesRepository.AddLike(new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            });
        }
        
        if(await likesRepository.SaveChanges()) return Ok();
        
        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        var userId = User.getUserId();

        return Ok(await likesRepository.GetCurrentUserLikeIds(userId));
    }

    [HttpGet] //Will look for predicate in the query string
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes(string predicate)
    {
        var userId = User.getUserId();
        
        return Ok(await likesRepository.GetUserLikes(predicate, userId));
    }
}