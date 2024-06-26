using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Bonus : Character
    {
        public const string ANIMATION_IDLE = "Idle";

        public const int BONUS_P_SCORE = 2000;
        public const int BONUS_B_SCORE = 1000;
        public const int BONUS_E_SCORE = 3000;

        public Level CurrentLevel { get; set; }

        private Action _onCollected;

        private BombJack _bombJack;

        public Bonus(SpriteSheet spriteSheet, Game game, BombJack bombJack) : base(spriteSheet, game)
        {
            _bombJack = bombJack;
            SetAnimation(ANIMATION_IDLE);
            SetBaseSpeed(10);
            DrawOrder = 10;
        }

        public virtual void Spawn(SpriteSheet spriteSheet, Action onCollected, Level currentLevel, Vector2 position)
        {
            CurrentLevel = currentLevel;
            _spriteSheet = spriteSheet;
            _onCollected = onCollected;
            MoveTo(position);
            SetFrame(0);
            Activate();
        }

        public void Collect()
        {
            Deactivate();
            _onCollected?.Invoke();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Rectangle bjBounds = _bombJack.GetBounds();
            Rectangle bounds = GetBounds();

            if (MathUtils.OverlapsWith(bounds, bjBounds))
            {
                Collect();
            }
        }
    }
}
