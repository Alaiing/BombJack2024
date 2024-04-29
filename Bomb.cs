﻿using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Bomb : Character
    {
        public static string ANIMATION_IDLE = "Idle";
        public static string ANIMATION_LIT= "Lit";
        
        public bool IsLit => CurrentAnimationName == ANIMATION_LIT;


        public Bomb(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            SetAnimation(ANIMATION_IDLE);
        }

        public void Light()
        {
            SetAnimation(ANIMATION_LIT);
        }
    }
}
