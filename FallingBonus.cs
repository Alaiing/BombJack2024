using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class FallingBonus : Bonus
    {
        private const string STATE_FALL = "Fall";
        private const string STATE_ROLL = "Roll";

        private SimpleStateMachine _stateMachine;
        private int _direction;

        public FallingBonus(SpriteSheet spriteSheet, Game game, BombJack bombJack) : base(spriteSheet, game, bombJack)
        {
            _stateMachine = new SimpleStateMachine();
            _stateMachine.AddState(STATE_FALL, OnEnter: FallEnter, OnUpdate: FallUpdate);
            _stateMachine.AddState(STATE_ROLL, OnEnter: RollEnter, OnUpdate: RollUpdate);
            _direction = -1;
        }

        public override void Spawn(SpriteSheet spriteSheet, Action onCollected, Level currentLevel, Vector2 position)
        {
            base.Spawn(spriteSheet, onCollected, currentLevel, position);
            _stateMachine.SetState(STATE_FALL);
        }

        public override void Update(GameTime gameTime)
        {
            _stateMachine.Update(gameTime);
            base.Update(gameTime);
        }

        private void FallEnter()
        {
            SetAnimationSpeedMultiplier(0);
            MoveDirection = new Vector2(0, 1);
        }

        private void FallUpdate(GameTime gameTime, float stateTime)
        {
            if (CurrentLevel.IsOnPlatform(this, out Platform platform, partial: true)
                || Position.Y >= BombJack2024.PLAYGROUND_HEIGHT)
            {
                if (platform != null)
                {
                    MoveTo(new Vector2(Position.X, platform.Bounds.Y));
                }
                else
                {
                    MoveTo(new Vector2(Position.X, BombJack2024.PLAYGROUND_HEIGHT));
                }
                _stateMachine.SetState(STATE_ROLL);
            }
        }

        private void RollEnter()
        {
            SetAnimationSpeedMultiplier(1f);
            MoveDirection = new Vector2(_direction, 0);
        }

        private void RollUpdate(GameTime gameTime, float stateTime)
        {
            if (Position.X - SpriteSheet.LeftMargin <= 0 || Position.X + SpriteSheet.RightMargin >= BombJack2024.PLAYGROUND_WIDTH)
                MoveDirection *= -1;

            if (Position.Y == BombJack2024.PLAYGROUND_HEIGHT)
                return;

            if (!CurrentLevel.IsOnPlatform(this, out Platform _, partial: false))
            {
                _stateMachine.SetState(STATE_FALL);
            }
        }
    }
}
