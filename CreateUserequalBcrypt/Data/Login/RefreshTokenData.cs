using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreateUserequalBcrypt.Data.Login
{
    [Table("RefreshToken")]
    public class RefreshTokenData
    {
        [Key]
        public Guid Id{ get; set; }
        public int IdAccount { get; set; }
        [ForeignKey(nameof(IdAccount))]
        public AccountData AccountData { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime IssueAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
