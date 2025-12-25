using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Models/Entities/HighScore.cs
namespace BombermanGame.src.Models.Entities
{
    public class HighScore
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
        public DateTime GameDate { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}