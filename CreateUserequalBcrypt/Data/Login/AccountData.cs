using CreateUserequalBcrypt.Data.UserInfo;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreateUserequalBcrypt.Data.Login
{
    [Table("Account")]
    public class AccountData
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } 
        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; }

        public virtual ICollection<UserInfoData> UserInfoDatas { get; set; }

    }
}
