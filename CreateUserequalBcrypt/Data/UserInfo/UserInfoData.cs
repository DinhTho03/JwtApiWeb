using CreateUserequalBcrypt.Data.Login;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreateUserequalBcrypt.Data.UserInfo
{
    [Table("UserInfo")]
    public class UserInfoData
    {
        [Key]
        public int Id { get; set; }
        public bool isAdmin { get; set; }
        [Required]
        [MaxLength(12)]
        public string Phone { get; set; }
        [Required]
        [MaxLength(100)]
        public string Address { get; set; }

        public int IdAccount { get; set; }
        [ForeignKey("IdAccount")]
        public AccountData account { get; set; }
    }
}
