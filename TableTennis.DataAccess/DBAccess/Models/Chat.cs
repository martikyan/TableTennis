using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class Chat
    {
        [Key] public long ChatId { get; set; }
    }
}