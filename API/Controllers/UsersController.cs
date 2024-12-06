using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        
        [HttpGet] // users
        //We passed an object , so by default the api will look in the body
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUsername = User.getUsername();
            var users = await userRepository.GetMembersAsync(userParams);
            
            Response.AddPaginationHeader(users);
            
            return Ok(users);
        }
        
        [HttpGet("{username}")] // users/2
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);
                
            if(user == null) return NotFound();

            return user;
        }
        
        //No benefit in returning anything in a Http PUT method
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.getUsername());
            
            if(user == null) return BadRequest("Could not find user");
            
            mapper.Map(memberUpdateDto, user);

            if(await userRepository.SaveAllAsync()) return NoContent();
            
            return BadRequest("Failed to update the user");

        }

        
        //Post should return a 201 created not a 200 ok
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.getUsername());
            
            if(user == null) return BadRequest("Could update user");

            var result = await photoService.AddPhotoAsync(file);
            
            if(result.Error != null) return BadRequest(result.Error.Message);
            
            //Create the photo
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };
            
            //Add photo to user 
            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync())
                return CreatedAtAction(nameof(GetUser),
                    new { username = user.UserName },
                    mapper.Map<PhotoDto>(photo));
            
            return BadRequest("Problem adding photo");
        }

        
        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.getUsername());

            if(user == null) return BadRequest("Could not find user");
            
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            
            //Check is photo is unavailable or is current main
            if(photo == null || photo.IsMain) return BadRequest("Can not use this as main photo");
            
            //Get the current main photo and make it not the main
            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            
            if(currentMain != null) currentMain.IsMain = false;
            
            //Make photo main photo
            photo.IsMain = true;
            
            //Return no content in HTTP PUT
            if(await userRepository.SaveAllAsync()) return NoContent();
            
            return BadRequest("Problem setting main photo");
        }


        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.getUsername());
            
            if(user == null) return BadRequest("Could not find user");
            
            //get the photo
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            
            //check if photo is not available
            if(photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");
            
            //Delete photo from cloudinary
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                
                if(result.Error != null) return BadRequest(result.Error.Message);
            }
            
            //delete photo
            user.Photos.Remove(photo);
            
            //Return ok in a delete request
            if (await userRepository.SaveAllAsync()) return Ok();
            
            return BadRequest("Problem deleting photo");
        }
    }
}
