using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        private readonly List<Enemy> _enemies = new();
        public List<Enemy> Enemies => _enemies;

        private bool _bombLit;
        private int _bombsFound;

        private SoundEffect _bombPickupUnlit;
        private SoundEffectInstance _bombPickupUnlitInstance;
        private SoundEffect _bombPickupLit;
        private SoundEffectInstance _bombPickupLitInstance;
        private int _backgroundIndex;
        public int BackgroundIndex => _backgroundIndex;

        private Vector2 _robotSpawn;
        public Vector2 RobotSpawn => _robotSpawn;

        public float LevelTime { get; set; }

        private static Bird _bird;
        private static Point[] _birdStartPositions = new Point[]
        {
            new Point(116,184 + 9), // bottom right
            new Point(116,20 + 8), // top right
            new Point(6,7 + 9), // top left
            new Point(6,184 + 9), // bottom left
        };


        public Level(OudidonGame game, string asset)
        {
            _game = game;

            _bombSheet = new SpriteSheet(game.Content, "bomb", 6, 13, Point.Zero);
            _bombSheet.RegisterAnimation(Bomb.ANIMATION_IDLE, 0, 0, 1);
            _bombSheet.RegisterAnimation(Bomb.ANIMATION_LIT, 1, 2, 20f);

            _bombPickupUnlit = game.Content.Load<SoundEffect>("zboui");
            _bombPickupUnlitInstance = _bombPickupUnlit.CreateInstance();
            _bombPickupLit = game.Content.Load<SoundEffect>("piou");
            _bombPickupLitInstance = _bombPickupLit.CreateInstance();

            LoadData(asset);
        }

        public void Start(BombJack bombJack)
        {
            _bird.SetBombJack(bombJack);
            SpawnBird();
            LevelTime = 0;
        }

        private void SpawnBird()
        {
            int positionIndex = CommonRandom.Random.Next(_birdStartPositions.Length);
            _bird.MoveTo(_birdStartPositions[positionIndex].ToVector2());
            AddEnemy(_bird);
            _bird.Activate();
        }

        public void Update()
        {
            // TODO: spawn robots
        }

        public void Reset()
        {
            _bombsFound = 0;
            ResetBombs();
            ClearEnemies();
        }

        public void Restart()
        {
            TurnOffBombs();
            ClearEnemies();
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
                    string[] coordValues;
                    switch (dataName.ToUpper())
                    {
                        case "BACKGROUND":
                            _backgroundIndex = int.Parse(dataValue.Trim());
                            break;
                        case "BOMBS":
                            string[] coords = dataValue.Split(';');
                            for (int i = 0; i < coords.Length; i++)
                            {
                                coordValues = coords[i].Split(",");
                                int x = int.Parse(coordValues[0].Trim());
                                int y = int.Parse(coordValues[1].Trim());
                                AddBomb(x, y, new Bomb(_bombSheet, _game));
                            }
                            break;
                        case "ROBOT_SPAWN":
                            coordValues = dataValue.Split(",");
                            _robotSpawn = new Vector2(int.Parse(coordValues[0]), int.Parse(coordValues[1]));
                            break;
                        case "PLATFORMS":
                            string[] data = dataValue.Split(';');
                            for (int i = 0; i < data.Length; i++)
                            {
                                coordValues = data[i].Split(",");
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

            if (_bird == null)
            {
                SpriteSheet birdSprite = new SpriteSheet(_game.Content, "bird", 7, 10, new Point(4, 9));
                birdSprite.AddLayer(_game.Content, "bird_eye");
                _bird = new Bird(birdSprite, _game);
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

        private void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
            _game.Components.Add(enemy);
            enemy.Activate();
        }

        private void ClearEnemies()
        {
            foreach (Enemy enemy in _enemies)
            {
                enemy.Deactivate();
                _game.Components.Remove(enemy);
            }

            _enemies.Clear();
        }

        public void FreezeEnemies(bool freeze)
        {
            foreach(Enemy enemy in _enemies)
            {
                enemy.Enabled = !freeze;
            }
        }

        public void RespawnEnemies()
        {
            if (!_enemies.Contains(_bird))
            {
                SpawnBird();
            }
        }

        public bool PickUpBomb(int index)
        {
            _bombs[index].Deactivate();
            _bombsFound++;

            SoundEffectInstance soundInstance = _bombs[index].IsLit ? _bombPickupLit.CreateInstance() : _bombPickupUnlit.CreateInstance();

            soundInstance.Pan = CommonRandom.Random.Next(-1, 2);
            soundInstance.Play();

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
                        _bombs[roundIndex].TurnOn();
                        _bombLit = true;
                        return false;
                    }
                }
            }

            return false;
        }

        private void ResetBombs()
        {
            _bombLit = false;
            foreach (Bomb bomb in _bombs)
            {
                bomb.TurnOff();
                bomb.Activate();
            }
        }

        private void TurnOffBombs()
        {
            _bombLit = false;
            foreach (Bomb bomb in _bombs)
            {
                bomb.TurnOff();
            }
        }

        public void Activate()
        {
            _bird.CurrentLevel = this;
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

            _bird.Deactivate();
        }

        public bool TestPlatformCollision(Character character, out Platform hitPlatform)
        {
            Rectangle bounds = character.GetBounds();
            bounds.Y += 3;
            bounds.Height -= 3;
            foreach (Platform platform in Plateforms)
            {
                if (MathUtils.OverlapsWith(bounds, platform.Bounds))
                {
                    hitPlatform = platform;
                    return true;
                }
            }
            hitPlatform = null;
            return false;
        }

        public bool IsOnPlatform(Character character, out Platform hitPlatform, bool partial)
        {
            foreach (Platform platform in Plateforms)
            {
                bool leftOnPlatform = platform.Bounds.Contains(character.Position + new Vector2(-character.SpriteSheet.LeftMargin, 1));
                bool rightOnPlatform = platform.Bounds.Contains(character.Position + new Vector2(character.SpriteSheet.RightMargin, 1));
                if (partial && (leftOnPlatform || rightOnPlatform) || leftOnPlatform && rightOnPlatform)
                {
                    hitPlatform = platform;
                    return true;
                }
            }
            hitPlatform = null;
            return false;
        }

    }
}
