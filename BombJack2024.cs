using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;

namespace BombJack2024
{
    public class BombJack2024 : OudidonGame
    {
        public static int PLAYGROUND_WIDTH = 124;
        public static int PLAYGROUND_HEIGHT = 198;

        private BombJack _bombJack;

        private Texture2D _levelBackground;
        private Texture2D _hud;

        private Level _currentLevel;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteSheet bombJackSprite = new SpriteSheet(Content, "bombjack", 8, 16, new Point(4,16));

            _bombJack = new BombJack(bombJackSprite, this);
            _bombJack.MoveTo(new Vector2(ScreenWidth / 2, ScreenHeight /2 ));
            Components.Add(_bombJack);
            _bombJack.Fall();

            _levelBackground = Content.Load<Texture2D>("egypt");
            _hud = Content.Load<Texture2D>("hud");

            _currentLevel = new Level(this);

            _currentLevel.AddPlatform(new Point(50, 150), 100);

            _bombJack.SetLevel(_currentLevel);

            _currentLevel.Activate();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime); // Updates state machine and components, in that order
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.DrawGameplay(gameTime); // Draws state machine
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            SpriteBatch.Draw(_levelBackground, Vector2.Zero, Color.White);
            SpriteBatch.Draw(_hud, new Vector2(PLAYGROUND_WIDTH, 0), Color.White);
            DrawComponents(gameTime);
            SpriteBatch.End();
        }

        protected override void DrawUI(GameTime gameTime)
        {
            // TODO: Draw your overlay UI here
        }
    }
}
