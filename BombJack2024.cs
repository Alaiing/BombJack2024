using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;

namespace BombJack2024
{
    public class BombJack2024 : OudidonGame
    {
        public static string EVENT_ALL_BOMBS_COLLECTED = "AllBombsCollected";

        public static string STATE_TITLE = "Title";
        public static string STATE_MENU = "Menu";
        public static string STATE_LEVEL_INTRO = "LevelIntro";
        public static string STATE_IN_GAME = "InGame";

        public static int PLAYGROUND_WIDTH = 124;
        public static int PLAYGROUND_HEIGHT = 198;
        public static int BOMB_JACK_POSITION_Y = 86;

        private BombJack _bombJack;

        private Texture2D[] _levelBackgrounds;
        private Texture2D _hud;
        private Texture2D _title;
        private Texture2D _menu;
        private Texture2D _playerStart;

        private List<Level> _levels = new();
        private int _currentLevelIndex;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            EventsManager.ListenTo(EVENT_ALL_BOMBS_COLLECTED, OnAllBombsCollected);
            EventsManager.ListenTo(BombJack.DIE_EVENT, OnBombJackDie);

            base.Initialize();
        }

        protected override void InitStateMachine()
        {
            AddState(STATE_TITLE, onUpdate: TitleUpdate, onDraw: TitleDraw);
            AddState(STATE_MENU, onUpdate: MenuUpdate, onDraw: MenuDraw);
            AddState(STATE_LEVEL_INTRO, onEnter: LevelIntroEnter, onUpdate: LevelIntroUpdate, onDraw: LevelIntroDraw);
            AddState(STATE_IN_GAME, onEnter: GameEnter, onDraw: GameDraw);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteSheet bombJackSprite = new SpriteSheet(Content, "bombjack", 8, 16, new Point(4, 16));

            _bombJack = new BombJack(bombJackSprite, this);
            Components.Add(_bombJack);
            _bombJack.Deactivate();

            _levelBackgrounds = new Texture2D[2];
            _levelBackgrounds[0] = Content.Load<Texture2D>("egypt");
            _levelBackgrounds[1] = Content.Load<Texture2D>("greece");

            _hud = Content.Load<Texture2D>("hud");
            _title = Content.Load<Texture2D>("title");
            _menu = Content.Load<Texture2D>("menu");
            
            _playerStart = Content.Load<Texture2D>("player-start");

            _levels.Add(new Level(this, "level1.data"));
            _levels.Add(new Level(this, "level2.data"));

            SetState(STATE_TITLE);
        }

        public void StartGame()
        {
            _currentLevelIndex = -1;
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
            _bombJack.Activate();
        }

        public void NextLevel()
        {
            if (_currentLevelIndex >= 0)
            {
                _levels[_currentLevelIndex].Reset();
                _levels[_currentLevelIndex].DeactivateLevel();
            }
            _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Count;
            _levels[_currentLevelIndex].Activate();
            SetState(STATE_LEVEL_INTRO);
        }

        private void OnBombJackDie()
        {
            SetState(STATE_LEVEL_INTRO);
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


        private void GameEnter()
        {
            StartBombJack();
            _levels[_currentLevelIndex].Start(_bombJack);
        }

        protected void GameDraw(SpriteBatch batch, GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            int backgroundIndex = _levels[_currentLevelIndex].BackgroundIndex;
            if (backgroundIndex < 0 || backgroundIndex >= _levelBackgrounds.Length)
                backgroundIndex = 0;

            batch.Draw(_levelBackgrounds[backgroundIndex], Vector2.Zero, Color.White);
            batch.Draw(_hud, new Vector2(PLAYGROUND_WIDTH, 0), Color.White);
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
        #endregion
    }
}
