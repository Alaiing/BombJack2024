using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class BombJack : Character
    {
        private static string WALK_STATE = "Walk";
        private static string JUMP_STATE = "Jump";
        private static string FALL_STATE = "Fall";

        private SimpleStateMachine _stateMachine;
        private float _drawnFrame;
        private float _walkAnimationSpeed = 10f;
        private float _jumpDuration = 1f;
        private int _jumpHeight = 128;
        private int _maxJumpHeight = 180;
        private float _spriteChangeValue = 1.3f;

        private Level _currentLevel;

        public BombJack(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _stateMachine = new SimpleStateMachine();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            _stateMachine.AddState(WALK_STATE, OnEnter: WalkEnter, OnUpdate: WalkUpdate);
            _stateMachine.AddState(JUMP_STATE, OnEnter: JumpEnter, OnUpdate: JumpUpdate);
            _stateMachine.AddState(FALL_STATE, OnEnter: FallEnter, OnUpdate: FallUpdate);

            _stateMachine.SetState(WALK_STATE);

            SetBaseSpeed(20f);
            SetSpeedMultiplier(1f);
        }

        public void SetLevel(Level level)
        {
            _currentLevel = level;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame((int)Math.Floor(_drawnFrame), SpriteBatch, Position, SpriteSheet.DefaultPivot, 0, CurrentScale, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SimpleControls.GetStates();

            if (SimpleControls.IsLeftDown(PlayerIndex.One))
            {
                MoveDirection.X = -1;
                SetScale(new Vector2(-1, 1));
            }
            else if (SimpleControls.IsRightDown(PlayerIndex.One))
            {
                MoveDirection.X = 1;
                SetScale(new Vector2(1, 1));
            }
            else
            {
                MoveDirection.X = 0;
            }

            if (TestCollision())
            {
                MoveDirection.X = 0;
            }

            _stateMachine.Update(gameTime);
            Move((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Position.X - SpriteSheet.LeftMargin < 0)
            {
                MoveTo(new Vector2(SpriteSheet.LeftMargin, Position.Y));
            }
            else if (Position.X + SpriteSheet.RightMargin > BombJack2024.PLAYGROUND_WIDTH)
            {
                MoveTo(new Vector2(BombJack2024.PLAYGROUND_WIDTH - SpriteSheet.RightMargin, Position.Y));
            }

            if (TestBombCollision(out int index))
            {
                if (_currentLevel.PickUpBomb(index))
                {
                    // TODO: scoring
                    EventsManager.FireEvent(BombJack2024.EVENT_ALL_BOMBS_COLLECTED);
                }
            }
        }

        public void Fall()
        {
            _stateMachine.SetState(FALL_STATE);
        }

        #region States
        private void WalkEnter()
        {
            _drawnFrame = 4;
        }

        private void WalkUpdate(GameTime time, float arg2)
        {
            if (!IsOnGround() && !IsOnPlatform(out Platform _))
            {
                Fall();
                return;
            }

            if (MoveDirection.X != 0)
            {
                _drawnFrame = (_drawnFrame + _walkAnimationSpeed * (float)time.ElapsedGameTime.TotalSeconds) % 2;
            }

            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                _stateMachine.SetState(JUMP_STATE);
            }
        }

        private float _inAirTimer;
        private float _inAirStartHeight;
        private float _boostTimer;

        private void JumpEnter()
        {
            _drawnFrame = 2;
            _inAirTimer = 0;
            _boostTimer = 0;
            _inAirStartHeight = Position.Y;
        }

        private void JumpUpdate(GameTime time, float arg2)
        {
            if (_inAirTimer > 0 && CheckGlide())
                return;

            float deltaTime = (float)time.ElapsedGameTime.TotalSeconds;

            if (SimpleControls.IsUpDown(PlayerIndex.One))
            {
                _boostTimer += deltaTime;
            }

            _inAirTimer += deltaTime;

            int frameOffset = 0;
            if (MoveDirection.X != 0)
            {
                frameOffset = 1;
            }

            _drawnFrame = 2 + frameOffset;

            float currentJumpHeight = MathHelper.Lerp(_jumpHeight, _maxJumpHeight, _boostTimer / _jumpDuration);

            float height = MathUtils.NormalizedParabolicPosition(_inAirTimer / _jumpDuration / 2) * currentJumpHeight;

            MoveTo(new Vector2(Position.X, _inAirStartHeight - height));

            if (_inAirTimer >= _jumpDuration)
            {
                _stateMachine.SetState(FALL_STATE);
            }

            if (IsUnderPlatform())
            {
                _stateMachine.SetState(FALL_STATE);
                return;
            }

            if (Position.Y - SpriteSheet.TopMargin < 0)
            {
                MoveTo(new Vector2(Position.X, SpriteSheet.TopMargin));
                _stateMachine.SetState(FALL_STATE);
            }
        }

        private void FallEnter()
        {
            _inAirTimer = _jumpDuration;
            _inAirStartHeight = Position.Y;
        }

        private void FallUpdate(GameTime time, float arg2)
        {
            if (CheckGlide())
                return;

            _inAirTimer += (float)time.ElapsedGameTime.TotalSeconds;

            int frameOffset = 0;
            if (MoveDirection.X != 0)
            {
                frameOffset = 1;
            }

            _drawnFrame = (_inAirTimer / _jumpDuration > _spriteChangeValue ? 4 : 2) + frameOffset;

            float height = (1 - MathUtils.NormalizedParabolicPosition(_inAirTimer / _jumpDuration / 2)) * _jumpHeight;

            MoveTo(new Vector2(Position.X, _inAirStartHeight + height));

            if (IsOnPlatform(out Platform platform))
            {
                MoveTo(new Vector2(Position.X, platform.Bounds.Y));
                _stateMachine.SetState(WALK_STATE);
                return;
            }

            if (IsOnGround())
            {
                MoveTo(new Vector2(Position.X, BombJack2024.PLAYGROUND_HEIGHT));
                _stateMachine.SetState(WALK_STATE);
            }
        }

        private bool IsOnPlatform(out Platform hitPlatform)
        {
            foreach (Platform platform in _currentLevel.Plateforms)
            {
                if (platform.Bounds.Contains(Position - new Vector2(SpriteSheet.LeftMargin, -1))
                    || platform.Bounds.Contains(Position + new Vector2(SpriteSheet.RightMargin, 1)))
                {
                    hitPlatform = platform;
                    return true;
                }
            }
            hitPlatform = null;
            return false;
        }

        private bool IsOnGround()
        {
            if (Position.Y + 1 >= BombJack2024.PLAYGROUND_HEIGHT)
            {
                return true;
            }
            return false;
        }


        private bool IsUnderPlatform()
        {
            foreach (Platform plateform in _currentLevel.Plateforms)
            {
                Vector2 top = new Vector2(0, SpriteSheet.TopMargin + 1);
                if (plateform.Bounds.Contains(Position - new Vector2(SpriteSheet.LeftMargin, 0) - top)
                    || plateform.Bounds.Contains(Position + new Vector2(SpriteSheet.RightMargin, 0) - top))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckGlide()
        {
            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                _stateMachine.SetState(FALL_STATE);
                return true;
            }

            return false;
        }

        private bool TestCollision()
        {
            Rectangle bounds = GetBounds();
            bounds.Y += 3;
            bounds.Height -= 3;
            foreach (Platform plateform in _currentLevel.Plateforms)
            {
                if (MathUtils.OverlapsWith(bounds, plateform.Bounds))
                    return true;
            }

            return false;
        }
        private bool TestBombCollision(out int index)
        {
            Rectangle bounds = GetBounds();
            for (int i = 0; i < _currentLevel.Bombs.Count; i++)
            {
                Bomb bomb = _currentLevel.Bombs[i];
                if (bomb.Enabled && MathUtils.OverlapsWith(bounds, bomb.GetBounds()))
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        #endregion
    }
}
