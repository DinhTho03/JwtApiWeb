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
        public static ReFreshTokenModel  reFreshTokenModel = new ReFreshTokenModel();
        public static UserResponse userResponse = new UserResponse();
        public static UserInfoFirst userInfoFirst = new UserInfoFirst(); 
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _userDbContext;
        

        public Token Login(UserDto userDto)
        {
             
            var passwordHash = _userDbContext.accounts.FirstOrDefault(p => p.Username == userDto.UserName).PasswordHash;
            var password = BCrypt.Net.BCrypt.Verify(userDto.Password, passwordHash);
            userResponse.UserName = userDto.UserName;
            userResponse.PasswordHash = passwordHash;
            
            var token = CreateToken(userResponse);
            
            if (password)
            {
                
                return new Token
                {
                    TokenCreated = token
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
        private  string CreateToken(UserResponse userResponse)
        {
            var user = _userDbContext.userInfoDatas.FirstOrDefault();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userResponse.UserName),
                new Claim(ClaimTypes.MobilePhone,user.Phone),
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
            return jwt;
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
