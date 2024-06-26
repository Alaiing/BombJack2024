using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class BombJack : Character
    {
        public const string DIE_EVENT = "Die";

        private const string WALK_STATE = "Walk";
        private const string JUMP_STATE = "Jump";
        private const string FALL_STATE = "Fall";
        private const string DIE_STATE = "Die";

        public const int LEMON_SCORE = 100;
        public const int UNLIT_BOMB_SCORE = 100;
        public const int LIT_BOMB_SCORE = 200;
        public const int JUMP_SCORE = 10;

        private const int STARTING_LIVES = 3;

        private SimpleStateMachine _stateMachine;
        private float _drawnFrame;
        private float _walkAnimationSpeed = 10f;
        private const float JUMP_DURATION = 1.5f;
        private const float DIE_JUMP_DURATION = 0.25f;
        private const float DIE_DURATION = 1f;
        private const int JUMP_HEIGHT = 128;
        private const int MAX_JUMP_HEIGHT = 180;
        private const int DIE_JUMP_HEIGHT = 10;
        private int _minJumpHeight;
        private int _maxJumpHeight;
        private float _spriteChangeValue = 1.3f;


        private SoundEffect _jumpSound;
        private SoundEffectInstance _jumpSoundInstance;
        private SoundEffect _platformSound;
        private SoundEffectInstance _platformSoundInstance;
        private float _jumpSoundCooldown;
        private float _jumpSoundTimer;

        private Level _currentLevel;

        private int _score;
        public int Score => _score;
        public int ScoreBonusRank { get; set; }
        private int _lives;
        public int RemainingLives => _lives;

        public BombJack(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _stateMachine = new SimpleStateMachine();
            Initialize();
            DrawOrder = 99;
        }

        public override void Initialize()
        {
            base.Initialize();
            _stateMachine.AddState(WALK_STATE, OnEnter: WalkEnter, OnUpdate: WalkUpdate);
            _stateMachine.AddState(JUMP_STATE, OnEnter: JumpEnter, OnUpdate: JumpUpdate);
            _stateMachine.AddState(FALL_STATE, OnEnter: FallEnter, OnUpdate: FallUpdate);
            _stateMachine.AddState(DIE_STATE, OnEnter: DieEnter, OnUpdate: DieUpdate);

            _stateMachine.SetState(WALK_STATE);

            SetBaseSpeed(15f);
            SetSpeedMultiplier(1f);
        }

        protected override void LoadContent()
        {
            _jumpSound = Game.Content.Load<SoundEffect>("bl");
            _jumpSoundInstance = _jumpSound.CreateInstance();
            _jumpSoundInstance.Volume = 0.5f;
            _jumpSoundCooldown = (float)_jumpSound.Duration.TotalSeconds / 2;

            _platformSound = Game.Content.Load<SoundEffect>("boing");
            _platformSoundInstance = _platformSound.CreateInstance();
        }

        public void SetLevel(Level level)
        {
            _currentLevel = level;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame((int)Math.Floor(_drawnFrame), SpriteBatch, new Vector2(PixelPositionX, PixelPositionY), SpriteSheet.DefaultPivot, 0, CurrentScale, Color.White);
            //SpriteBatch.DrawRectangle(GetBounds(), Color.Green);
        }

        private int _previousY;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _stateMachine.Update(gameTime);

            _jumpSoundTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_jumpSoundTimer <= 0 && _previousY != PixelPositionY
                /*&& (_stateMachine.CurrentState == JUMP_STATE || _stateMachine.CurrentState == FALL_STATE)*/)
            {
                _jumpSoundInstance.Stop();
                float pitch = Math.Clamp(MathHelper.Lerp(-1.0f, 1.0f, 1 - (float)PixelPositionY / BombJack2024.PLAYGROUND_HEIGHT), -1f, 1);
                _jumpSoundInstance.Pitch = pitch;
                _jumpSoundInstance.Play();
                _jumpSoundTimer = _jumpSoundCooldown;
            }
            _previousY = PixelPositionY;
        }

        private bool AliveUpdate(GameTime gameTime)
        {
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

            if (_currentLevel.TestPlatformCollision(this, out Platform _))
            {
                SoundEffectInstance platformSoundInstance = _platformSound.CreateInstance();
                platformSoundInstance.Pan = CommonRandom.Random.Next(-1, 2);
                platformSoundInstance.Play();
                MoveDirection.X = 0;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Move(deltaTime);

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
                if (_currentLevel.Bombs[index].IsLit)
                {
                    AddScore(LIT_BOMB_SCORE * (ScoreBonusRank + 1));
                    EventsManager.FireEvent(BombJack2024.LIT_BOMB_SCORE_EVENT, _currentLevel.Bombs[index].Position);
                }
                else
                {
                    AddScore(UNLIT_BOMB_SCORE);
                }

                if (_currentLevel.PickUpBomb(index))
                {
                    EventsManager.FireEvent(BombJack2024.EVENT_ALL_BOMBS_COLLECTED);
                }
                else
                {
                    EventsManager.FireEvent(BombJack2024.EVENT_BOMB_COLLECTED, _currentLevel.Bombs[index].IsLit);
                }
            }

            if (TestEnemyCollision())
            {
                _currentLevel.FreezeEnemies(true);
                Die();
                return false;
            }

            return true;
        }

        public void Fall()
        {
            _stateMachine.SetState(FALL_STATE);
        }

        public void Jump(int jumpHeight, int maxJumpHeight)
        {
            _minJumpHeight = jumpHeight;
            _maxJumpHeight = maxJumpHeight;

            _stateMachine.SetState(JUMP_STATE);
        }

        public void Die()
        {
            if (_stateMachine.CurrentState == WALK_STATE)
            {
                EventsManager.FireEvent(DIE_EVENT);
                _previousY = PixelPositionY;
                return;
            }

            _stateMachine.SetState(DIE_STATE);
        }

        public void PlatformSound()
        {
            _platformSoundInstance.Pan = CommonRandom.Random.Next(-1, 2);
            _platformSoundInstance.Stop();
            _platformSoundInstance.Play();
        }

        #region Lives
        public void ResetLives()
        {
            _lives = STARTING_LIVES;
        }

        public void AddLife()
        {
            _lives++;
        }

        public void RemoveLife()
        {
            _lives--;
        }

        #endregion

        #region Scoring
        public void ResetScore()
        {
            _score = 0;
        }

        public void AddScore(int score)
        {
            _score += score;
        }
        #endregion

        #region States
        private void WalkEnter()
        {
            _drawnFrame = 4;
        }

        private void WalkUpdate(GameTime time, float arg2)
        {
            if (!AliveUpdate(time))
            {
                return;
            }

            if (!IsOnGround() && !_currentLevel.IsOnPlatform(this, out Platform _, partial: true))
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
                Jump(JUMP_HEIGHT, MAX_JUMP_HEIGHT);
                AddScore(JUMP_SCORE);
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
            if (!AliveUpdate(time))
            {
                return;
            }

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

            float currentJumpHeight = MathHelper.Lerp(_minJumpHeight, _maxJumpHeight, _boostTimer / JUMP_DURATION);

            float height = MathUtils.NormalizedParabolicPosition(_inAirTimer / JUMP_DURATION / 2) * currentJumpHeight;

            MoveTo(new Vector2(Position.X, _inAirStartHeight - height));

            if (_inAirTimer >= JUMP_DURATION)
            {
                _stateMachine.SetState(FALL_STATE);
            }

            if (IsUnderPlatform())
            {
                _stateMachine.SetState(FALL_STATE);
                PlatformSound();
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
            _inAirTimer = JUMP_DURATION;
            _inAirStartHeight = Position.Y;
        }

        private void FallUpdate(GameTime time, float arg2)
        {
            if (!AliveUpdate(time))
            {
                return;
            }

            if (CheckGlide())
                return;

            _inAirTimer += (float)time.ElapsedGameTime.TotalSeconds;

            int frameOffset = 0;
            if (MoveDirection.X != 0)
            {
                frameOffset = 1;
            }

            _drawnFrame = (_inAirTimer / JUMP_DURATION > _spriteChangeValue ? 4 : 2) + frameOffset;

            float height = (1 - MathUtils.NormalizedParabolicPosition(_inAirTimer / JUMP_DURATION / 2)) * JUMP_HEIGHT;

            MoveTo(new Vector2(Position.X, _inAirStartHeight + height));

            if (_currentLevel.IsOnPlatform(this, out Platform platform, partial: true))
            {
                MoveTo(new Vector2(Position.X, platform.Bounds.Y));
                PlatformSound();
                _stateMachine.SetState(WALK_STATE);
                return;
            }

            if (IsOnGround())
            {
                MoveTo(new Vector2(Position.X, BombJack2024.PLAYGROUND_HEIGHT));
                _stateMachine.SetState(WALK_STATE);
            }
        }

        private void DieEnter()
        {
            MoveDirection = Vector2.Zero;
            _inAirStartHeight = Position.Y;
        }

        private void DieUpdate(GameTime time, float stateTime)
        {
            float height = MathUtils.NormalizedParabolicPosition(stateTime / DIE_JUMP_DURATION / 2) * DIE_JUMP_HEIGHT;

            MoveTo(new Vector2(Position.X, _inAirStartHeight - height));

            if (stateTime > DIE_DURATION)
            {
                EventsManager.FireEvent(DIE_EVENT);
            }
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

        private bool TestEnemyCollision()
        {
            Rectangle bounds = GetBounds();
            for (int i = 0; i < _currentLevel.Enemies.Count; i++)
            {
                Enemy enemy = _currentLevel.Enemies[i];
                if (enemy.Enabled && MathUtils.OverlapsWith(bounds, enemy.GetBounds()))
                {
                    return true;
                }
            }
            return false;

        }

        #endregion
    }
}
