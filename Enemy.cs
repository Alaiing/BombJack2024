using Microsoft.Xna.Framework;
using Newtonsoft.Json.Bson;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Enemy : Character
    {
        protected BombJack _bombJack;

        protected virtual float StartSpeed => 4;
        private float _timeToMaxSpeed = 120;
        private float _maxSpeedMultiplier = 2;

        private Color[] _blinkColors = new Color[]
        {
            new Color(255, 0, 128),
            new Color(128,0,0)
        };

        private float _blinkColorIndex;
        private float _blinkSpeed = 4;

        protected int _frame;
        protected int _previousX;
        protected float _speedMultiplierOverTime;

        public Level CurrentLevel { get; set; }

        public Enemy(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            DrawOrder = 1;
        }

        public override void Reset()
        {
            base.Reset();
            SetSpeedMultiplier(1f);
            SetBaseSpeed(StartSpeed);
            _blinkColorIndex = 0;
            SetLayerColor(_blinkColors[(int)_blinkColorIndex], 1);
        }

        public void SetBombJack(BombJack bombJack)
        {
            _bombJack = bombJack;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _blinkColorIndex = (_blinkColorIndex + _blinkSpeed * deltaTime) % _colors.Length;
            SetLayerColor(_blinkColors[(int)_blinkColorIndex], 1);

            UpdateFrame();
            _previousX = PixelPositionX;

            _speedMultiplierOverTime = MathHelper.Lerp(1, _maxSpeedMultiplier, Math.Clamp(CurrentLevel.LevelTime / _timeToMaxSpeed, 0f, 1f));
        }

        protected virtual void UpdateFrame()
        {
            if (_previousX != PixelPositionX)
            {
                _frame = (_frame + 1) % SpriteSheet.FrameCount;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame(_frame, SpriteBatch, Position, SpriteSheet.DefaultPivot, 0, CurrentScale, _colors);
            //SpriteBatch.DrawRectangle(GetBounds(), Color.Red);
        }

        protected bool TestBorderCollision()
        {
            Rectangle borders = new Rectangle(0, 0, BombJack2024.PLAYGROUND_WIDTH, BombJack2024.PLAYGROUND_HEIGHT);
            return !borders.Contains(GetBounds());
        }
    }
}
