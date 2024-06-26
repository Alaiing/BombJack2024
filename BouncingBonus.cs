using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class BouncingBonus : Bonus
    {
        public BouncingBonus(SpriteSheet spriteSheet, Game game, BombJack bombJack) : base(spriteSheet, game, bombJack)
        {
            SetBaseSpeed(20f);
        }

        public override void Spawn(SpriteSheet spriteSheet, Action onCollected, Level currentLevel, Vector2 position)
        {
            base.Spawn(spriteSheet, onCollected, currentLevel, position);
            MoveDirection = new Vector2(1, 1);
        }

        private Vector2 _lastSafePosition;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Position.X - SpriteSheet.LeftMargin < 0 || Position.X + SpriteSheet.RightMargin > BombJack2024.PLAYGROUND_WIDTH)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                Debug.WriteLine(MoveDirection);
            }
            if (Position.Y - SpriteSheet.TopMargin < 0 || Position.Y + SpriteSheet.BottomMargin > BombJack2024.PLAYGROUND_HEIGHT)
            {
                MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                Debug.WriteLine(MoveDirection);
            }

            if (CurrentLevel.TestPlatformCollision(this, out Platform hitPlatform))
            {
                Rectangle bounds = GetBounds();
                if (bounds.Left < hitPlatform.Bounds.Left && MoveDirection.X > 0 || bounds.Right > hitPlatform.Bounds.Right && MoveDirection.X < 0)
                {
                    MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                }
                else
                {
                    MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                }
                MoveTo(_lastSafePosition);
                Debug.WriteLine(MoveDirection);
            }
            else
            {
                _lastSafePosition = Position;
            }
        }
    }
}
