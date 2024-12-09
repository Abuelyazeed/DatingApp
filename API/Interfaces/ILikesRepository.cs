using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike?> GetUserLike(int sourceId, int targetId); //to check if one user has already liked another.
    Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams); //display all users liked by specific user and all users that like that specfic user
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int userId); //get ids of the users the user has liked
    void DeleteLike(UserLike like);
    void AddLike(UserLike like);
    Task<bool> SaveChanges();
}