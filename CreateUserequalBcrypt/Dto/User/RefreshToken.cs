namespace CreateUserequalBcrypt.Dto.User
{
    public class RefreshToken
    {
        public string Refresh_Token { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
