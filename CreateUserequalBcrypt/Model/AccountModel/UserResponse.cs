using CreateUserequalBcrypt.Dto.User;

namespace CreateUserequalBcrypt.Model.AccountModel
{
    public class UserResponse
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    
    }
}
