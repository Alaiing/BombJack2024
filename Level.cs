using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BombJack2024
{
    public class Level
    {
        private OudidonGame _game;
        private SpriteSheet _bombSheet;
        private readonly List<Platform> _platforms = new();
        public List<Platform> Plateforms => _platforms;
        private readonly List<Bomb> _bombs = new();
        public List<Bomb> Bombs => _bombs;
        private bool _bombLit;
        private int _bombsFound;

        private int _backgroundIndex;
        public int BackgroundIndex => _backgroundIndex;

        public Level(OudidonGame game, string asset)
        {
            _game = game;

            _bombSheet = new SpriteSheet(game.Content, "bomb", 6, 13, Point.Zero);
            _bombSheet.RegisterAnimation(Bomb.ANIMATION_IDLE, 0, 0, 1);
            _bombSheet.RegisterAnimation(Bomb.ANIMATION_LIT, 1, 2, 20f);

            LoadData(asset);
        }

        public void LoadData(string dataPath)
        {
            if (!System.IO.File.Exists(dataPath))
                return;

            string[] lines = System.IO.File.ReadAllLines(dataPath);

            try
            {
                foreach (string line in lines)
                {
                    string[] split = line.Split('=');
                    string dataName = split[0].Trim();
                    string dataValue = split[1].Trim();

                    switch (dataName.ToUpper())
                    {
                        case "BACKGROUND":
                            _backgroundIndex = int.Parse(dataValue.Trim());
                            break;
                        case "BOMBS":
                            string[] coords = dataValue.Split(';');
                            for (int i = 0; i < coords.Length; i++)
                            {
                                string[] coordValues = coords[i].Split(",");
                                int x = int.Parse(coordValues[0].Trim());
                                int y = int.Parse(coordValues[1].Trim());
                                AddBomb(x, y, new Bomb(_bombSheet, _game));
                            }
                            break;
                        case "PLATFORMS":
                            string[] data = dataValue.Split(';');
                            for (int i = 0; i < data.Length; i++)
                            {
                                string[] coordValues = data[i].Split(",");
                                int x = int.Parse(coordValues[0].Trim());
                                int y = int.Parse(coordValues[1].Trim());
                                int size = int.Parse(coordValues[2].Trim());
                                bool horizontal = coordValues.Length < 4 || coordValues[3].Trim().ToUpper() == "H";

                                AddPlatform(new Point(x, y), size, horizontal);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void AddPlatform(Point position, int size, bool horizontal = true)
        {
            Platform plateform = new Platform(_game, position, size, horizontal);
            _platforms.Add(plateform);
        }

        public void AddBomb(int x, int y, Bomb bomb)
        {
            _bombs.Add(bomb);
            bomb.MoveTo(new Vector2(x, y));
        }

        public bool PickUpBomb(int index)
        {
            _bombs[index].Deactivate();
            _bombsFound++;

            if (_bombsFound == _bombs.Count)
            {
                return true;
            }

            if (!_bombLit || _bombs[index].IsLit)
            {
                for (int i = 1; i < _bombs.Count; i++)
                {
                    int roundIndex = (index + i) % _bombs.Count;
                    if (_bombs[roundIndex].Enabled)
                    {
                        _bombs[roundIndex].Light();
                        _bombLit = true;
                        return false;
                    }
                }
            }

            return false;
        }

        public void Activate()
        {
            foreach (Platform plateform in _platforms)
            {
                _game.Components.Add(plateform);
            }

            foreach (Bomb bomb in _bombs)
            {
                _game.Components.Add(bomb);
            }
        }

        public void DeactivateLevel()
        {
            foreach (Platform plateform in _platforms)
            {
                _game.Components.Remove(plateform);
            }

            foreach (Bomb bomb in _bombs)
            {
                _game.Components.Remove(bomb);
            }
        }
    }
}
