using Domain.DTO;

namespace Bll.Interfaces;

public interface IUserProfileService
{
    Task<UserInfoDto> GetUserInfoAsync(long userId, long chatId, CancellationToken token);
    
    Task<string> GetUserLexiconPageAsync(long userId, long chatId, int offset, CancellationToken token);
}