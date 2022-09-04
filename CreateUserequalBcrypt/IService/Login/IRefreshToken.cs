namespace CreateUserequalBcrypt.IService.Login
{
    public interface IRefreshToken
    {
        string CreateToken(string username);
    }
}
