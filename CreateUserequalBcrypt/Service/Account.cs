using AutoMapper;
using CreateUserequalBcrypt.Data;
using CreateUserequalBcrypt.Constant;
using CreateUserequalBcrypt.Data.Login;
using CreateUserequalBcrypt.Dto.User;
using CreateUserequalBcrypt.IService.Login;
using CreateUserequalBcrypt.Model.AccountModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CreateUserequalBcrypt.Model.UserInfo;
using CreateUserequalBcrypt.IService.UserInfo;
using CreateUserequalBcrypt.Data.UserInfo;
using System.Security.Cryptography;

namespace CreateUserequalBcrypt.Service
{
    public class Account : IAccount
    {
        public Account(IConfiguration configuration , UserDbContext userDbContext )
        {
            _configuration = configuration;
            _userDbContext = userDbContext;
        }
        public static TokenModel  reFreshTokenModel = new TokenModel();
        public static UserResponse userResponse = new UserResponse();
        public static UserInfoFirst userInfoFirst = new UserInfoFirst(); 
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _userDbContext;
        

        public TokenModel Login(UserDto userDto)
        {

            string passwordHash = _userDbContext.accounts.FirstOrDefault(p => p.Username == userDto.UserName).PasswordHash;
            var password = BCrypt.Net.BCrypt.Verify(userDto.Password, passwordHash);
            userResponse.UserName = userDto.UserName;
            userResponse.PasswordHash = passwordHash;
            
            var token = CreateToken(userResponse);
            
            if (password)
            {
                
                return new TokenModel
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken
                };
            }
            return null;
            
        }

        public  UserResponse Register(UserInfoFirst userInfoFirst)
        {
            
            var password =  BCrypt.Net.BCrypt.HashPassword(userInfoFirst.Password ,Constant.Constant.PasswordSalt);
            var dataAccount = new AccountData
            {
                Username = userInfoFirst.UserName,
                PasswordHash = password
                
            };
            _userDbContext.Add(dataAccount);
            _userDbContext.SaveChanges();
            var idUser = _userDbContext.accounts.FirstOrDefault(c => c.Username == userInfoFirst.UserName).Id;

            var dataInfo = new UserInfoData
            {
                Phone = userInfoFirst.Phone,
                Address = userInfoFirst.Address,
                isAdmin = userInfoFirst.isAdmin,
                IdAccount = idUser
            };
            _userDbContext.Add(dataInfo);
            _userDbContext.SaveChanges();
            var user = new UserResponse
            {
                UserName = userInfoFirst.UserName,
                PasswordHash = password
            };
            return user;
        }

        




        //Create Token
        private TokenModel CreateToken(UserResponse userResponse)
        {
            var idAccount = IdAccount().Result;
            var user = _userDbContext.userInfoDatas.FirstOrDefault(c => c.IdAccount == idAccount);
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userResponse.UserName),
                new Claim(ClaimTypes.MobilePhone,user.Phone),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.StreetAddress, user.Address),
                new Claim("Id", user.ToString()),
                new Claim(ClaimTypes.Role, user.isAdmin.ToString())

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SecretKey").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims : claims,
                expires : DateTime.Now.AddDays(1),
                signingCredentials : cred
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();
            var RefreshTokenEntity = new RefreshTokenData
            {
                Id = Guid.NewGuid(),
                IdAccount = IdAccount().Result,
                JwtId =token.Id,
                Token = refreshToken,
                IsUsed =false,
                IsRevoked = false,
                IssueAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(1)
            };
            _userDbContext.Add(RefreshTokenEntity);
            _userDbContext.SaveChanges();
            return new TokenModel
            {
                AccessToken = jwt,
                RefreshToken = refreshToken
            };
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                string refreshToken = Convert.ToBase64String(randomNumber);
                return refreshToken;
            }
        }

        public ApiResponse RenewToken(TokenModel tokenModel)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SecretKey").Value);
            var tokenValidateParam = new TokenValidationParameters
            {
                //ký Token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SecretKey").Value)),
                // tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            try
            {
                //check 1 : Accesstoken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenModel.AccessToken,
                    tokenValidateParam,out var validatedToken);

                //check 2 : check alg
                if(validatedToken is JwtSecurityToken jwtSecurityToken )
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase);
                    if (!result) //false 
                    {
                        return new ApiResponse
                        {
                            
                            Success = false,
                            Message = "invalid token"
                        };
                    }
                }
                //check 3 : accesstoken expire? 
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired"
                    };
                }

                //check 4 :check refreshToken exist in Db
                var storedToken = _userDbContext.refreshTokens.FirstOrDefault(x => x.Token == tokenModel.RefreshToken); 
                if (storedToken == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "RefreshToken token doesn't exist"
                    };
                }

                //check 5 :refreshToken is used /revoked ?
                if (storedToken.IsUsed)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "RefreshToken token has been used"
                    };
                }

                if (storedToken.IsRevoked)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "RefreshToken token has been revoked "
                    };
                }

                //check 6 : AccessToken id == JwtID in refreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token don't matched "
                    };
                }

                //update store is used 
                storedToken.IsRevoked =true;
                storedToken.IsUsed =true;
                _userDbContext.Update(storedToken);
                _userDbContext.SaveChanges();

                // create Token 
                
                var token = CreateToken(userResponse);
                return new ApiResponse
                {
                    Success = true,
                    Message = "renew token success",
                    Data = token 
                };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "invalid token"
                };
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }








        //private ReFreshTokenModel GenerateRefreshToken()
        //{
        //    var refreshToken = new ReFreshTokenModel()
        //    {
        //        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        //        Expires = DateTime.Now.AddDays(1),
        //        Create = DateTime.Now

        //    };
        //    return refreshToken;
        //}

        //private void SetRefreshToken(ReFreshTokenModel reFreshTokenModel)
        //{
        //    var cookieOptions = new CookieOptions()
        //    {
        //        HttpOnly = true,
        //        Expires = reFreshTokenModel.Expires
        //    };

        //    reFreshTokenModel.Expires = reFreshTokenModel.Expires;
        //    reFreshTokenModel.Create = reFreshTokenModel.Create;
        //    reFreshTokenModel.Token = reFreshTokenModel.Token;

        //}


        // check userNameRequest have exist in database 
        public async Task<bool>  UserAlreadyExists(UserInfoFirst userInfoFirst)
        {
            return await _userDbContext.accounts.AnyAsync(p => p.Username == userInfoFirst.UserName);
        }

       
        public async Task<string> UserName()
        {
            return userResponse.UserName.ToString();
        }

        public async Task<bool> UserAlreadyExist(UserDto userDto)
        {
            return await _userDbContext.accounts.AnyAsync(p => p.Username == userDto.UserName);
        }

        public async Task<int> IdAccount ()
        {
            return _userDbContext.accounts.FirstOrDefault(c => c.Username == userResponse.UserName).Id;
        }

        
    }
}
