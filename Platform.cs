using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombJack2024
{
    public class Platform : OudidonGameComponent
    {
        private bool _horizontal;
        private int _size;
        private Point _position;
        public Vector2 Position => _position.ToVector2();
        private Texture2D _innerTexture;
        private Texture2D _leftTexture;
        private Texture2D _rightTexture;

        private Rectangle _bounds;
        public Rectangle Bounds => _bounds;

        public Platform(Game game, Point position, int size, bool isHorizontal = true) : base(game)
        {
            _horizontal = isHorizontal;
            _size = size;
            _position = position;
            if (_horizontal)
            {
                _innerTexture = game.Content.Load<Texture2D>("inner_platform_h");
                _leftTexture = game.Content.Load<Texture2D>("left_platform_h");
                _rightTexture = game.Content.Load<Texture2D>("right_platform_h");
            }
            else
            {
                // TODO
            }

            _bounds = new Rectangle(position.X, position.Y, size, _innerTexture.Height);
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Draw(_leftTexture, new Vector2(_position.X, _position.Y), Color.White);
            Game.SpriteBatch.Draw(_innerTexture, new Rectangle(_position.X + 1, _position.Y, _size - 1, _innerTexture.Height), Color.White);
            Game.SpriteBatch.Draw(_rightTexture, new Vector2(_position.X + _size, _position.Y), Color.White);
            //Game.SpriteBatch.DrawRectangle(_bounds, Color.Green);
        }
    }
}
