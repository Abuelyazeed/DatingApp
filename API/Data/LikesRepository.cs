using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public async Task<UserLike?> GetUserLike(int sourceId, int targetId)
    {
        return await context.Likes.FindAsync(sourceId, targetId);
    }

    public async Task<IEnumerable<MemberDto>> GetUserLikes(string predicate, int userId)
    {
        var likes = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked":
                return await likes.Where(x => x.SourceUserId == userId)
                    .Select(u => u.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            case "likedby":
                return await likes.Where(x => x.TargetUserId == userId)
                    .Select(u => u.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            default: //get mutual likes
                var likedIds = await GetCurrentUserLikeIds(userId);  //users the user likes

                return await likes
                    .Where(x => x.TargetUserId == userId && likedIds.Contains(x.SourceUserId))// gets mutual likes, likes user, and the user likes
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
        }
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int userId)
    {
        return await context.Likes
            .Where(x => x.SourceUserId == userId)
            .Select(x => x.TargetUserId)
            .ToListAsync();
    }

    public void DeleteLike(UserLike like)
    {
        context.Likes.Remove(like);
    }

    public void AddLike(UserLike like)
    {
        context.Likes.Add(like);
    }

    public async Task<bool> SaveChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}