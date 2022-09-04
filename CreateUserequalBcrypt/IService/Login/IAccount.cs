using CreateUserequalBcrypt.Dto.User;
using CreateUserequalBcrypt.Model.AccountModel;

namespace CreateUserequalBcrypt.IService.Login
{
    public interface IAccount
    {
        UserResponse Register(UserInfoFirst userInfoFirst) ;
        Token Login(UserDto userDto) ;
       
        Task<bool> UserAlreadyExists(UserInfoFirst userInfoFirst);
        Task<bool> UserAlreadyExist(UserDto userDto);
        Task<string> UserName();
        Task<int> IdAccount();
    }
}
