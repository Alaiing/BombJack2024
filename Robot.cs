using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Robot : Enemy
    {
        public const string ROBOT_DEAD_EVENT = "RobotDead";

        private const string STATE_WALK = "Walk";
        private const string STATE_FALL = "Fall";
        protected override float StartSpeed => 15f;

        private const float fallSpeedMultiplier = 3f;

        private int _turnCount = 0;
        private float _direction = 1;
        private bool CanTurn => _turnCount < 3;

        private SimpleStateMachine _stateMachine;

        public Robot(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _stateMachine = new SimpleStateMachine();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            _stateMachine.AddState(STATE_WALK, OnEnter: WalkEnter, OnUpdate: WalkUpdate);
            _stateMachine.AddState(STATE_FALL, OnEnter: FallEnter, OnUpdate: FallUpdate);

            _stateMachine.SetState(STATE_FALL);

            SetBaseSpeed(20f);
            SetSpeedMultiplier(1f);
        }

        private void FallUpdate(GameTime gameTime, float stateTime)
        {
            if (Position.Y >= BombJack2024.PLAYGROUND_HEIGHT)
            {
                EventsManager.FireEvent(ROBOT_DEAD_EVENT, this);
                return;
            }

            if (CurrentLevel.IsOnPlatform(this, out Platform platform, partial: false))
            {
                MoveTo(new Vector2(Position.X, platform.Bounds.Y));
                Walk();
            }
        }

        private void FallEnter()
        {
            _turnCount = 0;
            MoveDirection = new Vector2(0, 1);
            SetSpeedMultiplier(fallSpeedMultiplier);
        }

        private void WalkUpdate(GameTime gameTime, float stateTime)
        {
            if (!CurrentLevel.IsOnPlatform(this, out Platform _, partial: !CanTurn))
            {
                if (CanTurn)
                {
                    MoveDirection = new Vector2(-MoveDirection.X, 0);
                    _turnCount++;
                }
                else
                {
                    Fall();
                }
            }
        }

        private void WalkEnter()
        {
            MoveDirection = new Vector2(CurrentScale.X, 0);
            SetSpeedMultiplier(1f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _stateMachine.Update(gameTime);

            if (MoveDirection.X != 0)
            {
                SetScale(new Vector2(MoveDirection.X, 1));
            }
            Move((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void Walk()
        {
            _stateMachine.SetState(STATE_WALK);
        }

        private void Fall()
        {
            _stateMachine.SetState(STATE_FALL);
        }
    }
}
