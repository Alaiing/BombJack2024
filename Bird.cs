using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Bird : Enemy
    {
        private const int GRID_SIZE = 20;

        private float _startSpeed = 8;
        private float _maxSpeedMultiplier = 2;
        private int _previousX;

        public Bird(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            DrawOrder = 1;
        }

        public override void Reset()
        {
            base.Reset();
            SetBaseSpeed(_startSpeed);
            SetSpeedMultiplier(1f);
            SetLayerColor(Color.Red, 1);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int bjX = _bombJack.PixelPositionX / GRID_SIZE;
            int bjY = _bombJack.PixelPositionY / GRID_SIZE;

            int X = PixelPositionX / GRID_SIZE;
            int Y = PixelPositionY / GRID_SIZE;

            int deltaX = bjX - X;
            int deltaY = bjY - Y;

            if (Math.Abs(deltaY) >= Math.Abs(deltaX))
            {
                MoveDirection = new Vector2(0, Math.Sign(deltaY));
            }
            else
            {
                MoveDirection = new Vector2(Math.Sign(deltaX), 0);
            }

            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (_previousX != PixelPositionX)
            {
                SetFrame((CurrentFrame + 1) % SpriteSheet.FrameCount);
            }
            _previousX = PixelPositionX;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame(CurrentFrame, SpriteBatch, Position, SpriteSheet.DefaultPivot, 0, CurrentScale, Color.White);
        }
    }
}
