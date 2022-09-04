namespace CreateUserequalBcrypt.Dto.User
{
    public class UserInfoFirst
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool isAdmin { get; set; } = false;
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
