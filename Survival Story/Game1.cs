using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Entradas;
using System;
using System.Linq;
using System.Diagnostics;

public class Camera2D
{
    public Matrix Transform { get; private set; }
    public Vector2 Position { get; set; }

    private GraphicsDevice _graphicsDevice;

    public Camera2D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        Position = Vector2.Zero;
    }

    public void Follow(Vector2 targetPosition, Viewport viewport)
    {
        Position = new Vector2(
            targetPosition.X - viewport.Width / 2f,
            targetPosition.Y - viewport.Height / 2f
        );

        Transform = Matrix.CreateTranslation(new Vector3(-Position, 0));
    }
}

namespace Survival_Story
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _player, _rock, _topTree, _bottomTree, _sand, _map, _woodPicaxe, _woodAxe, _woodShovel, _inventory, _menu, _select_play, _select_exit, _select_controls, _controls, _rockDrop, _sandDrop, _mapNight, _oveja, _toro, _pixel, _casa;
        private bool casaColocada = false;
        private SpriteFont _font;
        private string _userName = string.Empty;
        private bool _nameFieldActive;
        private readonly Rectangle _nameInputBounds = new Rectangle(37, 410, 288, 44);
        private float dayTimer = 0f;
        private float dayDuration = 5.0f;
        private int currentDay = 0;
        private bool isNight = false;
        private float playerHealth = 100f;
        private float playerHunger = 100f;
        private float playerEnergy = 100f;
        private string playerMoral = "Bueno";
        private float hungerDecayRate = 0.25f;
        private float energyDecayRate = 0.20f;
        private float healthRegenRate = 2.0f;
        private Camera2D _camera;
        private Random _random;

        private Vector2 ovejaPosition, ovejaInicio, ovejaDestino, casaPosition;
        private int UltimoMov = 0;
        private bool _esperando = true;
        private float _timer = 0f;
        private float _delay = 1f;
        private int ovejaFrame = 1;
        private int ovejaDireccionSprite = 0;
        private float ovejaAnimTimer = 0f;
        private float ovejaAnimSpeed = 0.12f;
        private bool ovejaMoviendo = false;
        private float ovejaMoveTimer = 0f;
        private float ovejaMoveDuration = 0.3f;
        private int ovejaDireccionHuir = -1;
        private int ovejaMovimientosHuir = 0;

        private Vector2 toroPosition, toroInicio, toroDestino;
        private int toroUltimoMov = 0;
        private bool toroEsperando = true;
        private float toroTimer = 0f;
        private float toroDelay = 1f;
        private int toroFrame = 1;
        private int toroDireccionSprite = 0;
        private float toroAnimTimer = 0f;
        private float toroAnimSpeed = 0.12f;
        private bool toroMoviendo = false;
        private float toroMoveTimer = 0f;
        private float toroMoveDuration = 0.3f;
        private int toroDireccionAcercarse = -1;
        private int toroMovimientosAcercarse = 0;

        private Vector2 movement, playerPosition, toolPosition, mapPosition;
        private Vector2[] rockPosition, treePosition, sandPosition;

        private bool[] rockActive, treeActive, sandActive;//entidades
        private bool menuOpen, controlsOpen, selectPlay, selectExit, selectExitControls, selectControls;//menu
        private bool onGrass, onMountain;
        private bool[] toolEquipped = new bool[3];//herramienta

        private float velocidadGlobal = 250.0f;

        private int playerWidth = 64;
        private int playerHeight = 64;
        private int toolIndicate;
        private int[] inventoryStorage = new int[10];
        private Rectangle[] inventorySlot = new Rectangle[10];

        private float playerTimer;
        private int threshold;

        private Rectangle[,] playerRectangle;

        private byte previousAnimationIndex;
        private byte currentAnimationIndex;
        private int playerDirection;

        private bool _openInventory = false;
        private MouseState _curMS, _oldMS;
        private KeyboardState _curKS, _oldKS;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.TextInput += OnTextInput;
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (!menuOpen || controlsOpen || !_nameFieldActive || _userName.Length >= 16)
            {
                return;
            }

            char character = e.Character;
            bool isAsciiLetterOrDigit =
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');

            if (isAsciiLetterOrDigit || (character == ' ' && _userName.Length > 0 && !_userName.EndsWith(" ")))
            {
                _userName += character;
            }
        }

        private Vector2 pos(int posX, int posY)
        {
            int grPosX = 71;
            int grPosY = 71;
            return new Vector2(grPosX * posX, grPosY * posY);
        }

        private int Epos(int pos)
        {
            int PosEs = 71;
            pos = pos * PosEs;
            return pos;
        }

        protected override void Initialize()
        {
            Rectangle grassZone = new Rectangle(Epos(9), Epos(7), Epos(22), Epos(22));
            Rectangle mountainZone1 = new Rectangle(Epos(11), Epos(10), Epos(4), Epos(4));
            Rectangle mountainZone2 = new Rectangle(Epos(15), Epos(17), Epos(4), Epos(4));
            Rectangle mountainZone3 = new Rectangle(Epos(24), Epos(9), Epos(4), Epos(4));
            Rectangle mountainZone4 = new Rectangle(Epos(23), Epos(22), Epos(4), Epos(4));

            for (int i = 0; i < inventoryStorage.Length; i++)
            {
                inventoryStorage[i] = 0;
            }
            inventoryStorage[0] = 2;
            inventoryStorage[1] = 1;
            inventoryStorage[2] = 3;


            playerPosition = pos(10, 10);
            ovejaPosition = pos(10, 10);
            toroPosition = pos(20, 20);
            menuOpen = true;

            _camera = new Camera2D(GraphicsDevice);

            _random = new Random();

            int cantSand = 15;
            int cantRock = 10;
            int cantTree = 30;

            rockPosition = new Vector2[cantRock];
            rockActive = new bool[cantRock];

            treePosition = new Vector2[cantTree];
            treeActive = new bool[cantTree];

            sandPosition = new Vector2[cantSand];
            sandActive = new bool[cantSand];

            int minCol = 7;
            int maxCol = 33;
            int minFila = 5;
            int maxFila = 31;

            for (int i = 0; i < cantRock; i++)
            {
                int rockY, rockX;
                onMountain = false;
                var positionTry = Vector2.Zero;
                while (onMountain == false)
                {
                    rockX = _random.Next(minCol, maxCol);
                    rockY = _random.Next(minFila, maxFila);
                    positionTry = pos(rockX, rockY);
                    if (mountainZone1.Contains(positionTry) && !rockPosition.Take(i).Contains(positionTry) || mountainZone2.Contains(positionTry) && !rockPosition.Take(i).Contains(positionTry) || mountainZone3.Contains(positionTry) && !rockPosition.Take(i).Contains(positionTry) || mountainZone4.Contains(positionTry) && !rockPosition.Take(i).Contains(positionTry))
                    {
                        onMountain = true;
                    }
                    else
                    {
                        onMountain = false;
                    }
                }
                rockPosition[i] = positionTry;
                rockActive[i] = true;
            }

            for (int i = 0; i < cantTree; i++)
            {
                int treeY, treeX;
                onGrass = false;
                var positionTry = Vector2.Zero;
                while (onGrass == false)
                {
                    treeX = _random.Next(minCol, maxCol);
                    treeY = _random.Next(minFila, maxFila);
                    positionTry = pos(treeX, treeY);
                    if (grassZone.Contains(positionTry) && !mountainZone1.Contains(positionTry) && !mountainZone2.Contains(positionTry) && !mountainZone3.Contains(positionTry) && !mountainZone4.Contains(positionTry) && !treePosition.Take(i).Contains(positionTry))
                    {
                        onGrass = true;
                    }
                    else
                    {
                        onGrass = false;
                    }
                }
                treePosition[i] = positionTry;
                treeActive[i] = true;
            }

            for (int i = 0; i < cantSand; i++)
            {
                int sandY, sandX;
                onGrass = true;
                var positionTry = Vector2.Zero;
                while (onGrass == true)
                {
                    sandX = _random.Next(minCol, maxCol);
                    sandY = _random.Next(minFila, maxFila);
                    positionTry = pos(sandX, sandY);
                    if (grassZone.Contains(positionTry) || sandPosition.Take(i).Contains(positionTry))
                    {
                        onGrass = true;
                    }
                    else
                    {
                        onGrass = false;
                    }
                }
                sandPosition[i] = positionTry;
                sandActive[i] = true;
            }

            mapPosition = new Vector2(0, 0);
            playerTimer = 0;
            threshold = 250;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _player = Content.Load<Texture2D>("assets/images/playerSheet");
            _rock = Content.Load<Texture2D>("assets/images/rock");
            _rockDrop = Content.Load<Texture2D>("assets/images/rockDrop");
            _sand = Content.Load<Texture2D>("assets/images/sand");
            _sandDrop = Content.Load<Texture2D>("assets/images/sandDrop");
            _topTree = Content.Load<Texture2D>("assets/images/topTree");
            _bottomTree = Content.Load<Texture2D>("assets/images/bottomTree");
            _map = Content.Load<Texture2D>("assets/images/map");
            _inventory = Content.Load<Texture2D>("assets/images/inventory");
            _menu = Content.Load<Texture2D>("assets/images/menu");
            _woodPicaxe = Content.Load<Texture2D>("assets/images/woodPicaxe");
            _woodAxe = Content.Load<Texture2D>("assets/images/woodAxe");
            _woodShovel = Content.Load<Texture2D>("assets/images/woodShovel");
            _select_controls = Content.Load<Texture2D>("assets/images/controlButtons");
            _select_play = Content.Load<Texture2D>("assets/images/play");
            _select_exit = Content.Load<Texture2D>("assets/images/quit");
            _controls = Content.Load<Texture2D>("assets/images/controls");
            _oveja = Content.Load<Texture2D>("assets/images/Oveja");
            _toro = Content.Load<Texture2D>("assets/images/Toro");
            _mapNight = Content.Load<Texture2D>("assets/images/mapNight");
            _font = Content.Load<SpriteFont>("assets/fonts/font");
            _casa = Content.Load<Texture2D>("assets/images/house");
            playerRectangle = new Rectangle[8, 3];
            int playerWidth = 32;
            int playerHeight = 32;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    playerRectangle[i, j] = new Rectangle(j * playerWidth, i * playerHeight, playerWidth, playerHeight);
                }
            }
            inventorySlot[0] = new Rectangle(128, 138, 70, 70);
            inventorySlot[1] = new Rectangle(247, 138, 70, 70);
            inventorySlot[2] = new Rectangle(366, 138, 70, 70);
            inventorySlot[3] = new Rectangle(485, 138, 70, 70);
            inventorySlot[4] = new Rectangle(597, 138, 70, 70);
            inventorySlot[5] = new Rectangle(128, 250, 70, 70);
            inventorySlot[6] = new Rectangle(247, 250, 70, 70);
            inventorySlot[7] = new Rectangle(366, 250, 70, 70);
            inventorySlot[8] = new Rectangle(485, 250, 70, 70);
            inventorySlot[9] = new Rectangle(597, 250, 70, 70);
        }


        void playerStats(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            playerHunger -= hungerDecayRate * deltaTime;
            playerEnergy -= energyDecayRate * deltaTime;

            playerHunger = MathHelper.Clamp(playerHunger, 0f, 100f);
            playerEnergy = MathHelper.Clamp(playerEnergy, 0f, 100f);

            if (playerHunger <= 0f)
            {
                playerHealth -= 5f * deltaTime;
            }

            if (playerHealth <= 0f)
            {
                playerHealth = 0f;
            }
        }

        void timeDay(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            dayTimer += deltaTime;
            if (dayTimer >= dayDuration)
            {
                dayTimer = 0f;
                currentDay++;
            }
            isNight = dayTimer >= (dayDuration / 2f);
        }

        protected override void Update(GameTime gameTime)
        {
            _oldKS = _curKS;
            _curKS = Keyboard.GetState();
            _oldMS = _curMS;
            _curMS = Mouse.GetState();
            bool singleClick = _curMS.LeftButton == ButtonState.Pressed && _oldMS.LeftButton == ButtonState.Released;
            _camera.Follow(Vector2.Zero, GraphicsDevice.Viewport);
            if (_curKS.IsKeyDown(Keys.Escape) && _oldKS.IsKeyUp(Keys.Escape))
            {
                if (menuOpen == true)
                {
                    Exit();
                }
                else
                {
                    menuOpen = true;
                    ConnectDB.RegistrarJugador(1, "javier");
                }
            }
            Rectangle buttonPlay = new Rectangle(37, 90, 288, 96);
            Rectangle buttonControls = new Rectangle(91, 192, 288, 96);
            Rectangle buttonExit = new Rectangle(145, 294, 288, 96);
            Rectangle buttonExitControls = new Rectangle(345, 348, 288, 96);
            if (menuOpen)
            {
                selectPlay = false;
                selectControls = false;
                selectExit = false;
                selectExitControls = false;

                if (!controlsOpen)
                {
                    if (singleClick)
                    {
                        _nameFieldActive = _nameInputBounds.Contains(_curMS.Position);
                    }

                    if (_nameFieldActive && _curKS.IsKeyDown(Keys.Back) && _oldKS.IsKeyUp(Keys.Back) && _userName.Length > 0)
                    {
                        _userName = _userName.Substring(0, _userName.Length - 1);
                    }

                    if (buttonPlay.Contains(_curMS.Position))
                    {
                        selectPlay = true;
                        if (singleClick) menuOpen = false;
                    }
                    if (buttonControls.Contains(_curMS.Position))
                    {
                        selectControls = true;
                        if (singleClick) controlsOpen = true;
                    }
                    if (buttonExit.Contains(_curMS.Position))
                    {
                        selectExit = true;
                        if (singleClick) Exit();
                    }
                }
                else if (controlsOpen)
                {
                    if (buttonExitControls.Contains(_curMS.Position))
                    {
                        selectExitControls = true;
                        if (singleClick) controlsOpen = false;
                    }
                }
            }
            else if (!menuOpen)
            {
                inventory();
                movementPlayer(gameTime);
                collision(gameTime);
                interact();
                colocarCasa();
                actualizarHerramienta();
                AnimalLogic(gameTime);
                ToroLogic(gameTime);
                playerStats(gameTime);
                timeDay(gameTime);
                _camera.Follow(new Vector2(playerPosition.X + playerWidth / 2, playerPosition.Y + playerHeight / 2), GraphicsDevice.Viewport);
            }
            base.Update(gameTime);
        }
        private void drawResource(Texture2D res, Texture2D resDrop, Vector2[] resPosition, bool[] resActive)
        {
            for (int i = 0; i < resPosition.Length; i++)
            {
                if (resActive[i])
                {
                    _spriteBatch.Draw(res, resPosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                }
                else
                {
                    _spriteBatch.Draw(resDrop, resPosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                }
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _camera.Transform);
            _spriteBatch.Draw(_map, mapPosition, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            drawResource(_rock, _rockDrop, rockPosition, rockActive);
            drawResource(_sand, _sandDrop, sandPosition, sandActive);

            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeActive[i] == true)
                {
                    _spriteBatch.Draw(_bottomTree, treePosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.3f);
                    _spriteBatch.Draw(_topTree, treePosition[i] - new Vector2(Epos(0), Epos(1)), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
                }
            }
            if (casaColocada)
            {
                _spriteBatch.Draw(_casa, casaPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.35f);
            }
            Rectangle areaVision = Rectangle.Empty;
            switch (UltimoMov)
            {
                case 0:
                    areaVision = new Rectangle((int)ovejaPosition.X, (int)ovejaPosition.Y - Epos(1), Epos(3), Epos(3));
                    break;
                case 1:
                    areaVision = new Rectangle((int)ovejaPosition.X - Epos(2), (int)ovejaPosition.Y - Epos(1), Epos(3), Epos(3));
                    break;
                case 2:
                    areaVision = new Rectangle((int)ovejaPosition.X - Epos(1), (int)ovejaPosition.Y, Epos(3), Epos(3));
                    break;
                case 3:
                    areaVision = new Rectangle((int)ovejaPosition.X - Epos(1), (int)ovejaPosition.Y - Epos(2), Epos(3), Epos(3));
                    break;
            }
            Rectangle areaVisionToro = Rectangle.Empty;
            switch (toroUltimoMov)
            {
                case 0:
                    areaVisionToro = new Rectangle((int)toroPosition.X, (int)toroPosition.Y - Epos(1), Epos(3), Epos(3));
                    break;
                case 1:
                    areaVisionToro = new Rectangle((int)toroPosition.X - Epos(2), (int)toroPosition.Y - Epos(1), Epos(3), Epos(3));
                    break;
                case 2:
                    areaVisionToro = new Rectangle((int)toroPosition.X - Epos(1), (int)toroPosition.Y, Epos(3), Epos(3));
                    break;
                case 3:
                    areaVisionToro = new Rectangle((int)toroPosition.X - Epos(1), (int)toroPosition.Y - Epos(2), Epos(3), Epos(3));
                    break;
            }
            _spriteBatch.Draw(_pixel, areaVisionToro, null, new Color(255, 70, 70, 55), 0f, Vector2.Zero, SpriteEffects.None, 0.251f);
            _spriteBatch.Draw(_pixel, areaVision, null, new Color(255, 220, 80, 45), 0f, Vector2.Zero, SpriteEffects.None, 0.25f);
            int anchoOveja = _oveja.Width / 3;
            int altoOveja = _oveja.Height / 8;
            Rectangle ovejaSource = new Rectangle(
            ovejaFrame * anchoOveja,
            ovejaDireccionSprite * altoOveja,
            anchoOveja,
            altoOveja
            );
            _spriteBatch.Draw(_oveja, ovejaPosition + new Vector2(12, 7), ovejaSource, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.297f);
            int anchoToro = _toro.Width / 3;
            int altoToro = _toro.Height / 4;
            Rectangle toroSource = new Rectangle(
            toroFrame * anchoToro,
            toroDireccionSprite * altoToro,
            anchoToro,
            altoToro
            );
            _spriteBatch.Draw(_toro, toroPosition + new Vector2(2, 2), toroSource, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.296f);
            _spriteBatch.Draw(_player, playerPosition, playerRectangle[playerDirection, currentAnimationIndex], Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.298f);
            if (toolEquipped[0] == true)
            {
                _spriteBatch.Draw(_woodPicaxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.299f);
            }
            else if (toolEquipped[1] == true)
            {
                _spriteBatch.Draw(_woodAxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.299f);
            }
            else if (toolEquipped[2] == true)
            {
                _spriteBatch.Draw(_woodShovel, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.299f);
            }


            _spriteBatch.End();
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            Color colorDay = Color.Transparent;
            Color colorNight = new Color(10, 15, 45) * 0.75f;
            float dayProgress = dayTimer / dayDuration;
            float nightIntensity = (MathF.Sin(dayProgress * MathF.PI * 2f - MathF.PI / 2f) + 1f) / 2f;
            Color actualTime = Color.Lerp(colorDay, colorNight, nightIntensity);
            Rectangle fullScreen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_mapNight, fullScreen, null, actualTime, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
            if (_openInventory == true)
            {
                _spriteBatch.Draw(_inventory, new Vector2((GraphicsDevice.Viewport.Width / 2) - _inventory.Width / 2 * 7, (GraphicsDevice.Viewport.Height / 2) - _inventory.Height / 2 * 7), null, Color.White, 0.0f, Vector2.Zero, 7.0f, SpriteEffects.None, 0.8f); //588x238
                for (int i = 0; i < inventoryStorage.Length; i++)
                {
                    if (inventoryStorage[i] == 1)
                    {
                        _spriteBatch.Draw(_topTree, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                    if (inventoryStorage[i] == 2)
                    {
                        _spriteBatch.Draw(_rockDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                    if (inventoryStorage[i] == 3)
                    {
                        _spriteBatch.Draw(_sandDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                }
            }
            if (menuOpen == true)
            {
                _spriteBatch.Draw(_menu, new Vector2(0, 0), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.98f);
                if (!controlsOpen)
                {
                    Color nameFieldBorder = _nameFieldActive ? Color.Cyan : Color.LightGray;
                    _spriteBatch.DrawString(_font, "Nombre de usuario", new Vector2(37, 392), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.995f);
                    _spriteBatch.Draw(_pixel, _nameInputBounds, null, nameFieldBorder, 0f, Vector2.Zero, SpriteEffects.None, 0.996f);
                    _spriteBatch.Draw(_pixel, new Rectangle(_nameInputBounds.X + 2, _nameInputBounds.Y + 2, _nameInputBounds.Width - 4, _nameInputBounds.Height - 4), null, new Color(25, 28, 52), 0f, Vector2.Zero, SpriteEffects.None, 0.997f);
                    string nameText = _userName.Length == 0 ? "Haz clic para escribir..." : _userName;
                    _spriteBatch.DrawString(_font, nameText, new Vector2(_nameInputBounds.X + 10, _nameInputBounds.Y + 13), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.998f);
                }
                if (selectPlay == true)
                {
                    _spriteBatch.Draw(_select_play, new Vector2(37, 90), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (selectControls == true)
                {
                    _spriteBatch.Draw(_select_controls, new Vector2(91, 192), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (selectExit == true)
                {
                    _spriteBatch.Draw(_select_exit, new Vector2(145, 294), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (controlsOpen == true)
                {
                    _spriteBatch.Draw(_controls, new Vector2(0, 0), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                    if (selectExitControls)
                    {
                        _spriteBatch.Draw(_select_exit, new Vector2(345, 348), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.995f);
                    }
                }
            }
            string statsText = $"Salud: {(int)playerHealth} | Hambre: {(int)playerHunger} | Energia: {(int)playerEnergy} | Moral: {playerMoral}";
            _spriteBatch.DrawString(_font, statsText, new Vector2(20, 20), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.8f);

            string dayText = $"Dia: {currentDay}";
            Vector2 textSize = _font.MeasureString(dayText);
            float textX = GraphicsDevice.Viewport.Width - textSize.X - 20f;
            float textY = 20f;
            _spriteBatch.DrawString(_font, dayText, new Vector2(textX, textY), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        private void movementPlayer(GameTime gameTime)
        {
            movement = Vector2.Zero;
            if (_curKS.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (_curKS.IsKeyDown(Keys.S)) movement.Y += 1;
            if (_curKS.IsKeyDown(Keys.A)) movement.X -= 1;
            if (_curKS.IsKeyDown(Keys.D)) movement.X += 1;
            if (movement != Vector2.Zero)
            { //Normaliza el vector de movimiento para no acelerar de más en diagonal
                movement.Normalize();
                if (movement.X < 0 && movement.Y < 0) playerDirection = 6;
                else if (movement.X > 0 && movement.Y < 0) playerDirection = 7;
                else if (movement.X < 0 && movement.Y > 0) playerDirection = 4;
                else if (movement.X > 0 && movement.Y > 0) playerDirection = 5;
                else if (movement.Y < 0) playerDirection = 1;
                else if (movement.Y > 0) playerDirection = 0;
                else if (movement.X < 0) playerDirection = 2;
                else if (movement.X > 0) playerDirection = 3;
            }
            else
            {
                playerTimer = 0f;
            }
            animation(gameTime);
        }
        private void CheckCollisions(ref Vector2 playerPosition, ref Rectangle playerHitbox, Vector2[] objPosition, bool[] objActive, Texture2D obj, float movement, bool movementX)
        {
            for (int i = 0; i < objPosition.Length; i++)
            {
                if (!objActive[i]) continue;
                Rectangle objHitbox = new Rectangle((int)objPosition[i].X, (int)objPosition[i].Y, obj.Width, obj.Height);
                if (playerHitbox.Intersects(objHitbox))
                {
                    if (movementX)
                    {
                        if (movement > 0) playerPosition.X = objHitbox.Left - playerWidth;
                        else if (movement < 0) playerPosition.X = objHitbox.Right;
                        playerHitbox.X = (int)playerPosition.X;
                    }
                    else
                    {
                        if (movement > 0) playerPosition.Y = objHitbox.Top - playerHeight;
                        else if (movement < 0) playerPosition.Y = objHitbox.Bottom;
                        playerHitbox.Y = (int)playerPosition.Y;
                    }
                }
            }
        }
        private void collision(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rectangle windowEdge = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            playerPosition.X += movement.X * velocidadGlobal * delta;
            if (playerPosition.X < 497) playerPosition.X = 497;
            if (playerPosition.X > 2279) playerPosition.X = 2279;
            Rectangle playerHitboxX = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            CheckCollisions(ref playerPosition, ref playerHitboxX, rockPosition, rockActive, _rock, movement.X, true);
            CheckCollisions(ref playerPosition, ref playerHitboxX, treePosition, treeActive, _bottomTree, movement.X, true);
            CheckCollisions(ref playerPosition, ref playerHitboxX, sandPosition, sandActive, _sand, movement.X, true);
            playerPosition.Y += movement.Y * velocidadGlobal * delta;
            if (playerPosition.Y < 355) playerPosition.Y = 355;
            if (playerPosition.Y > 2137) playerPosition.Y = 2137;
            Rectangle playerHitboxY = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            CheckCollisions(ref playerPosition, ref playerHitboxY, rockPosition, rockActive, _rock, movement.Y, false);
            CheckCollisions(ref playerPosition, ref playerHitboxY, treePosition, treeActive, _bottomTree, movement.Y, false);
            CheckCollisions(ref playerPosition, ref playerHitboxY, sandPosition, sandActive, _sand, movement.Y, false);
        }
        private void inventory()
        {
            if (_curKS.IsKeyDown(Keys.Tab) && _oldKS.IsKeyUp(Keys.Tab))
            {
                _openInventory = !_openInventory;
            }
        }
        private bool PuedeMoverOveja(Vector2 destino)
        {
            // Crea el rectangulo de colision de la oveja en la posicion que quiere ocupar.
            Rectangle ovejaHitbox = new Rectangle((int)destino.X, (int)destino.Y, 64, 64);
            // Evita que la oveja salga de los limites del mapa.
            if (destino.X < 497 || destino.X > 2279 || destino.Y < 355 || destino.Y > 2137)
                return false;
            // Comprueba las piedras.
            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (rockActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)rockPosition[i].X, (int)rockPosition[i].Y, _rock.Width, _rock.Height);
                    if (ovejaHitbox.Intersects(obstaculo)) return false;
                }
            }
            // Comprueba la parte inferior de los arboles.
            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)treePosition[i].X, (int)treePosition[i].Y, _bottomTree.Width, _bottomTree.Height);
                    if (ovejaHitbox.Intersects(obstaculo)) return false;
                }
            }
            // Comprueba los montones de arena.
            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (sandActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)sandPosition[i].X, (int)sandPosition[i].Y, _sand.Width, _sand.Height);
                    if (ovejaHitbox.Intersects(obstaculo)) return false;
                }
            }
            return true;
        }
        // Controla el comportamiento de la oveja: movimiento, animacion, vision y huida.
        private void AnimalLogic(GameTime gameTime)
        {
            // Tiempo transcurrido desde el ultimo fotograma.
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Si la oveja ya se esta moviendo, actualiza su posicion y animacion.
            if (ovejaMoviendo)
            {
                ovejaMoveTimer += delta;
                ovejaAnimTimer += delta;
                // Calcula que parte del movimiento ya se completo, entre 0 y 1.
                float progreso = MathHelper.Clamp(ovejaMoveTimer / ovejaMoveDuration, 0f, 1f);
                ovejaPosition = Vector2.Lerp(ovejaInicio, ovejaDestino, progreso);
                // Cambia el cuadro de animacion cuando se cumple el tiempo establecido.
                if (ovejaAnimTimer >= ovejaAnimSpeed)
                {
                    ovejaFrame = (ovejaFrame + 1) % 3;
                    ovejaAnimTimer = 0f;
                }
                // Cuando llega al destino, termina el movimiento y prepara la siguiente espera.
                if (progreso >= 1f)
                {
                    ovejaPosition = ovejaDestino;
                    ovejaMoviendo = false;
                    ovejaFrame = 1;
                    _timer = 0f;
                    _esperando = true;
                    _delay = ovejaMovimientosHuir > 0 ? 0.2f : 1f;
                }
                return;
            }
            // La oveja espera un tiempo antes de decidir su siguiente movimiento.
            if (_esperando)
            {
                _timer += delta;
                if (_timer < _delay)
                    return;
                _esperando = false;
                _timer = 0f;
                // Obtiene un punto aproximado de la posicion del jugador para comprobar si esta dentro de la vision.
                Point jugador = new Point((int)(playerPosition.X + playerWidth / 2), (int)(playerPosition.Y + playerHeight / 2));
                // Crea cuatro rectangulos de vision, uno para cada direccion.
                Rectangle VisionDerecha = new Rectangle((int)ovejaPosition.X, (int)ovejaPosition.Y - Epos(1), Epos(3), Epos(3));
                Rectangle VisionIzquierda = new Rectangle((int)ovejaPosition.X - Epos(2), (int)ovejaPosition.Y - Epos(1), Epos(3), Epos(3));
                Rectangle VisionArriba = new Rectangle((int)ovejaPosition.X - Epos(1), (int)ovejaPosition.Y - Epos(2), Epos(3), Epos(3));
                Rectangle VisionAbajo = new Rectangle((int)ovejaPosition.X - Epos(1), (int)ovejaPosition.Y, Epos(3), Epos(3));
                // La oveja solo detecta al jugador en la direccion hacia la que esta mirando.
                bool vioDerecha = VisionDerecha.Contains(jugador) && UltimoMov == 0;
                bool vioIzquierda = VisionIzquierda.Contains(jugador) && UltimoMov == 1;
                bool vioArriba = VisionArriba.Contains(jugador) && UltimoMov == 3;
                bool vioAbajo = VisionAbajo.Contains(jugador) && UltimoMov == 2;
                // Si detecta al jugador, guarda la direccion contraria para comenzar a escapar.
                if (vioDerecha)
                {
                    ovejaDireccionHuir = 1;
                    ovejaMovimientosHuir = 11;
                }
                else if (vioIzquierda)
                {
                    ovejaDireccionHuir = 0;
                    ovejaMovimientosHuir = 11;
                }
                else if (vioArriba)
                {
                    ovejaDireccionHuir = 2;
                    ovejaMovimientosHuir = 11;
                }
                else if (vioAbajo)
                {
                    ovejaDireccionHuir = 3;
                    ovejaMovimientosHuir = 11;
                }
                // Guarda la direccion que se eligio para el siguiente movimiento.
                int mov;
                // Si quedan movimientos de huida, la oveja continua escapando
                bool ovejaHuyendo = ovejaMovimientosHuir > 0;
                // Mientras huye, tiene un 70 por ciento de probabilidad de seguir en la direccion de escape.
                if (ovejaHuyendo && _random.Next(0, 100) < 70)
                    mov = ovejaDireccionHuir;
                else
                    mov = _random.Next(0, 4);
                // Durante la huida se mueve mas rapido; cuando camina normalmente tarda mas.
                ovejaMoveDuration = ovejaHuyendo ? 0.01f : 0.3f;
                // Reduce la cantidad de movimientos de huida que quedan.
                if (ovejaMovimientosHuir > 0)
                    ovejaMovimientosHuir--;
                // Convierte la direccion elegida en un desplazamiento horizontal o vertical.
                Vector2 movimiento = Vector2.Zero;
                // Asigna el desplazamiento y la animacion que corresponden a cada direccion.
                switch (mov)
                {
                    case 0:
                        movimiento = new Vector2(1, 0);
                        UltimoMov = 0;
                        ovejaDireccionSprite = 2;
                        break;
                    case 1:
                        movimiento = new Vector2(-1, 0);
                        UltimoMov = 1;
                        ovejaDireccionSprite = 6;
                        break;
                    case 2:
                        movimiento = new Vector2(0, 1);
                        UltimoMov = 2;
                        ovejaDireccionSprite = 0;
                        break;
                    case 3:
                        movimiento = new Vector2(0, -1);
                        UltimoMov = 3;
                        ovejaDireccionSprite = 4;
                        break;
                }
                // Comprueba si la direccion elegida esta bloqueada.
                // Calcula la posicion de la siguiente casilla, usando el tamaño de la grilla.
                Vector2 destinoPropuesto = ovejaPosition + movimiento * Epos(1);
                // Si el camino elegido esta bloqueado, busca otra direccion que este libre.
                if (!PuedeMoverOveja(destinoPropuesto))
                {
                    // Indica si se encontro al menos una direccion alternativa disponible.
                    bool encontroCamino = false;
                    // Prepara las cuatro direcciones posibles y las mezcla para elegir al azar.
                    int[] direcciones = { 0, 1, 2, 3 };
                    direcciones = direcciones.OrderBy(x => _random.Next()).ToArray();
                    // Prueba cada direccion hasta encontrar una posicion sin obstaculos.
                    foreach (int direccion in direcciones)
                    {
                        Vector2 movimientoAlternativo = Vector2.Zero;
                        switch (direccion)
                        {
                            case 0: movimientoAlternativo = new Vector2(1, 0); break;
                            case 1: movimientoAlternativo = new Vector2(-1, 0); break;
                            case 2: movimientoAlternativo = new Vector2(0, 1); break;
                            case 3: movimientoAlternativo = new Vector2(0, -1); break;
                        }
                        // Calcula y comprueba la posicion alternativa.
                        Vector2 destinoAlternativo = ovejaPosition + movimientoAlternativo * Epos(1);
                        if (PuedeMoverOveja(destinoAlternativo))
                        {
                            movimiento = movimientoAlternativo;
                            destinoPropuesto = destinoAlternativo;
                            encontroCamino = true;
                            switch (direccion)
                            {
                                case 0: UltimoMov = 0; ovejaDireccionSprite = 2; break;
                                case 1: UltimoMov = 1; ovejaDireccionSprite = 6; break;
                                case 2: UltimoMov = 2; ovejaDireccionSprite = 0; break;
                                case 3: UltimoMov = 3; ovejaDireccionSprite = 4; break;
                            }
                            break;
                        }
                    }
                    // Si esta rodeada, se queda quieta.
                    if (!encontroCamino)
                    {
                        ovejaFrame = 1;
                        _timer = 0f;
                        _esperando = true;
                        _delay = 0.5f;
                        return;
                    }
                }
                // Guarda la posicion inicial y final para animar el desplazamiento suavemente.
                ovejaInicio = ovejaPosition;
                ovejaDestino = destinoPropuesto;
                ovejaMoveTimer = 0f;
                ovejaAnimTimer = 0f;
                ovejaFrame = 0;
                ovejaMoviendo = true;
            }
        }
        private bool PuedeMoverToro(Vector2 destino)
        {
            // Crea el rectangulo de colision de el toro en la posicion que quiere ocupar.
            Rectangle toroHitbox = new Rectangle((int)destino.X, (int)destino.Y, 64, 64);
            // Evita que el toro salga de los limites del mapa.
            if (destino.X < 497 || destino.X > 2279 || destino.Y < 355 || destino.Y > 2137)
                return false;
            // Comprueba las piedras.
            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (rockActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)rockPosition[i].X, (int)rockPosition[i].Y, _rock.Width, _rock.Height);
                    if (toroHitbox.Intersects(obstaculo)) return false;
                }
            }
            // Comprueba la parte inferior de los arboles.
            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)treePosition[i].X, (int)treePosition[i].Y, _bottomTree.Width, _bottomTree.Height);
                    if (toroHitbox.Intersects(obstaculo)) return false;
                }
            }
            // Comprueba los montones de arena.
            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (sandActive[i])
                {
                    Rectangle obstaculo = new Rectangle((int)sandPosition[i].X, (int)sandPosition[i].Y, _sand.Width, _sand.Height);
                    if (toroHitbox.Intersects(obstaculo)) return false;
                }
            }
            return true;
        }
        // Controla el comportamiento de el toro: movimiento, animacion, vision y acercamiento.
        private void ToroLogic(GameTime gameTime)
        {
            // Tiempo transcurrido desde el ultimo fotograma.
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Si el toro ya se esta moviendo, actualiza su posicion y animacion.
            if (toroMoviendo)
            {
                toroMoveTimer += delta;
                toroAnimTimer += delta;
                // Calcula que parte del movimiento ya se completo, entre 0 y 1.
                float progreso = MathHelper.Clamp(toroMoveTimer / toroMoveDuration, 0f, 1f);
                toroPosition = Vector2.Lerp(toroInicio, toroDestino, progreso);
                // Cambia el cuadro de animacion cuando se cumple el tiempo establecido.
                if (toroAnimTimer >= toroAnimSpeed)
                {
                    toroFrame = (toroFrame + 1) % 3;
                    toroAnimTimer = 0f;
                }
                // Cuando llega al destino, termina el movimiento y prepara la siguiente espera.
                if (progreso >= 1f)
                {
                    toroPosition = toroDestino;
                    toroMoviendo = false;
                    toroFrame = 1;
                    toroTimer = 0f;
                    toroEsperando = true;
                    toroDelay = toroMovimientosAcercarse > 0 ? 0.2f : 1f;
                }
                return;
            }
            // El toro espera un tiempo antes de decidir su siguiente movimiento.
            if (toroEsperando)
            {
                toroTimer += delta;
                if (toroTimer < toroDelay)
                    return;
                toroEsperando = false;
                toroTimer = 0f;
                // Obtiene un punto aproximado de la posicion del jugador para comprobar si esta dentro de la vision.
                Point jugador = new Point((int)(playerPosition.X + playerWidth / 2), (int)(playerPosition.Y + playerHeight / 2));
                // Crea cuatro rectangulos de vision, uno para cada direccion.
                Rectangle VisionDerecha = new Rectangle((int)toroPosition.X, (int)toroPosition.Y - Epos(1), Epos(3), Epos(3));
                Rectangle VisionIzquierda = new Rectangle((int)toroPosition.X - Epos(2), (int)toroPosition.Y - Epos(1), Epos(3), Epos(3));
                Rectangle VisionArriba = new Rectangle((int)toroPosition.X - Epos(1), (int)toroPosition.Y - Epos(2), Epos(3), Epos(3));
                Rectangle VisionAbajo = new Rectangle((int)toroPosition.X - Epos(1), (int)toroPosition.Y, Epos(3), Epos(3));
                // El toro solo detecta al jugador en la direccion hacia la que esta mirando.
                bool vioDerecha = VisionDerecha.Contains(jugador) && toroUltimoMov == 0;
                bool vioIzquierda = VisionIzquierda.Contains(jugador) && toroUltimoMov == 1;
                bool vioArriba = VisionArriba.Contains(jugador) && toroUltimoMov == 3;
                bool vioAbajo = VisionAbajo.Contains(jugador) && toroUltimoMov == 2;
                // Si detecta al jugador, guarda la direccion hacia el jugador para acercarse.
                if (vioDerecha)
                {
                    toroDireccionAcercarse = 0;
                    toroMovimientosAcercarse = 11;
                }
                else if (vioIzquierda)
                {
                    toroDireccionAcercarse = 1;
                    toroMovimientosAcercarse = 11;
                }
                else if (vioArriba)
                {
                    toroDireccionAcercarse = 3;
                    toroMovimientosAcercarse = 11;
                }
                else if (vioAbajo)
                {
                    toroDireccionAcercarse = 2;
                    toroMovimientosAcercarse = 11;
                }
                // Guarda la direccion que se eligio para el siguiente movimiento.
                int mov;
                // Si quedan movimientos de acercamiento, la toro continua acercandose
                bool toroAcercandose = toroMovimientosAcercarse > 0;
                // Mientras huye, tiene un 70 por ciento de probabilidad de seguir en la direccion de escape.
                if (toroAcercandose && _random.Next(0, 100) < 70)
                    mov = toroDireccionAcercarse;
                else
                    mov = _random.Next(0, 4);
                // Durante la acercamiento se mueve mas rapido; cuando camina normalmente tarda mas.
                toroMoveDuration = toroAcercandose ? 0.01f : 0.3f;
                // Reduce la cantidad de movimientos de acercamiento que quedan.
                if (toroMovimientosAcercarse > 0)
                    toroMovimientosAcercarse--;
                // Convierte la direccion elegida en un desplazamiento horizontal o vertical.
                Vector2 movimiento = Vector2.Zero;
                // Asigna el desplazamiento y la animacion que corresponden a cada direccion.
                switch (mov)
                {
                    case 0:
                        movimiento = new Vector2(1, 0);
                        toroUltimoMov = 0;
                        toroDireccionSprite = 1;
                        break;
                    case 1:
                        movimiento = new Vector2(-1, 0);
                        toroUltimoMov = 1;
                        toroDireccionSprite = 3;
                        break;
                    case 2:
                        movimiento = new Vector2(0, 1);
                        toroUltimoMov = 2;
                        toroDireccionSprite = 0;
                        break;
                    case 3:
                        movimiento = new Vector2(0, -1);
                        toroUltimoMov = 3;
                        toroDireccionSprite = 2;
                        break;
                }
                // Comprueba si la direccion elegida esta bloqueada.
                // Calcula la posicion de la siguiente casilla, usando el tamaño de la grilla.
                Vector2 destinoPropuesto = toroPosition + movimiento * Epos(1);
                // Si el camino elegido esta bloqueado, busca otra direccion que este libre.
                if (!PuedeMoverToro(destinoPropuesto))
                {
                    // Indica si se encontro al menos una direccion alternativa disponible.
                    bool encontroCamino = false;
                    // Prepara las cuatro direcciones posibles y las mezcla para elegir al azar.
                    int[] direcciones = { 0, 1, 2, 3 };
                    direcciones = direcciones.OrderBy(x => _random.Next()).ToArray();
                    // Prueba cada direccion hasta encontrar una posicion sin obstaculos.
                    foreach (int direccion in direcciones)
                    {
                        Vector2 movimientoAlternativo = Vector2.Zero;
                        switch (direccion)
                        {
                            case 0: movimientoAlternativo = new Vector2(1, 0); break;
                            case 1: movimientoAlternativo = new Vector2(-1, 0); break;
                            case 2: movimientoAlternativo = new Vector2(0, 1); break;
                            case 3: movimientoAlternativo = new Vector2(0, -1); break;
                        }
                        // Calcula y comprueba la posicion alternativa.
                        Vector2 destinoAlternativo = toroPosition + movimientoAlternativo * Epos(1);
                        if (PuedeMoverToro(destinoAlternativo))
                        {
                            movimiento = movimientoAlternativo;
                            destinoPropuesto = destinoAlternativo;
                            encontroCamino = true;
                            switch (direccion)
                            {
                                case 0: toroUltimoMov = 0; toroDireccionSprite = 1; break;
                                case 1: toroUltimoMov = 1; toroDireccionSprite = 3; break;
                                case 2: toroUltimoMov = 2; toroDireccionSprite = 0; break;
                                case 3: toroUltimoMov = 3; toroDireccionSprite = 2; break;
                            }
                            break;
                        }
                    }
                    // Si esta rodeada, se queda quieta.
                    if (!encontroCamino)
                    {
                        toroFrame = 1;
                        toroTimer = 0f;
                        toroEsperando = true;
                        toroDelay = 0.5f;
                        return;
                    }
                }
                // Guarda la posicion inicial y final para animar el desplazamiento suavemente.
                toroInicio = toroPosition;
                toroDestino = destinoPropuesto;
                toroMoveTimer = 0f;
                toroAnimTimer = 0f;
                toroFrame = 0;
                toroMoviendo = true;
            }
        }
        private void checkResource(ref Vector2 playerCenter, Vector2[] objPosition, Texture2D obj, bool[] objActive)
        {
            for (int i = 0; i < objPosition.Length; i++)
            {
                if (objActive[i])
                {
                    Vector2 objCenter = new Vector2(objPosition[i].X + (obj.Width / 2), objPosition[i].Y + (obj.Height / 2));
                    float distance = Vector2.Distance(playerCenter, objCenter);
                    if (distance < 80.0f)
                    {
                        objActive[i] = false;
                        break;
                    }
                }
            }
        }
        private void colocarCasa()
        {
            if (_curKS.IsKeyDown(Keys.C) && _oldKS.IsKeyUp(Keys.C))
            {
                switch (playerDirection)
                {
                    case 0: // abajo
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 1: // arriba
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                    case 2: // izquierda
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f;
                        casaColocada = true;
                        break;
                    case 3: // derecha
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f;
                        casaColocada = true;
                        break;
                    case 4: // abajo izquierda
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 5: // abajo derecha
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 6: // arriba izquierda
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                    case 7: // arriba derecha
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                }
            }
        }
        private void interact()
        { //35x36
            Vector2 playerCenter = new Vector2(playerPosition.X + (playerWidth / 2f), playerPosition.Y + (playerHeight / 2f));
            if (_curKS.IsKeyDown(Keys.R)) toolIndicate = 0;
            if (_curKS.IsKeyDown(Keys.T)) toolIndicate = 1;
            if (_curKS.IsKeyDown(Keys.Y)) toolIndicate = 2;
            if (_curKS.IsKeyDown(Keys.U)) toolIndicate = -1;
            for (int i = 0; i < toolEquipped.Length; i++)
            {
                toolEquipped[i] = (i == toolIndicate) ? true : false;
            }
            if (_curKS.IsKeyDown(Keys.E))
            {
                for (int i = 0; i < toolEquipped.Length; i++)
                {
                    if (toolEquipped[0])
                    {
                        checkResource(ref playerCenter, rockPosition, _rock, rockActive);
                    }
                    if (toolEquipped[1])
                    {
                        checkResource(ref playerCenter, treePosition, _bottomTree, treeActive);
                    }
                    if (toolEquipped[2])
                    {
                        checkResource(ref playerCenter, sandPosition, _sand, sandActive);
                    }
                }
            }
            toolPosition.X = playerPosition.X;
            toolPosition.Y = playerPosition.Y;
        }
        private void animation(GameTime gameTime)
        {
            playerTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (playerTimer > threshold)
            {
                if (currentAnimationIndex == 1)
                {
                    currentAnimationIndex = (previousAnimationIndex == 0) ? (byte)2 : (byte)0;
                    previousAnimationIndex = currentAnimationIndex;
                }
                else
                {
                    currentAnimationIndex = 1;
                }
                playerTimer -= threshold;
            }
        }
        private void actualizarHerramienta()
        {
            switch (playerDirection)
            {
                case 0:
                    toolPosition = playerPosition + new Vector2(5, 5);
                    break;
                case 1:
                    toolPosition = playerPosition + new Vector2(47, 0);
                    break;
                case 2:
                    toolPosition = playerPosition + new Vector2(20, 3);
                    break;
                case 3:
                    toolPosition = playerPosition + new Vector2(20, 27);
                    break;
                case 4:
                    toolPosition = playerPosition + new Vector2(3, 5);
                    break;
                case 5:
                    toolPosition = playerPosition + new Vector2(3, 20);
                    break;
                case 6:
                    toolPosition = playerPosition + new Vector2(38, 2);
                    break;
                case 7:
                    toolPosition = playerPosition + new Vector2(43, 12);
                    break;
            }
        }
    }
}