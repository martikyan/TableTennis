using System.ComponentModel.DataAnnotations;

namespace TableTennis.DataAccess.DBAccess.Models
{
    public class PlayerInfo
    {
        [Key] public int Id { get; set; }

        public string Name { get; set; }
    }
}