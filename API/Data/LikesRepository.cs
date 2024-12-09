using API.DTOs;
using API.Entities;
using API.Helpers;
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

    public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
    {
        var likes = context.Likes.AsQueryable();
        IQueryable<MemberDto> query;

        switch (likesParams.Predicate)
        {
            case "liked":
                query = likes.Where(x => x.SourceUserId == likesParams.UserId)
                    .Select(u => u.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            case "likedby":
                query = likes.Where(x => x.TargetUserId == likesParams.UserId)
                    .Select(u => u.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            default: //get mutual likes
                var likedIds = await GetCurrentUserLikeIds(likesParams.UserId);  //users the user likes

                query = likes
                    .Where(x => x.TargetUserId == likesParams.UserId &&
                                likedIds.Contains(x.SourceUserId)) // gets mutual likes, likes user, and the user likes
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
        }
        
        return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
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