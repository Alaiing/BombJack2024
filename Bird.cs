using Microsoft.Xna.Framework;
using Oudidon;
using System;

namespace BombJack2024
{
    public class Bird : Enemy
    {
        public const int GRID_SIZE = 20;

        private float _startSpeed = 4;
        private float _maxSpeedMultiplier = 2;
        private int _previousX;

        private int _frame;
        private bool _collided;
        private Point _collidedGridCell;

        public Level CurrentLevel { get; set; }

        private Color[] _blinkColors = new Color[]
            {
            new Color(255, 0, 128),
            new Color(128,0,0)
            };

        private float _blinkColorIndex;
        private float _blinkSpeed = 4;

        public Bird(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            DrawOrder = 1;
        }

        public override void Reset()
        {
            base.Reset();
            SetBaseSpeed(_startSpeed);
            SetSpeedMultiplier(1f);
            SetLayerColor(Color.Red, 1);
            _blinkColorIndex = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 previousPosition = Position;
            Move(deltaTime);

            int X = PixelPositionX / GRID_SIZE;
            int Y = PixelPositionY / GRID_SIZE;

            if (CurrentLevel.TestPlatformCollision(this))
            {
                MoveTo(previousPosition);
                _collided = true;
                X = PixelPositionX / GRID_SIZE;
                Y = PixelPositionY / GRID_SIZE;
                _collidedGridCell = new Point(X, Y);
                if (MoveDirection.X != 0)
                {
                    MoveDirection = new Vector2(0, CommonRandom.Random.Next(0, 2) * 2 - 1);
                }
                else
                {
                    MoveDirection = new Vector2(CommonRandom.Random.Next(0, 2) * 2 - 1, 0);
                    SetScale(new Vector2(-Math.Sign(MoveDirection.X), 1));
                }
            }


            if (!_collided || X != _collidedGridCell.X || Y != _collidedGridCell.Y)
            {
                _collided = false;

                int bjX = _bombJack.PixelPositionX / GRID_SIZE;
                int bjY = _bombJack.PixelPositionY / GRID_SIZE;

                int deltaX = bjX - X;
                int deltaY = bjY - Y;

                if (deltaX == 0 && deltaY == 0)
                {
                    deltaX = _bombJack.PixelPositionX - PixelPositionX;
                    deltaY = _bombJack.PixelPositionY - PixelPositionY;
                }

                if (Math.Abs(deltaY) >= Math.Abs(deltaX))
                {
                    MoveDirection = new Vector2(0, Math.Sign(deltaY));
                }
                else
                {
                    MoveDirection = new Vector2(Math.Sign(deltaX), 0);
                    SetScale(new Vector2(-Math.Sign(MoveDirection.X), 1));
                }
            }

            if (_previousX != PixelPositionX)
            {
                _frame = (_frame + 1) % SpriteSheet.FrameCount;
            }
            _previousX = PixelPositionX;

            _blinkColorIndex = (_blinkColorIndex + _blinkSpeed * deltaTime) % _colors.Length;
            SetLayerColor(_blinkColors[(int)_blinkColorIndex], 1);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteSheet.DrawFrame(_frame, SpriteBatch, Position, SpriteSheet.DefaultPivot, 0, CurrentScale, _colors);
            //SpriteBatch.DrawRectangle(GetBounds(), Color.Red);
        }
    }
}
