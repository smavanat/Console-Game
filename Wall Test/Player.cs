using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wall
{
    public  class Player
    {
        public int health;
        public int damage = 10;
        public int xPos;
        public int yPos;
        public string sprite = " 0";
        

        public Player(int x, int y) {
            health = 100;
            xPos = x;
            yPos = y;
        }
    }
}
