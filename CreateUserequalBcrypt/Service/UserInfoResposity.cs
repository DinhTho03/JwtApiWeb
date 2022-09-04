using CreateUserequalBcrypt.Data;
using CreateUserequalBcrypt.Data.UserInfo;
using CreateUserequalBcrypt.Dto.User;
using CreateUserequalBcrypt.IService.Login;
using CreateUserequalBcrypt.IService.UserInfo;
using CreateUserequalBcrypt.Model.AccountModel;
using CreateUserequalBcrypt.Model.UserInfo;
using Microsoft.EntityFrameworkCore;

namespace CreateUserequalBcrypt.Service
{
    public class UserInfoResposity : IUserInfoResposity
    {
        private readonly UserDbContext _userDbContext;
        private readonly IAccount _account;

        public UserInfoResposity(UserDbContext userDbContext , IAccount account)
        {
            _userDbContext = userDbContext;
            _account = account;
        }
        
        public async Task<UserInfoModel> Add(AddUserInfo addUserInfo)
        {

            var username = _account.UserName().Result;
            var idUser = _userDbContext.accounts.SingleOrDefault( c =>  c.Username == username ).Id;
            var addUser = new UserInfoData
            {
                Address = addUserInfo.Address,
                isAdmin = addUserInfo.isAdmin,
                Phone = addUserInfo.Phone,
                IdAccount = idUser
            };
            _userDbContext.Add(addUser);
            _userDbContext.SaveChanges();
            return new UserInfoModel
            {
                Address = addUserInfo.Address,
                Phone = addUserInfo.Phone,
                isAdmin = addUserInfo.isAdmin
            };
        }

        public void Delete(int id)
        {
            var userinfo = _userDbContext.userInfoDatas.SingleOrDefault(us => us.Id == id);
            if (userinfo != null)
            {
                _userDbContext.Remove(userinfo);
                _userDbContext.SaveChanges();
            }
        }

        public List<UserInfoModel> GetAll()
        {
            var idAccount = _account.IdAccount().Result;
            var userInfo = _userDbContext.userInfoDatas.Select(us => new UserInfoModel
            {
                Id = us.Id,
                isAdmin = us.isAdmin,
                Address =us.Address,
                Phone = us.Phone
            }).Where(c => c.Id == idAccount);
            return userInfo.ToList();
        }

        public UserInfoModel GetById(int id)
        {
            var userInfo = _userDbContext.userInfoDatas.SingleOrDefault(us => us.Id == id);
            if (userInfo != null)
            {
                return new UserInfoModel
                {
                    Address = userInfo.Address,
                    Phone = userInfo.Phone,
                    isAdmin = userInfo.isAdmin
                };
            }
            return null;
        }


        public void Update(int id, UpdateModel update)
        {
            var userInfo = _userDbContext.userInfoDatas.SingleOrDefault(us => us.Id == id);
            userInfo.Phone = update.Phone;
            userInfo.Address = update.Address;      
            _userDbContext.Update(userInfo);
            _userDbContext.SaveChanges();
        }

        public async Task<bool> IdAlreadyExist(int id)
        {
            return await _userDbContext.userInfoDatas.AnyAsync(p => p.Id == id);
        }

        public async Task<string> UserName(string username)
        {
            return username;
        }
    }
}
