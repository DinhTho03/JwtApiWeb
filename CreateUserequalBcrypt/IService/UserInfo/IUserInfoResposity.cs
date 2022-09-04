using CreateUserequalBcrypt.Model.UserInfo;

namespace CreateUserequalBcrypt.IService.UserInfo
{
    public interface IUserInfoResposity
    {
        List<UserInfoModel> GetAll();
        UserInfoModel GetById(int id);
        Task<UserInfoModel> Add(AddUserInfo addUserInfo);
        void Update(int id, UpdateModel update);
        void Delete(int id);

        Task<bool> IdAlreadyExist(int id);
        
    }
}
