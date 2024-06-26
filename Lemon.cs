using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    internal class Lemon : Character
    {
        public const string LEMON_COLLECTED_EVENT = "LemonCollected";

        public const string ANIMATION_IDLE = "Idle";
        public const string ANIMATION_ENDING = "Ending";

        private Enemy _enemy;
        public Enemy Enemy => _enemy;
        private BombJack _bombJack;

        private float _duration;
        private float _timer;

        public Lemon(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
        }

        public void Spawn(Enemy enemy, BombJack bombJack, float duration)
        {
            _duration = duration;
            _timer = 0;
            _enemy = enemy;
            _bombJack = bombJack;
            MoveTo(enemy.Position);
            _enemy.Deactivate();
            Game.Components.Add(this);
            SetAnimation(ANIMATION_IDLE);
        }

        public void RespawnEnemy()
        {
            _enemy.Activate();
            Game.Components.Remove(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (MathUtils.OverlapsWith(GetBounds(), _bombJack.GetBounds()))
            {
                EventsManager.FireEvent(LEMON_COLLECTED_EVENT, this);
                Game.Components.Remove(this);
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer < _duration / 2f && _timer + deltaTime > _duration / 2f)
            {
                SetAnimation(ANIMATION_ENDING);
            }

            _timer += deltaTime;
            if (_timer >= _duration)
            {
                RespawnEnemy();
                return;
            }
        }
    }
}
