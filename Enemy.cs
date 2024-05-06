using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Enemy : Character
    {
        protected BombJack _bombJack;

        public Enemy(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
        }

        public void SetBombJack(BombJack bombJack)
        {
            _bombJack = bombJack;
        }
    }
}
