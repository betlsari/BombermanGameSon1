using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombermanGame.src.Models
{
    public class EmptySpace : IWall
    {
        public char GetSymbol() => ' ';
        public bool IsDestructible() => false;
        public void TakeDamage() { }
        public bool IsDestroyed() => false;
        public int GetDurability() => 0;
    }
}

