using CreateUserequalBcrypt.Data.Login;
using CreateUserequalBcrypt.Data.UserInfo;
using Microsoft.EntityFrameworkCore;

namespace CreateUserequalBcrypt.Data
{
    public class UserDbContext :DbContext
    {
        public UserDbContext (DbContextOptions options) : base(options) { }

        public DbSet<AccountData> accounts { get; set; }
        public DbSet<RefreshTokenData> refreshTokens { get; set; }
        public DbSet<UserInfoData> userInfoDatas { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
