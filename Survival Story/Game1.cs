using System;
using System.Diagnostics;
using System.Linq;
using Entradas;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Configuration;

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
    public struct AnimalData
    {
        public string Nombre;
        public Vector2 Posicion;
        public Vector2 Inicio;
        public Vector2 Destino;

        // 1 = vivo, 2 = drop en el suelo, 0 = recogido
        public int Estado;
        public int Vida;
        public float HitCooldown;

        public int UltimoMov;
        public bool Esperando;
        public float Timer;
        public float Delay;

        public int Frame;
        public int DireccionSprite;
        public float AnimTimer;
        public float AnimSpeed;

        public bool Moviendo;
        public float MoveTimer;
        public float MoveDuration;
        public int DireccionObjetivo;
        public int MovimientosObjetivo;

        public bool HuyeDelJugador;
        public int[] FilasPorDireccion; // [derecha, izquierda, abajo, arriba]

        public Texture2D SpriteSheet;
        public Texture2D DropTexture;
        public Vector2 OffsetVisual;
        public float LayerDepth;
        public int ColumnasSprite;
        public int FilasSprite;
        public int DropItemId;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int[] rockState, treeState, sandState; // 1 = activo, 2 = drop en el suelo, 0 = recolectado
        private int rockCount = 0;
        private int treeCount = 0;
        private int sandCount = 0;
        private int meatCount = 0;

        private Texture2D _player, _rock, _topTree, _bottomTree, _sand, _map, _woodPicaxe, _woodAxe, _woodShovel, _inventory, _menu, _select_play, _select_exit, _select_controls, _controls, _rockDrop, _sandDrop, _treeDrop, _mapNight, _pantmuerte, _oveja, _toro, _pixel, _casa, _registroacciones, _meatDrop, _error;
        private bool casaColocada = false;
        private SpriteFont _font;
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

        private AnimalData oveja;
        private AnimalData toro;
        private Vector2 casaPosition;

        private Vector2 movement, toolPosition;
        private Vector2 mapPosition = new Vector2(0, 0);
        private Vector2 playerPosition = new Vector2(710, 710);
        private Vector2[] rockPosition, treePosition, sandPosition;

        private bool menuOpen, controlsOpen, selectPlay, selectExit, selectExitControls, selectControls, isDead;
        private bool onGrass, onMountain;
        private bool[] toolEquipped = new bool[3];

        private float velocidadGlobal = 250.0f;

        private int playerWidth = 64;
        private int playerHeight = 64;
        private int toolIndicate = -1;
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
        private Stack<string> historialAcciones = new Stack<string>();
        private string ultimaAccion = "Ninguna accion";
        private string playerName = string.Empty;

        private int playerId;
        private int sessionId;
        private List<string> mensajesError = new List<string>();
        private float tiempoError = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.TextInput += OnTextInput;
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (!menuOpen || controlsOpen || !_nameFieldActive || playerName.Length >= 16)
            {
                return;
            }

            char character = e.Character;
            bool isAsciiLetterOrDigit =
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');

            if (isAsciiLetterOrDigit || (character == ' ' && playerName.Length > 0 && !playerName.EndsWith(" ")))
            {
                playerName += character;
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

        private void CargarPartida(string nombreJugador)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return;
            }

            string nombreLimpio = nombreJugador.Trim();
            playerName = nombreLimpio;

            ConnectDB.playerData? jugadorGuardado = ConnectDB.CargarJugador(nombreLimpio);

            if (jugadorGuardado.HasValue)
            {
                playerPosition = new Vector2(jugadorGuardado.Value.axisX, jugadorGuardado.Value.axisY);

                ConnectDB.sessionData? sesionGuardada = ConnectDB.CargarSesion(jugadorGuardado.Value.id);

                if (sesionGuardada.HasValue)
                {
                    currentDay = sesionGuardada.Value.survived_days;
                    dayTimer = sesionGuardada.Value.time_day;
                }
                else
                {
                    currentDay = 1;
                    dayTimer = 0f;
                }

                System.Diagnostics.Debug.WriteLine("Partida cargada.");
            }
            else
            {
                playerPosition = new Vector2(710, 710);
                currentDay = 1;
                dayTimer = 0f;
            }
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

            oveja = new AnimalData
            {
                Nombre = "la oveja",
                Posicion = pos(12, 12),
                Estado = 1,
                Vida = 3,
                Esperando = true,
                Delay = 1f,
                Frame = 1,
                AnimSpeed = 0.12f,
                MoveDuration = 0.3f,
                HuyeDelJugador = true,
                FilasPorDireccion = new int[] { 2, 6, 0, 4 },
                OffsetVisual = new Vector2(12, 7),
                LayerDepth = 0.297f,
                ColumnasSprite = 3,
                FilasSprite = 8,
                DropItemId = 4
            };

            toro = new AnimalData
            {
                Nombre = "el toro",
                Posicion = pos(20, 20),
                Estado = 1,
                Vida = 5,
                Esperando = true,
                Delay = 1f,
                Frame = 1,
                AnimSpeed = 0.12f,
                MoveDuration = 0.3f,
                HuyeDelJugador = false,
                FilasPorDireccion = new int[] { 1, 3, 0, 2 },
                OffsetVisual = new Vector2(2, 2),
                LayerDepth = 0.296f,
                ColumnasSprite = 3,
                FilasSprite = 4,
                DropItemId = 4
            };

            menuOpen = true;
            isDead = false;

            _camera = new Camera2D(GraphicsDevice);
            _random = new Random();

            int cantSand = 15;
            int cantRock = 10;
            int cantTree = 30;

            rockPosition = new Vector2[cantRock];
            rockState = new int[cantRock];

            treePosition = new Vector2[cantTree];
            treeState = new int[cantTree];

            sandPosition = new Vector2[cantSand];
            sandState = new int[cantSand];

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
                rockState[i] = 1;
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
                treeState[i] = 1;
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
                sandState[i] = 1;
            }

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
            _rockDrop = Content.Load<Texture2D>("assets/images/RockDrop");
            _sand = Content.Load<Texture2D>("assets/images/sand");
            _sandDrop = Content.Load<Texture2D>("assets/images/sandDrop");
            _topTree = Content.Load<Texture2D>("assets/images/topTree");
            _bottomTree = Content.Load<Texture2D>("assets/images/bottomTree");
            _treeDrop = Content.Load<Texture2D>("assets/images/treeDrop");
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
            _pantmuerte = Content.Load<Texture2D>("assets/images/pantallamuerte");
            _registroacciones = Content.Load<Texture2D>("assets/images/registro acciones");
            _meatDrop = Content.Load<Texture2D>("assets/images/meatDrop");
            _error = Content.Load<Texture2D>("assets/images/error");

            oveja.SpriteSheet = _oveja;
            oveja.DropTexture = _meatDrop;
            toro.SpriteSheet = _toro;
            toro.DropTexture = _meatDrop;

            playerRectangle = new Rectangle[8, 3];
            int spriteWidth = 32;
            int spriteHeight = 32;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    playerRectangle[i, j] = new Rectangle(j * spriteWidth, i * spriteHeight, spriteWidth, spriteHeight);
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
                isDead = true;
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
            if (isNight)
            {
                playerHealth -= 5f * deltaTime;
            }
        }

        private void GuardarPartida()
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return;

            ConnectDB.playerData perfil = new ConnectDB.playerData
            {
                name = playerName.Trim(),
                axisX = (int)playerPosition.X,
                axisY = (int)playerPosition.Y
            };
            ConnectDB.Jugador(perfil);

            int idJugador = ConnectDB.ObtenerIdJugador(perfil.name);
            if (idJugador <= 0)
            {
                return;
            }

            ConnectDB.sessionData sesion = new ConnectDB.sessionData
            {
                id_player = idJugador,
                survived_days = currentDay,
                time_day = dayTimer
            };
            ConnectDB.Sesion(sesion);
        }

        private void ActualizarAtaqueAnimal(GameTime gameTime, ref AnimalData animal)
        {
            if (animal.Estado != 1)
                return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animal.HitCooldown > 0f)
                animal.HitCooldown -= delta;

            // Golpear con el hacha equipada (toolEquipped[1])
            if (toolEquipped[1] && animal.HitCooldown <= 0f)
            {
                bool ataco = (_curMS.LeftButton == ButtonState.Pressed && _oldMS.LeftButton == ButtonState.Released) ||
                             (_curKS.IsKeyDown(Keys.E) && _oldKS.IsKeyUp(Keys.E));

                if (ataco)
                {
                    Rectangle alcanceHacha = new Rectangle(
                        (int)playerPosition.X - 24,
                        (int)playerPosition.Y - 24,
                        playerWidth + 48,
                        playerHeight + 48
                    );

                    Rectangle hitboxAnimal = new Rectangle(
                        (int)animal.Posicion.X,
                        (int)animal.Posicion.Y,
                        64,
                        64
                    );

                    if (alcanceHacha.Intersects(hitboxAnimal))
                    {
                        animal.Vida--;
                        animal.HitCooldown = 0.35f;

                        if (animal.Vida <= 0)
                        {
                            animal.Estado = 2; // Deja el drop en el suelo
                            animal.Moviendo = false;
                            RegistrarAccion($"Mato a {animal.Nombre}");
                        }
                        else
                        {
                            RegistrarAccion($"Golpeo a {animal.Nombre}");
                        }
                    }
                }
            }
        }

        private void RecogerDropAnimal(Rectangle playerHitbox, ref AnimalData animal, ref int count)
        {
            if (animal.Estado == 2 && animal.DropTexture != null)
            {
                Rectangle dropHitbox = new Rectangle(
                    (int)animal.Posicion.X,
                    (int)animal.Posicion.Y,
                    animal.DropTexture.Width,
                    animal.DropTexture.Height
                );

                if (playerHitbox.Intersects(dropHitbox) && _curKS.IsKeyDown(Keys.Q) && _oldKS.IsKeyUp(Keys.Q))
                {
                    if (AgregarAlInventario(animal.DropItemId))
                    {
                        animal.Estado = 0;
                        count++;
                        RegistrarAccion("Recogio carne");
                    }
                }
            }
        }

        private void DibujarAnimal(AnimalData animal)
        {
            if (animal.Estado == 1 && animal.SpriteSheet != null)
            {
                int anchoFrame = animal.SpriteSheet.Width / animal.ColumnasSprite;
                int altoFrame = animal.SpriteSheet.Height / animal.FilasSprite;

                Rectangle sourceRect = new Rectangle(
                    animal.Frame * anchoFrame,
                    animal.DireccionSprite * altoFrame,
                    anchoFrame,
                    altoFrame
                );

                _spriteBatch.Draw(
                    animal.SpriteSheet,
                    animal.Posicion + animal.OffsetVisual,
                    sourceRect,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    animal.LayerDepth
                );
            }
            else if (animal.Estado == 2 && animal.DropTexture != null)
            {
                _spriteBatch.Draw(
                    animal.DropTexture,
                    animal.Posicion + animal.OffsetVisual,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.1f
                );
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _oldKS = _curKS;
            _curKS = Keyboard.GetState();
            _oldMS = _curMS;
            _curMS = Mouse.GetState();
            bool singleClick = _curMS.LeftButton == ButtonState.Pressed && _oldMS.LeftButton == ButtonState.Released;

            if (tiempoError > 0)
                tiempoError -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_curKS.IsKeyDown(Keys.Escape) && _oldKS.IsKeyUp(Keys.Escape))
            {
                if (menuOpen)
                {
                    Exit();
                }
                else
                {
                    GuardarPartida();
                    menuOpen = true;
                }
            }

            Rectangle buttonPlay = new Rectangle(37, 90, 288, 96);
            Rectangle buttonControls = new Rectangle(91, 192, 288, 96);
            Rectangle buttonExit = new Rectangle(145, 294, 288, 96);
            Rectangle buttonExitControls = new Rectangle(345, 348, 288, 96);

            if (menuOpen)
            {
                _camera.Follow(Vector2.Zero, GraphicsDevice.Viewport);
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
                    if (_nameFieldActive && _curKS.IsKeyDown(Keys.Back) && _oldKS.IsKeyUp(Keys.Back) && playerName.Length > 0)
                    {
                        playerName = playerName.Substring(0, playerName.Length - 1);
                    }
                    if (buttonPlay.Contains(_curMS.Position))
                    {
                        selectPlay = true;
                        if (singleClick && playerName != "")
                        {
                            CargarPartida(playerName);
                            menuOpen = false;
                        }
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
            else if (!isDead)
            {
                inventory();
                movementPlayer(gameTime);
                collision(gameTime);
                interact();
                collectDrops();
                colocarCasa();
                actualizarHerramienta();
                ActualizarIAAnimal(gameTime, ref oveja);
                ActualizarIAAnimal(gameTime, ref toro);
                playerStats(gameTime);
                timeDay(gameTime);

                ActualizarAtaqueAnimal(gameTime, ref oveja);
                ActualizarAtaqueAnimal(gameTime, ref toro);

                _camera.Follow(new Vector2(playerPosition.X + playerWidth / 2f, playerPosition.Y + playerHeight / 2f), GraphicsDevice.Viewport);
            }
            else if (isDead)
            {
                _camera.Follow(new Vector2(playerPosition.X + playerWidth / 2f, playerPosition.Y + playerHeight / 2f), GraphicsDevice.Viewport);
                if (_curKS.IsKeyDown(Keys.Enter) && _oldKS.IsKeyUp(Keys.Enter))
                {
                    playerHealth = 100f;
                    playerHunger = 100f;
                    playerEnergy = 100f;
                    dayTimer = 0f;
                    isNight = false;
                    isDead = false;
                    menuOpen = true;
                }
            }

            base.Update(gameTime);
        }

        private bool AgregarAlInventario(int itemID)
        {
            for (int i = 0; i < inventoryStorage.Length; i++)
            {
                if (inventoryStorage[i] == 0)
                {
                    inventoryStorage[i] = itemID;
                    return true;
                }
            }
            MostrarError("No tienes suficiente espacio");
            return false;
        }

        private void checkDropPickup(Rectangle playerHitbox, Vector2[] dropPos, Texture2D dropTex, int[] state, ref int count, int itemID)
        {
            for (int i = 0; i < dropPos.Length; i++)
            {
                if (state[i] == 2)
                {
                    Rectangle dropHitbox = new Rectangle((int)dropPos[i].X, (int)dropPos[i].Y, dropTex.Width, dropTex.Height);
                    if (playerHitbox.Intersects(dropHitbox) && _curKS.IsKeyDown(Keys.Q) && _oldKS.IsKeyUp(Keys.Q))
                    {
                        if (AgregarAlInventario(itemID))
                        {
                            state[i] = 0;
                            count++;
                            if (itemID == 1)
                                RegistrarAccion("Recogio madera");
                            else if (itemID == 2)
                                RegistrarAccion("Recogio piedra");
                            else if (itemID == 3)
                                RegistrarAccion("Recogio arena");
                            else if (itemID == 4)
                                RegistrarAccion("Recogio carne");
                        }
                    }
                }
            }
        }

        private void collectDrops()
        {
            Rectangle playerHitbox = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            checkDropPickup(playerHitbox, rockPosition, _rockDrop, rockState, ref rockCount, 2);
            checkDropPickup(playerHitbox, treePosition, _treeDrop, treeState, ref treeCount, 1);
            checkDropPickup(playerHitbox, sandPosition, _sandDrop, sandState, ref sandCount, 3);
            RecogerDropAnimal(playerHitbox, ref oveja, ref meatCount);
            RecogerDropAnimal(playerHitbox, ref toro, ref meatCount);
        }

        private void drawResource(Texture2D res, Texture2D resDrop, Vector2[] resPosition, int[] resState)
        {
            for (int i = 0; i < resPosition.Length; i++)
            {
                if (resState[i] == 1)
                {
                    _spriteBatch.Draw(res, resPosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                }
                else if (resState[i] == 2)
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
            drawResource(_rock, _rockDrop, rockPosition, rockState);
            drawResource(_sand, _sandDrop, sandPosition, sandState);

            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeState[i] == 1)
                {
                    _spriteBatch.Draw(_bottomTree, treePosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.3f);
                    _spriteBatch.Draw(_topTree, treePosition[i] - new Vector2(Epos(0), Epos(1)), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
                }
                else if (treeState[i] == 2)
                {
                    _spriteBatch.Draw(_treeDrop, treePosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                }
            }

            if (casaColocada)
            {
                _spriteBatch.Draw(_casa, casaPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.35f);
            }

            if (oveja.Estado == 1)
            {
                Rectangle areaVision = Rectangle.Empty;
                switch (oveja.UltimoMov)
                {
                    case 0:
                        areaVision = new Rectangle((int)oveja.Posicion.X, (int)oveja.Posicion.Y - Epos(1), Epos(3), Epos(3));
                        break;
                    case 1:
                        areaVision = new Rectangle((int)oveja.Posicion.X - Epos(2), (int)oveja.Posicion.Y - Epos(1), Epos(3), Epos(3));
                        break;
                    case 2:
                        areaVision = new Rectangle((int)oveja.Posicion.X - Epos(1), (int)oveja.Posicion.Y, Epos(3), Epos(3));
                        break;
                    case 3:
                        areaVision = new Rectangle((int)oveja.Posicion.X - Epos(1), (int)oveja.Posicion.Y - Epos(2), Epos(3), Epos(3));
                        break;
                }
                _spriteBatch.Draw(_pixel, areaVision, null, new Color(255, 220, 80, 45), 0f, Vector2.Zero, SpriteEffects.None, 0.25f);
            }

            if (toro.Estado == 1)
            {
                Rectangle areaVisionToro = Rectangle.Empty;
                switch (toro.UltimoMov)
                {
                    case 0:
                        areaVisionToro = new Rectangle((int)toro.Posicion.X, (int)toro.Posicion.Y - Epos(1), Epos(3), Epos(3));
                        break;
                    case 1:
                        areaVisionToro = new Rectangle((int)toro.Posicion.X - Epos(2), (int)toro.Posicion.Y - Epos(1), Epos(3), Epos(3));
                        break;
                    case 2:
                        areaVisionToro = new Rectangle((int)toro.Posicion.X - Epos(1), (int)toro.Posicion.Y, Epos(3), Epos(3));
                        break;
                    case 3:
                        areaVisionToro = new Rectangle((int)toro.Posicion.X - Epos(1), (int)toro.Posicion.Y - Epos(2), Epos(3), Epos(3));
                        break;
                }
                _spriteBatch.Draw(_pixel, areaVisionToro, null, new Color(255, 70, 70, 55), 0f, Vector2.Zero, SpriteEffects.None, 0.251f);
            }

            DibujarAnimal(oveja);
            DibujarAnimal(toro);

            _spriteBatch.Draw(_player, playerPosition, playerRectangle[playerDirection, currentAnimationIndex], Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.298f);
            if (toolEquipped[0])
            {
                _spriteBatch.Draw(_woodPicaxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.299f);
            }
            else if (toolEquipped[1])
            {
                _spriteBatch.Draw(_woodAxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0.299f);
            }
            else if (toolEquipped[2])
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

            if (_openInventory && !isDead)
            {
                _spriteBatch.Draw(_inventory, new Vector2((GraphicsDevice.Viewport.Width / 2) - _inventory.Width / 2 * 7, (GraphicsDevice.Viewport.Height / 2) - _inventory.Height / 2 * 7), null, Color.White, 0.0f, Vector2.Zero, 7.0f, SpriteEffects.None, 0.8f);
                for (int i = 0; i < inventoryStorage.Length; i++)
                {
                    if (inventoryStorage[i] == 1)
                    {
                        _spriteBatch.Draw(_treeDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                    if (inventoryStorage[i] == 2)
                    {
                        _spriteBatch.Draw(_rockDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                    if (inventoryStorage[i] == 3)
                    {
                        _spriteBatch.Draw(_sandDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                    if (inventoryStorage[i] == 4)
                    {
                        _spriteBatch.Draw(_meatDrop, inventorySlot[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                    }
                }
            }

            _spriteBatch.DrawString(_font, "Ultima accion: " + ultimaAccion, new Vector2(20, 50), Color.White);
            if (_curKS.IsKeyDown(Keys.H))
            {
                int espacioR = 33;
                _spriteBatch.Draw(_registroacciones, new Vector2(620, 60), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.98f);
                foreach (string accion in historialAcciones)
                {
                    espacioR += 32;
                    if (espacioR + 32 > 350)
                        break;
                    _spriteBatch.DrawString(_font, accion, new Vector2(630, espacioR), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
            }

            if (tiempoError > 0)
            {
                _spriteBatch.Draw(_error, new Vector2(20, 300), null, Color.White, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, 0.98f);
                foreach (string mensaje in mensajesError)
                {
                    _spriteBatch.DrawString(_font, mensaje, new Vector2(80, 325), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
                }
            }

            if (menuOpen)
            {
                _spriteBatch.Draw(_menu, new Vector2(0, 0), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.98f);
                if (!controlsOpen)
                {
                    Color nameFieldBorder = _nameFieldActive ? Color.Cyan : Color.LightGray;
                    _spriteBatch.DrawString(_font, "Nombre de usuario", new Vector2(37, 392), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.995f);
                    _spriteBatch.Draw(_pixel, _nameInputBounds, null, nameFieldBorder, 0f, Vector2.Zero, SpriteEffects.None, 0.996f);
                    _spriteBatch.Draw(_pixel, new Rectangle(_nameInputBounds.X + 2, _nameInputBounds.Y + 2, _nameInputBounds.Width - 4, _nameInputBounds.Height - 4), null, new Color(25, 28, 52), 0f, Vector2.Zero, SpriteEffects.None, 0.997f);
                    string nameText = playerName.Length == 0 ? "Haz clic para escribir..." : playerName;
                    _spriteBatch.DrawString(_font, nameText, new Vector2(_nameInputBounds.X + 10, _nameInputBounds.Y + 13), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.998f);
                }
                if (selectPlay)
                {
                    _spriteBatch.Draw(_select_play, new Vector2(37, 90), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (selectControls)
                {
                    _spriteBatch.Draw(_select_controls, new Vector2(91, 192), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (selectExit)
                {
                    _spriteBatch.Draw(_select_exit, new Vector2(145, 294), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                }
                if (controlsOpen)
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

            if (isDead)
            {
                _spriteBatch.Draw(_pantmuerte, fullScreen, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }
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
            {
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

        private void CheckCollisions(ref Vector2 playerPosition, ref Rectangle playerHitbox, Vector2[] objPosition, int[] objState, Texture2D obj, float movement, bool movementX)
        {
            for (int i = 0; i < objPosition.Length; i++)
            {
                if (objState[i] != 1) continue;
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
            playerPosition.X += movement.X * velocidadGlobal * delta;
            if (playerPosition.X < 497) playerPosition.X = 497;
            if (playerPosition.X > 2279) playerPosition.X = 2279;
            Rectangle playerHitboxX = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            CheckCollisions(ref playerPosition, ref playerHitboxX, rockPosition, rockState, _rock, movement.X, true);
            CheckCollisions(ref playerPosition, ref playerHitboxX, treePosition, treeState, _bottomTree, movement.X, true);
            CheckCollisions(ref playerPosition, ref playerHitboxX, sandPosition, sandState, _sand, movement.X, true);
            playerPosition.Y += movement.Y * velocidadGlobal * delta;
            if (playerPosition.Y < 355) playerPosition.Y = 355;
            if (playerPosition.Y > 2137) playerPosition.Y = 2137;
            Rectangle playerHitboxY = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            CheckCollisions(ref playerPosition, ref playerHitboxY, rockPosition, rockState, _rock, movement.Y, false);
            CheckCollisions(ref playerPosition, ref playerHitboxY, treePosition, treeState, _bottomTree, movement.Y, false);
            CheckCollisions(ref playerPosition, ref playerHitboxY, sandPosition, sandState, _sand, movement.Y, false);
        }

        private void inventory()
        {
            if (_curKS.IsKeyDown(Keys.Tab) && _oldKS.IsKeyUp(Keys.Tab))
            {
                _openInventory = !_openInventory;
            }
        }

        private bool PuedeMoverAnimal(Vector2 destino)
        {
            Rectangle animalHitbox = new Rectangle((int)destino.X, (int)destino.Y, 64, 64);
            if (destino.X < 497 || destino.X > 2279 || destino.Y < 355 || destino.Y > 2137)
                return false;

            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (rockState[i] == 1)
                {
                    Rectangle obstaculo = new Rectangle((int)rockPosition[i].X, (int)rockPosition[i].Y, _rock.Width, _rock.Height);
                    if (animalHitbox.Intersects(obstaculo)) return false;
                }
            }

            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeState[i] == 1)
                {
                    Rectangle obstaculo = new Rectangle((int)treePosition[i].X, (int)treePosition[i].Y, _bottomTree.Width, _bottomTree.Height);
                    if (animalHitbox.Intersects(obstaculo)) return false;
                }
            }

            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (sandState[i] == 1)
                {
                    Rectangle obstaculo = new Rectangle((int)sandPosition[i].X, (int)sandPosition[i].Y, _sand.Width, _sand.Height);
                    if (animalHitbox.Intersects(obstaculo)) return false;
                }
            }

            return true;
        }

        private Vector2 ObtenerVectorDireccion(int dir)
        {
            return dir switch
            {
                0 => new Vector2(1, 0),
                1 => new Vector2(-1, 0),
                2 => new Vector2(0, 1),
                3 => new Vector2(0, -1),
                _ => Vector2.Zero
            };
        }

        private void ActualizarIAAnimal(GameTime gameTime, ref AnimalData animal)
        {
            if (animal.Estado != 1)
                return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (animal.Moviendo)
            {
                animal.MoveTimer += delta;
                animal.AnimTimer += delta;
                float progreso = MathHelper.Clamp(animal.MoveTimer / animal.MoveDuration, 0f, 1f);
                animal.Posicion = Vector2.Lerp(animal.Inicio, animal.Destino, progreso);

                if (animal.AnimTimer >= animal.AnimSpeed)
                {
                    animal.Frame = (animal.Frame + 1) % animal.ColumnasSprite;
                    animal.AnimTimer = 0f;
                }

                if (progreso >= 1f)
                {
                    animal.Posicion = animal.Destino;
                    animal.Moviendo = false;
                    animal.Frame = 1;
                    animal.Timer = 0f;
                    animal.Esperando = true;
                    animal.Delay = animal.MovimientosObjetivo > 0 ? 0.2f : 1f;
                }
                return;
            }

            if (animal.Esperando)
            {
                animal.Timer += delta;
                if (animal.Timer < animal.Delay)
                    return;

                animal.Esperando = false;
                animal.Timer = 0f;

                Point jugador = new Point((int)(playerPosition.X + playerWidth / 2), (int)(playerPosition.Y + playerHeight / 2));
                Rectangle visionDerecha = new Rectangle((int)animal.Posicion.X, (int)animal.Posicion.Y - Epos(1), Epos(3), Epos(3));
                Rectangle visionIzquierda = new Rectangle((int)animal.Posicion.X - Epos(2), (int)animal.Posicion.Y - Epos(1), Epos(3), Epos(3));
                Rectangle visionArriba = new Rectangle((int)animal.Posicion.X - Epos(1), (int)animal.Posicion.Y - Epos(2), Epos(3), Epos(3));
                Rectangle visionAbajo = new Rectangle((int)animal.Posicion.X - Epos(1), (int)animal.Posicion.Y, Epos(3), Epos(3));

                bool vioDerecha = visionDerecha.Contains(jugador) && animal.UltimoMov == 0;
                bool vioIzquierda = visionIzquierda.Contains(jugador) && animal.UltimoMov == 1;
                bool vioAbajo = visionAbajo.Contains(jugador) && animal.UltimoMov == 2;
                bool vioArriba = visionArriba.Contains(jugador) && animal.UltimoMov == 3;

                if (vioDerecha)
                {
                    animal.DireccionObjetivo = animal.HuyeDelJugador ? 1 : 0;
                    animal.MovimientosObjetivo = 11;
                }
                else if (vioIzquierda)
                {
                    animal.DireccionObjetivo = animal.HuyeDelJugador ? 0 : 1;
                    animal.MovimientosObjetivo = 11;
                }
                else if (vioArriba)
                {
                    animal.DireccionObjetivo = animal.HuyeDelJugador ? 2 : 3;
                    animal.MovimientosObjetivo = 11;
                }
                else if (vioAbajo)
                {
                    animal.DireccionObjetivo = animal.HuyeDelJugador ? 3 : 2;
                    animal.MovimientosObjetivo = 11;
                }

                bool persiguiendoOHuyendo = animal.MovimientosObjetivo > 0;
                int mov = (persiguiendoOHuyendo && _random.Next(0, 100) < 70)
                    ? animal.DireccionObjetivo
                    : _random.Next(0, 4);

                animal.MoveDuration = persiguiendoOHuyendo ? 0.01f : 0.3f;
                if (animal.MovimientosObjetivo > 0)
                    animal.MovimientosObjetivo--;

                Vector2 movimiento = ObtenerVectorDireccion(mov);
                animal.UltimoMov = mov;
                animal.DireccionSprite = animal.FilasPorDireccion[mov];

                Vector2 destinoPropuesto = animal.Posicion + movimiento * Epos(1);

                if (!PuedeMoverAnimal(destinoPropuesto))
                {
                    bool encontroCamino = false;
                    int[] direccionesAlternativas = { 0, 1, 2, 3 };
                    direccionesAlternativas = direccionesAlternativas.OrderBy(x => _random.Next()).ToArray();

                    foreach (int direccion in direccionesAlternativas)
                    {
                        Vector2 destinoAlternativo = animal.Posicion + ObtenerVectorDireccion(direccion) * Epos(1);
                        if (PuedeMoverAnimal(destinoAlternativo))
                        {
                            destinoPropuesto = destinoAlternativo;
                            animal.UltimoMov = direccion;
                            animal.DireccionSprite = animal.FilasPorDireccion[direccion];
                            encontroCamino = true;
                            break;
                        }
                    }

                    if (!encontroCamino)
                    {
                        animal.Frame = 1;
                        animal.Timer = 0f;
                        animal.Esperando = true;
                        animal.Delay = 0.5f;
                        return;
                    }
                }

                animal.Inicio = animal.Posicion;
                animal.Destino = destinoPropuesto;
                animal.MoveTimer = 0f;
                animal.AnimTimer = 0f;
                animal.Frame = 0;
                animal.Moviendo = true;
            }
        }

        private int checkResource(ref Vector2 playerCenter, Vector2[] objPosition, Texture2D obj, int[] objState, bool herramientaCorrecta)
        {
            for (int i = 0; i < objPosition.Length; i++)
            {
                if (objState[i] == 1)
                {
                    Vector2 objCenter = new Vector2(objPosition[i].X + (obj.Width / 2), objPosition[i].Y + (obj.Height / 2));
                    float distance = Vector2.Distance(playerCenter, objCenter);
                    if (distance < 80.0f)
                    {
                        if (herramientaCorrecta)
                        {
                            objState[i] = 2;
                        }
                        return 1;
                    }
                }
            }
            return 0;
        }

        private void colocarCasa()
        {
            if (_curKS.IsKeyDown(Keys.C) && _oldKS.IsKeyUp(Keys.C))
            {
                switch (playerDirection)
                {
                    case 0:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 1:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                    case 2:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f;
                        casaColocada = true;
                        break;
                    case 3:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f;
                        casaColocada = true;
                        break;
                    case 4:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 5:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f + 71;
                        casaColocada = true;
                        break;
                    case 6:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f - 142;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                    case 7:
                        casaPosition.X = (float)Math.Floor((playerPosition.X + 32) / 71f) * 71f + 71;
                        casaPosition.Y = (float)Math.Floor((playerPosition.Y + 32) / 71f) * 71f - 142;
                        casaColocada = true;
                        break;
                }
                RegistrarAccion("Construyo casa");
            }
        }

        private void interact()
        {
            Vector2 playerCenter = new Vector2(playerPosition.X + (playerWidth / 2f), playerPosition.Y + (playerHeight / 2f));
            if (_curKS.IsKeyDown(Keys.R) && toolIndicate != 0) { toolIndicate = 0; RegistrarAccion("Equipo pico"); }
            if (_curKS.IsKeyDown(Keys.T) && toolIndicate != 1) { toolIndicate = 1; RegistrarAccion("Equipo hacha"); }
            if (_curKS.IsKeyDown(Keys.Y) && toolIndicate != 2) { toolIndicate = 2; RegistrarAccion("Equipo pala"); }
            if (_curKS.IsKeyDown(Keys.U) && toolIndicate != -1) { toolIndicate = -1; RegistrarAccion("Desequipo"); }
            for (int i = 0; i < toolEquipped.Length; i++)
            {
                toolEquipped[i] = (i == toolIndicate);
            }
            if (_curKS.IsKeyDown(Keys.E) && _oldKS.IsKeyUp(Keys.E))
            {
                if (toolEquipped[0]) // Pico
                {
                    if (checkResource(ref playerCenter, rockPosition, _rock, rockState, true) == 1)
                    {
                        RegistrarAccion("Pico piedra");
                    }
                    else if (checkResource(ref playerCenter, treePosition, _bottomTree, treeState, false) == 1 ||
                    checkResource(ref playerCenter, sandPosition, _sand, sandState, false) == 1)
                    {
                        MostrarError("Herramienta incorrecta");
                    }
                }
                else if (toolEquipped[1]) // Hacha
                {
                    if (checkResource(ref playerCenter, treePosition, _bottomTree, treeState, true) == 1)
                    {
                        RegistrarAccion("Talo arbol");
                    }
                    else if (checkResource(ref playerCenter, rockPosition, _rock, rockState, false) == 1 ||
                    checkResource(ref playerCenter, sandPosition, _sand, sandState, false) == 1)
                    {
                        MostrarError("Herramienta incorrecta");
                    }
                }
                else if (toolEquipped[2]) // Pala
                {
                    if (checkResource(ref playerCenter, sandPosition, _sand, sandState, true) == 1)
                    {
                        RegistrarAccion("Paleo arena");
                    }
                    else if (checkResource(ref playerCenter, rockPosition, _rock, rockState, false) == 1 ||
                    checkResource(ref playerCenter, treePosition, _bottomTree, treeState, false) == 1)
                    {
                        MostrarError("Herramienta incorrecta");
                    }
                }
            }
        }

        private void MostrarError(string mensaje)
        {
            mensajesError.Add(mensaje);
            tiempoError = 0.7f;
            if (mensajesError.Count > 1)
            {
                mensajesError.RemoveAt(0);
            }
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

        private void RegistrarAccion(string accion)
        {
            historialAcciones.Push(accion);
            ultimaAccion = accion;
        }

        private void actualizarHerramienta()
        {
            switch (playerDirection)
            {
                case 0: toolPosition = playerPosition + new Vector2(5, 5); break;
                case 1: toolPosition = playerPosition + new Vector2(47, 0); break;
                case 2: toolPosition = playerPosition + new Vector2(20, 3); break;
                case 3: toolPosition = playerPosition + new Vector2(20, 27); break;
                case 4: toolPosition = playerPosition + new Vector2(3, 5); break;
                case 5: toolPosition = playerPosition + new Vector2(3, 20); break;
                case 6: toolPosition = playerPosition + new Vector2(38, 2); break;
                case 7: toolPosition = playerPosition + new Vector2(43, 12); break;
            }
        }
    }
}