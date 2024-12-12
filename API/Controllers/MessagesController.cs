using API.DTOs;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Message = API.Entities.Message;

namespace API.Controllers;

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
        
        if(recipient == null || sender == null) return BadRequest("Cannot send message");

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
}