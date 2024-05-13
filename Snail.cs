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
    public class Snail : Enemy
    {
        private Vector2 Velocity;
        private const float MAX_SPEED = 20;
        private const float FORCE = 50;
        private const float REBOUND = 10;

        public Snail(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
        }

        private Vector2 _lastSafePosition;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CurrentLevel.TestPlatformCollision(this, out Platform platform))
            {
                MoveTo(_lastSafePosition);

                Rectangle bounds = GetBounds();

                Vector2 centerReboundDirection = bounds.Center.ToVector2() - platform.Bounds.Center.ToVector2();

                float platformTangent = (float)platform.Bounds.Height / platform.Bounds.Width;

                if (centerReboundDirection.Y == 0 || MathF.Abs(centerReboundDirection.Y) / MathF.Abs(centerReboundDirection.X) < platformTangent)
                {
                    Velocity.X = -Velocity.X * REBOUND;
                }
                else
                {
                    Velocity.Y = -Velocity.Y * REBOUND;
                }
            }
            else
            {
                _lastSafePosition = Position;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 force = _bombJack.Position - Position;
            force.Normalize();
            force *= FORCE;

            Vector2 newPosition = 0.5f * deltaTime * deltaTime * force + deltaTime * Velocity + Position;
            Velocity = deltaTime * force + Velocity;

            Velocity = MathF.Min(Velocity.Length(), MAX_SPEED) * Vector2.Normalize(Velocity);

            MoveTo(newPosition);
        }
    }
}
