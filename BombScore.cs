using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;

namespace BombJack2024
{
    public class BombScore : Character
    {
        private const int ANIMATION_COUNT = 4;
        private static List<BombScore> _bombScores = new();

        private SpriteFont _bonusFont;
        private int _bonusRank;
        private Color[] _bonusColors =
            {
                Color.White,
                new Color(255,255,128),
                new Color(255,255,0),
                new Color(255,128,0)
            };
        private float _bonusColorIndex;
        private float _colorChangeSpeed = 10;
        private int _animationCount;

        public static void SpawnScore(SpriteSheet spriteSheet, Game game, SpriteFont bonusFont, Vector2 position, int bonusRank)
        {
            BombScore bombScore = new BombScore(spriteSheet, game, bonusFont);
            bombScore.Spawn(position, bonusRank);
        }

        public static void ClearScores(Game game)
        {
            foreach(BombScore score in _bombScores)
            {
                game.Components.Remove(score);
            }
            _bombScores.Clear();
        }

        public BombScore(SpriteSheet spriteSheet, Game game, SpriteFont bonusFont) : base(spriteSheet, game)
        {
            _bonusFont = bonusFont;
        }

        public void Spawn(Vector2 position, int bonusRank)
        {
            _bonusRank = bonusRank;
            _bonusColorIndex = 0;
            MoveTo(position);
            Game.Components.Add(this);
            _animationCount = 0;
            _bombScores.Add(this);
            SetAnimation("Idle", onAnimationEnd: CountAnimations);
        }

        private void CountAnimations()
        {
            _animationCount++;
            if (_animationCount > ANIMATION_COUNT)
            {
                _bombScores.Remove(this);
                Game.Components.Remove(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (_bonusRank > 0)
            {
                _bonusColorIndex = (_bonusColorIndex + _colorChangeSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds) % _bonusColors.Length;
                SpriteBatch.DrawString(_bonusFont, $"x{_bonusRank + 1}", Position + new Vector2(-1, 6), _bonusColors[(int)_bonusColorIndex]);
            }
        }
    }
}
