using API.DTOs;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Message = API.Entities.Message;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository,
    IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> Post(CreateMessageDto createMessageDto)
    {
        //username of sender
        var username = User.getUsername();
        
        if(username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot message yourself");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        
        if(recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) return BadRequest("Cannot send message");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        
        messageRepository.AddMessage(message);
        
        if(await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));
        
        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.getUsername();

        var messages = await messageRepository.GetMessagesForUser(messageParams);
        
        Response.AddPaginationHeader(messages);
        
        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username)
    {
        var currentUsername = User.getUsername();
        
        var messages = await messageRepository.GetMessagesThread(currentUsername, username);
        
        return Ok(messages);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.getUsername();
        
        var message = await messageRepository.GetMessage(id);
        
         if(message == null) return BadRequest("Cannot delete this message");
         
         if(message.SenderUsername != username || message.RecipientUsername != username)
             return Forbid("You cannot delete this message");
         
         if(message.SenderUsername == username) message.SenderDeleted = true;
         if(message.RecipientUsername == username) message.RecipientDeleted = true;

         if (message is { SenderDeleted: true, RecipientDeleted: true })
         {
             messageRepository.DeleteMessage(message);
         }

         if (await messageRepository.SaveAllAsync()) return Ok();
         
         return BadRequest("Problem deleting message");
    }
}