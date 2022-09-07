//using CreateUserequalBcrypt.Data;
//using CreateUserequalBcrypt.IService.Login;
//using System.Security.Cryptography;

//namespace CreateUserequalBcrypt.Service
//{
//    public class RefreshToken : IRefreshToken
//    {
//        private readonly UserDbContext _userDbContext;

//        public RefreshToken(UserDbContext userDbContext)
//        {
//            _userDbContext = userDbContext;
//        }
//        public string CreateToken(string username)
//        {
//            var randomNumber = new byte[32];
//            using (var randomNumberGenerator = RandomNumberGenerator.Create())
//            {
//                randomNumberGenerator.GetBytes(randomNumber);
//                string refreshToken = Convert.ToBase64String(randomNumber);
//                return refreshToken;
//            }
//        }
//    }
//}
