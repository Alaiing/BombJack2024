using Microsoft.Xna.Framework;
using Oudidon;
using System.Collections.Generic;

namespace BombJack2024
{
    public class Level
    {
        private OudidonGame _game;
        private readonly List<Platform> _platforms = new();
        public List<Platform> Plateforms => _platforms;

        public Level(OudidonGame game)
        {
            _game = game;
        }

        public void AddPlatform(Point position, int size, bool horizontal = true)
        {
            Platform plateform = new Platform(_game, position, size, horizontal);
            _platforms.Add(plateform);
        }

        public void Activate()
        {
            foreach(Platform plateform in _platforms)
            {
                _game.Components.Add(plateform);
            }
        }

        public void DeactivateLevel()
        {
            foreach(Platform plateform in _platforms)
            {
                _game.Components.Remove(plateform);
            }
        }
    }
}
