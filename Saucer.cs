using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Saucer : Enemy
    {
        private const float MIN_SPEED = 1f;
        private const float MAX_SPEED = 20f;
        private float _maxDistance = MathF.Sqrt(BombJack2024.PLAYGROUND_WIDTH * BombJack2024.PLAYGROUND_WIDTH + BombJack2024.PLAYGROUND_HEIGHT * BombJack2024.PLAYGROUND_HEIGHT) / 2;

        public Saucer(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CurrentLevel.TestPlatformCollision(this, out Platform platform) || TestBorderCollision())
            {
                OnHitPlatform(platform);
            }

            SetSpeedMultiplier(_speedMultiplierOverTime);
            Move((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void OnHitPlatform(Platform platform)
        {
            Vector2 relativePositionToBombjack = _bombJack.Position - Position;

            if (platform == null)
            {
                SetDirectionToBombJack(relativePositionToBombjack);
                return;
            }

            Vector2 previousPosition = Position;
            MoveBy(Vector2.Normalize(relativePositionToBombjack));
            if (CurrentLevel.TestPlatformCollision(this, out Platform _))
            {
                MoveDirection = -MoveDirection;
            }
            else
            {
                SetDirectionToBombJack(relativePositionToBombjack);
            }
            MoveTo(previousPosition);
        }

        private void SetDirectionToBombJack(Vector2 relativePositionToBombjack)
        {
            MoveDirection = Vector2.Normalize(relativePositionToBombjack);
            SetBaseSpeed(MathHelper.Lerp(MIN_SPEED, MAX_SPEED, relativePositionToBombjack.Length() / _maxDistance));
        }
    }
}
