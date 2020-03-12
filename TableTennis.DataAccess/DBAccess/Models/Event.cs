using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class Event
    {
        [Key] public int EventId { get; set; }
    }
}