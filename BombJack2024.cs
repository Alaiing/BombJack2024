using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BombJack2024
{
    public class BombJack2024 : OudidonGame
    {
        public const string EVENT_ALL_BOMBS_COLLECTED = "AllBombsCollected";
        public const string EVENT_BOMB_COLLECTED = "BombCollected";

        public const string STATE_TITLE = "Title";
        public const string STATE_MENU = "Menu";
        public const string STATE_LEVEL_INTRO = "LevelIntro";
        public const string STATE_IN_GAME = "InGame";
        public const string STATE_GAME_OVER = "GameOver";

        public const string LIT_BOMB_SCORE_EVENT = "LitBombScore";

        public const int PLAYGROUND_WIDTH = 124;
        public const int PLAYGROUND_HEIGHT = 198;
        public const int BOMB_JACK_POSITION_Y = 86;

        public const float SPAWN_PERIOD = 4;
        public const int MAX_ROBOTS = 4;
        public const int MAX_SNAILS = 3;

        public const float MIN_BONUS_SPAWN_TIMER = 2f;
        public const float MAX_BONUS_SPAWN_TIMER = 10f;

        public const float BONUS_P_DURATION = 5f;

        private BombJack _bombJack;
        private FallingBonus _bonus;
        private BouncingBonus _bonusP;

        private Character _explosion;

        private Texture2D[] _levelBackgrounds;
        private Texture2D _hud;
        private Texture2D _title;
        private Texture2D _menu;
        private Texture2D _playerStart;
        private Texture2D _gameOver;
        private Texture2D _lifeIcon;

        private SpriteSheet _robotSprite;
        private SpriteSheet _snailSprite;
        private SpriteSheet _ballSprite;
        private SpriteSheet _saucerSprite;

        private List<Level> _levels = new();
        private int _currentLevelIndex;

        private SpriteSheet _explosionSprite;
        private SpriteSheet _bombScore1;
        private SpriteSheet _bombScore2;

        private SpriteSheet _bonusESprite;
        private SpriteSheet _bonusBSprite;
        private SpriteSheet _bonusPSprite;
        private float _bonusPTimer;
        private int _bonusPGauge;
        private bool _bonusPSpawned;
        private SpriteSheet _lemonSprite;

        private enum BonusType { B, E };
        private const float _bonusPChance = 0.2f;
        private const float _bonusEChance = 0.2f;
        private const float _bonusBChance = 0.6f;
        private List<BonusType> _bonusTable = new();

        private SpriteFont _digits;
        private SpriteFont _bonusFont;

        private SoundEffect _spawnSound;
        private SoundEffectInstance _spawnSoundInstance;
        private SoundEffect _explosionSound;
        private SoundEffectInstance _explosionSoundInstance;
        private SoundEffect _bonusBSound;
        private SoundEffectInstance _bonusBSoundInstance;
        private SoundEffect _bonusESound;
        private SoundEffectInstance _bonusESoundInstance;
        private SoundEffect _bonusPSound;
        private SoundEffectInstance _bonusPSoundInstance;
        private SoundEffect _bonusPCollectSound;
        private SoundEffectInstance _bonusPCollectSoundInstance;
        private SoundEffect _lemonCollectSound;
        private SoundEffectInstance _lemonCollectSoundInstance;

        public Level CurrentLevel => _levels[_currentLevelIndex];

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            EventsManager.ListenTo(EVENT_ALL_BOMBS_COLLECTED, OnAllBombsCollected);
            EventsManager.ListenTo(BombJack.DIE_EVENT, OnBombJackDie);
            EventsManager.ListenTo<Robot>(Robot.ROBOT_DEAD_EVENT, OnRobotDead);
            EventsManager.ListenTo<bool>(EVENT_BOMB_COLLECTED, BombCollected);
            EventsManager.ListenTo<Vector2>(LIT_BOMB_SCORE_EVENT, SpawnLitBombScore);
            EventsManager.ListenTo<Lemon>(Lemon.LEMON_COLLECTED_EVENT, OnLemonCollected);
            InitBonusTable(10);

            base.Initialize();
        }

        protected override void InitStateMachine()
        {
            AddState(STATE_TITLE, onUpdate: TitleUpdate, onDraw: TitleDraw);
            AddState(STATE_MENU, onUpdate: MenuUpdate, onDraw: MenuDraw);
            AddState(STATE_LEVEL_INTRO, onEnter: LevelIntroEnter, onUpdate: LevelIntroUpdate, onDraw: LevelIntroDraw);
            AddState(STATE_IN_GAME, onEnter: GameEnter, onExit : GameExit, onUpdate: GameUpdate, onDraw: GameDraw);
            AddState(STATE_GAME_OVER, onEnter: GameOverEnter, onUpdate: GameOverUpdate, onExit: GameOverExit, onDraw: GameOverDraw);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteSheet bombJackSprite = new SpriteSheet(Content, "bombjack", 8, 16, new Point(4, 16));

            _bombJack = new BombJack(bombJackSprite, this);
            Components.Add(_bombJack);
            _bombJack.Deactivate();

            _robotSprite = new SpriteSheet(Content, "robot", 6, 16, new Point(3, 16));
            _robotSprite.AddLayer(Content, "robot-eye");
            _snailSprite = new SpriteSheet(Content, "snail", 7, 16, new Point(3, 16));
            _snailSprite.AddLayer(Content, "snail-eye");
            _ballSprite = new SpriteSheet(Content, "ball", 7, 13, new Point(3, 13));
            _ballSprite.AddLayer(Content, "ball-eye");
            _saucerSprite = new SpriteSheet(Content, "saucer", 7, 9, new Point(3, 9));
            _saucerSprite.AddLayer(Content, "saucer_eye");

            _explosionSprite = new SpriteSheet(Content, "explosion", 8, 16, new Point(4, 16));
            _explosionSprite.RegisterAnimation("Idle", 0, 1, 12f);
            _explosion = new Character(_explosionSprite, this);
            Components.Add(_explosion);
            _explosion.Deactivate();

            _levelBackgrounds = new Texture2D[3];
            _levelBackgrounds[0] = Content.Load<Texture2D>("egypt");
            _levelBackgrounds[1] = Content.Load<Texture2D>("greece");
            _levelBackgrounds[2] = Content.Load<Texture2D>("germany");

            _hud = Content.Load<Texture2D>("hud");
            _title = Content.Load<Texture2D>("title");
            _menu = Content.Load<Texture2D>("menu");

            _playerStart = Content.Load<Texture2D>("player-start");
            _gameOver = Content.Load<Texture2D>("game_over");

            _lifeIcon = Content.Load<Texture2D>("life_icon");

            _digits = CreateSpriteFont("digits", " 0123456789", width: 5, height: 8);
            _bonusFont = CreateSpriteFont("bonus-font", "23456789x", width: 3, height: 5);

            _bombScore1 = new SpriteSheet(Content, "bomb_score", 8, 5, Point.Zero);
            _bombScore1.RegisterAnimation("Idle", 0, 3, 10);

            _bonusESprite = new SpriteSheet(Content, "bonus-E", 7, 13, new Point(3, 13));
            _bonusESprite.RegisterAnimation(Bonus.ANIMATION_IDLE, 0, 2, 8);

            _bonusBSprite = new SpriteSheet(Content, "bonus-B", 7, 13, new Point(3, 13));
            _bonusBSprite.RegisterAnimation(Bonus.ANIMATION_IDLE, 0, 2, 8);

            _bonusPSprite = new SpriteSheet(Content, "bonus-P", 7, 13, new Point(3, 13));
            _bonusPSprite.RegisterAnimation(Bonus.ANIMATION_IDLE, 0, 3, 16);

            _lemonSprite = new SpriteSheet(Content, "lemon", 8, 16, new Point(4, 16));
            _lemonSprite.RegisterAnimation(Lemon.ANIMATION_IDLE, 0, 3, 8);
            _lemonSprite.RegisterAnimation(Lemon.ANIMATION_ENDING, 4, 7, 8);

            _bonus = new FallingBonus(_bonusESprite, this, _bombJack);
            Components.Add(_bonus);
            _bonus.Deactivate();

            _bonusP = new BouncingBonus(_bonusPSprite, this, _bombJack);
            Components.Add(_bonusP);
            _bonusP.Deactivate();

            _spawnSound = Content.Load<SoundEffect>("teuhuu");
            _spawnSoundInstance = _spawnSound.CreateInstance();
            _explosionSound = Content.Load<SoundEffect>("prrouiiii");
            _explosionSoundInstance = _explosionSound.CreateInstance();
            _bonusBSound = Content.Load<SoundEffect>("poumtrrrr");
            _bonusBSoundInstance = _bonusBSound.CreateInstance();
            _bonusESound = Content.Load<SoundEffect>("tchountchountchountchien");
            _bonusESoundInstance = _bonusESound.CreateInstance();
            _bonusPSound = Content.Load<SoundEffect>("tchoutchou");
            _bonusPSoundInstance = _bonusPSound.CreateInstance();
            _bonusPSoundInstance.IsLooped = true;
            _bonusPCollectSound = Content.Load<SoundEffect>("bouipbouipbouip");
            _bonusPCollectSoundInstance = _bonusPCollectSound.CreateInstance();
            _lemonCollectSound = Content.Load<SoundEffect>("poutoumdoum");

            _levels.Add(new Level(this, "level1.data"));
            _levels.Add(new Level(this, "level2.data"));
            _levels.Add(new Level(this, "level3.data"));

            SetState(STATE_TITLE);
        }

        private void InitBonusTable(int amount)
        {
            for (int i = 0; i < amount * _bonusBChance; i++)
            {
                _bonusTable.Add(BonusType.B);
            }
            for (int i = 0; i < amount * _bonusEChance; i++)
            {
                _bonusTable.Add(BonusType.E);
            }
        }

        private SpriteFont CreateSpriteFont(string fontAsset, string charsString, int width, int height)
        {
            Texture2D fontTexture = Content.Load<Texture2D>(fontAsset);
            List<Rectangle> glyphBounds = new List<Rectangle>();
            List<Rectangle> cropping = new List<Rectangle>();
            List<char> chars = new List<char>();
            chars.AddRange(charsString);
            List<Vector3> kerning = new List<Vector3>();
            for (int i = 0; i < fontTexture.Width / width; i++)
            {
                glyphBounds.Add(new Rectangle(i * width, 0, width, height));
                cropping.Add(new Rectangle(0, 0, width, height));
                kerning.Add(new Vector3(1, width, 0));
            }
            return new SpriteFont(fontTexture, glyphBounds, cropping, chars, 0, 0, kerning, '2');
        }

        public void StartGame()
        {
            _currentLevelIndex = -1;
            _bombJack.ResetScore();
            _bombJack.ResetLives();
            NextLevel();
        }

        public void StartLevel(int index)
        {
            _levels[index].Restart();
            _bombJack.SetLevel(_levels[index]);
            _bombJack.MoveTo(new Vector2(PLAYGROUND_WIDTH / 2, BOMB_JACK_POSITION_Y));
            _bombJack.MoveDirection = Vector2.Zero;
            _bombJack.Fall();
        }

        public void StartBombJack()
        {
            _bombJack.RemoveLife();
            _bombJack.Activate();
        }

        private void SpawnRandomBonus()
        {
            BonusType randomType = _bonusTable[CommonRandom.Random.Next(_bonusTable.Count)];

            switch (randomType)
            {
                case BonusType.B:
                    SpawnBonusB();
                    break;
                case BonusType.E:
                    SpawnBonusE();
                    break;
            }
        }

        private void SpawnBonusE()
        {
            _bonus.Spawn(_bonusESprite, ExtraLife, CurrentLevel, new Vector2(118, 23));
        }

        private void ExtraLife()
        {
            _bombJack.AddLife();
            _bombJack.AddScore(Bonus.BONUS_E_SCORE);
            _bonusESoundInstance.Stop();
            _bonusESoundInstance.Play();
        }

        private void SpawnBonusB()
        {
            _bonus.Spawn(_bonusBSprite, IncreaseBonusRank, CurrentLevel, new Vector2(118, 23));
        }

        private void SpawnBonusP()
        {
            _bonusPSoundInstance.Play();
            _bonusP.Spawn(_bonusPSprite, SpawnLemons, CurrentLevel, new Vector2(PLAYGROUND_WIDTH / 2, _bonusPSprite.TopMargin));
        }

        private void IncreaseBonusRank()
        {
            _bonusBSoundInstance.Stop();
            _bonusBSoundInstance.Play();
            _bombJack.AddScore(Bonus.BONUS_B_SCORE);
            _bombJack.ScoreBonusRank++;
        }

        public void NextLevel()
        {
            _bombJack.ScoreBonusRank = 0;
            if (_currentLevelIndex >= 0)
            {
                CurrentLevel.Reset();
                ClearLemons();
                _explosion.Deactivate();
                CurrentLevel.DeactivateLevel();
            }
            _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Count;
            CurrentLevel.Activate();
            SetState(STATE_LEVEL_INTRO);
        }

        private int _robotCount;
        private void SpawnRobot(Vector2 position)
        {
            if (_stateMachine.CurrentState != STATE_IN_GAME)
            {
                return;
            }
            Enemy robot = new Robot(_robotSprite, this);
            AddEnemy(robot, position);
            _robotCount++;
        }

        private int _snailCount;
        private void SpawnSnail(Vector2 position)
        {
            Enemy snail = new Snail(_snailSprite, this);
            AddEnemy(snail, position);
            _snailCount++;
        }

        private void SpawnBall(Vector2 position)
        {
            Enemy snail = new Snail(_ballSprite, this);
            AddEnemy(snail, position);
            _snailCount++;
        }

        private void SpawnSaucer(Vector2 position)
        {
            Saucer saucer = new Saucer(_saucerSprite, this);
            AddEnemy(saucer, position);
            saucer.OnHitPlatform(null);
        }

        private void AddEnemy(Enemy enemy, Vector2 position)
        {
            enemy.SetBombJack(_bombJack);
            enemy.MoveTo(position);
            enemy.CurrentLevel = CurrentLevel;
            Components.Add(enemy);
            CurrentLevel.Enemies.Add(enemy);
            enemy.Activate();
        }

        private void RemoveEnemy(Enemy enemy)
        {
            Components.Remove(enemy);
            CurrentLevel.Enemies.Remove(enemy);
        }

        private int _explosionAnimationCount;
        private Action<Vector2> _onExplosionEnd;
        private void SpawnExplosion(Vector2 position, Action<Vector2> onExplosionEnd)
        {
            if (_bonusPTimer > 0)
                return;

            _explosion.MoveTo(position);
            _explosion.Activate();
            _explosionAnimationCount = 0;
            _onExplosionEnd = onExplosionEnd;
            _explosion.SetAnimation("Idle", onAnimationEnd: OnExplosionAnimation);
        }


        private void OnExplosionAnimation()
        {
            _explosionAnimationCount++;
            if (_explosionAnimationCount >= 4)
            {
                if (_bonusPTimer > 0)
                    return;

                _explosion.Deactivate();
                _onExplosionEnd?.Invoke(_explosion.Position);
            }
        }

        private void BombCollected(bool lit)
        {
            if (_bonusPSpawned)
                return;

            _bonusPGauge += lit ? 2 : 1;
            if (_bonusPGauge > 20 && !_bonus.Enabled)
            {
                SpawnBonusP();
                _bonusPSpawned = true;
            }
        }

        private void SpawnLitBombScore(Vector2 position)
        {
            BombScore.SpawnScore(_bombScore1, this, _bonusFont, position, _bombJack.ScoreBonusRank);
        }

        private List<Lemon> _lemons = new();
        private void SpawnLemons()
        {
            _bonusPSoundInstance.Stop();
            _bonusPCollectSoundInstance.Play();
            _bonusPTimer = BONUS_P_DURATION;
            foreach (Enemy enemy in CurrentLevel.Enemies)
            {
                Lemon lemon = new Lemon(_lemonSprite, this);
                lemon.Spawn(enemy, _bombJack, BONUS_P_DURATION);
                _lemons.Add(lemon);
            }
            _bombJack.AddScore(Bonus.BONUS_P_SCORE);
        }

        private void OnLemonCollected(Lemon lemon)
        {
            SoundEffectInstance lemonCollectSoundInstance = _lemonCollectSound.CreateInstance();
            lemonCollectSoundInstance.Pan = CommonRandom.Random.Next(-1, 2);
            lemonCollectSoundInstance.Play();
            RemoveEnemy(lemon.Enemy);
            if (lemon.Enemy is not Bird)
            {
                _robotCount--;
            }
            _bombJack.AddScore(BombJack.LEMON_SCORE);
            _lemons.Remove(lemon);
        }

        private void ClearLemons()
        {
            foreach (Lemon lemon in _lemons)
            {
                Components.Remove(lemon);
            }
            _lemons.Clear();
        }
        #region States
        private void TitleUpdate(GameTime time, float stateTime)
        {
            SimpleControls.GetStates();
            if (stateTime > 5f || SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                SetState(STATE_MENU);
            }
        }

        private void TitleDraw(SpriteBatch batch, GameTime time)
        {
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Opaque);
            batch.Draw(_title, Vector2.Zero, Color.White);
            batch.End();
        }


        private void MenuUpdate(GameTime time, float stateTime)
        {
            SimpleControls.GetStates();
            if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                StartGame();
            }
        }

        private void MenuDraw(SpriteBatch batch, GameTime time)
        {
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Opaque);
            batch.Draw(_menu, Vector2.Zero, Color.White);
            batch.End();
        }

        private void LevelIntroEnter()
        {
            _bombJack.Deactivate();
            _bonus.Deactivate();
            StartLevel(_currentLevelIndex);
        }

        private void LevelIntroUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 3f)
                SetState(STATE_IN_GAME);
        }

        private void LevelIntroDraw(SpriteBatch batch, GameTime gameTime)
        {
            GameDraw(batch, gameTime);
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            batch.Draw(_playerStart, new Vector2((PLAYGROUND_WIDTH - _playerStart.Width) / 2, (PLAYGROUND_HEIGHT - _playerStart.Height) / 2), Color.White);
            batch.End();
        }

        private void GameOverEnter()
        {
            _bombJack.Enabled = false;
        }

        private void GameOverUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 3f)
                SetState(STATE_MENU);
        }

        private void GameOverDraw(SpriteBatch batch, GameTime gameTime)
        {
            GameDraw(batch, gameTime);
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            batch.Draw(_gameOver, new Vector2((PLAYGROUND_WIDTH - _gameOver.Width) / 2, (PLAYGROUND_HEIGHT - _gameOver.Height) / 2), Color.White);
            batch.End();
        }

        private void GameOverExit()
        {
            CurrentLevel.DeactivateLevel();
        }


        private float _spawnTimer;
        private float _bonusSpawnTimer;
        private void GameEnter()
        {
            StartBombJack();
            CurrentLevel.Start(_bombJack);
            _spawnTimer = 0;
            _bonusPGauge = 0;
            _bonusPSpawned = false;
            SetBonusSpawnTimer();
            _robotCount = 0;
            _snailCount = 0;
        }

        private void GameExit()
        {
            BombScore.ClearScores(this);
        }

        private void GameUpdate(GameTime gameTime, float stateTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_bonusPTimer > 0 && _bonusPTimer < deltaTime)
            {
                CurrentLevel.RespawnEnemies();
                _spawnTimer = 0;
            }

            _bonusPTimer -= deltaTime;
            if (_bonusPTimer <= 0 && _robotCount < MAX_ROBOTS)
            {
                _spawnTimer += deltaTime;
                if (_spawnTimer > SPAWN_PERIOD)
                {
                    SpawnExplosion(CurrentLevel.RobotSpawn, onExplosionEnd: SpawnRobot);
                    _spawnSoundInstance.Stop();
                    _spawnSoundInstance.Play();
                    _spawnTimer -= SPAWN_PERIOD;
                }
            }

            if (!_bonus.Enabled && !_bonusP.Enabled && _bombJack.ScoreBonusRank < 3)
            {
                _bonusSpawnTimer -= deltaTime;
                if (_bonusSpawnTimer <= 0)
                {
                    SpawnRandomBonus();
                    SetBonusSpawnTimer();
                }
            }
            CurrentLevel.LevelTime += deltaTime;
        }

        private void SetBonusSpawnTimer()
        {
            _bonusSpawnTimer = MathHelper.Lerp(MIN_BONUS_SPAWN_TIMER, MAX_BONUS_SPAWN_TIMER, (float)Math.Clamp(MathUtils.NextGaussian(mu: 2) / 4, 0, 1));
        }

        protected void GameDraw(SpriteBatch batch, GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            int backgroundIndex = CurrentLevel.BackgroundIndex;
            if (backgroundIndex < 0 || backgroundIndex >= _levelBackgrounds.Length)
                backgroundIndex = 0;

            batch.Draw(_levelBackgrounds[backgroundIndex], Vector2.Zero, Color.White);
            batch.Draw(_hud, new Vector2(PLAYGROUND_WIDTH, 0), Color.White);
            batch.DrawString(_digits, _bombJack.Score.ToString().PadLeft(6, ' '), new Vector2(124, 20), Color.White);

            for (int i = 0; i < Math.Min(_bombJack.RemainingLives, 4); i++)
            {
                batch.Draw(_lifeIcon, new Vector2(150 - i * (_lifeIcon.Width + 1), 28), Color.White);
            }

            DrawComponents(gameTime);
            //DrawGrid(batch);
            batch.End();
        }

        private static void DrawGrid(SpriteBatch batch)
        {
            for (int i = 0; i <= PLAYGROUND_WIDTH / Bird.GRID_SIZE; i++)
            {
                batch.DrawLine(new Vector2(i * Bird.GRID_SIZE, 0), new Vector2(i * Bird.GRID_SIZE, PLAYGROUND_HEIGHT), Color.Black);
            }
            for (int i = 0; i <= PLAYGROUND_HEIGHT / Bird.GRID_SIZE; i++)
            {
                batch.DrawLine(new Vector2(0, i * Bird.GRID_SIZE), new Vector2(PLAYGROUND_WIDTH, i * Bird.GRID_SIZE), Color.Black);
            }
        }


        #endregion

        #region Events
        private void OnAllBombsCollected()
        {
            NextLevel();
        }

        private void OnBombJackDie()
        {
            if (_bombJack.RemainingLives == 0)
            {
                SetState(STATE_GAME_OVER);
            }
            else
            {
                SetState(STATE_LEVEL_INTRO);
            }
        }

        private void OnRobotDead(Robot robot)
        {
            Components.Remove(robot);
            CurrentLevel.Enemies.Remove(robot);
            _explosionSoundInstance.Stop();
            _explosionSoundInstance.Play();
            if (_snailCount < MAX_SNAILS)
            {
                SpawnExplosion(robot.Position, SpawnBall);
            }
            else
            {
                SpawnExplosion(robot.Position, SpawnSaucer);
            }
        }
        #endregion
    }
}
