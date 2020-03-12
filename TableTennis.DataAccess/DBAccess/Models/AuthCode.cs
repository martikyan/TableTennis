using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class AuthCode
    {
        [Key] public string AuthCodeId { get; set; }

        public bool IsUsed { get; set; }
    }
}