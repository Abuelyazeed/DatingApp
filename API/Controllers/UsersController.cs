using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extentions;
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
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();
            
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
            
            if(await userRepository.SaveAllAsync()) return mapper.Map<PhotoDto>(photo);
            
            return BadRequest("Problem adding photo");
        }
    }
}
