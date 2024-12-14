using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) :IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false), //Messages where the user is the recipient.
            "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false), //Messages where the user is the sender.
            _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false), //Messages where the user is the recipient.(unread)
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
        
        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesThread(string currentUsername, string recepientUsername)
    {
        var messages = await context.Messages
            .Include(x => x.Sender).ThenInclude(x =>x.Photos) //Join messages with sender, then with its photos
            .Include(x => x.Recipient).ThenInclude(x =>x.Photos)//Join messages with recipient then with its photos
            .Where(x => x.RecipientUsername == currentUsername
                        && x.RecipientDeleted == false 
                        && x.SenderUsername == recepientUsername || //Get messages sent to current user
                        x.SenderUsername == currentUsername 
                        && x.SenderDeleted == false 
                        && x.RecipientUsername == recepientUsername) //Get messages sent by current user
            .OrderBy(x => x.MessageSent)
            .ToListAsync();
        
        var unreadMessages = messages.Where(x => x.DateRead == null && x.RecipientUsername == currentUsername)
            .ToList(); //Get unread messages

        if (unreadMessages != null)
        {
            unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow); //Mark unread messages as read
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}