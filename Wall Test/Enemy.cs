using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wall
{
    public class Enemy : Player
    {
        public new int damage = 5;
        public new string sprite = "EE";
        public Enemy(int x, int y) : base(x, y)
        {
            health = 20;
            xPos= x;
            yPos= y;
            health = 20;
        }

    }
}
