using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Entradas;
using System;
using System.Linq;

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

        private Texture2D _player, _rock, _topTree, _bottomTree, _sand, _map, _woodPicaxe, _woodAxe, _woodShovel, _inventory, _menu, _select_play, _select_exit, _select_controls, _controls, _rockDrop, _sandDrop;
        private Camera2D _camera;
        private Random _random;

        private Vector2 movement, playerPosition, toolPosition, mapPosition;
        private Vector2[] rockPosition, treePosition, sandPosition;

        private bool[] rockActive, treeActive, sandActive;//entidades
        private bool menuOpen, controlsOpen, selectPlay, selectExit, selectExitControls, selectControls ;//menu
        private bool onGrass, onMountain;
        private bool[] toolEquipped = new bool[3];//herramienta

        private float velocidadGlobal = 250.0f;

        private int playerWidth = 64;
        private int playerHeight =  64;
        private int toolIndicate;

        private float playerTimer;
        private int threshold;

        private Rectangle[,] playerRectangle;

        private byte previousAnimationIndex;
        private byte currentAnimationIndex;
        private int playerDirection;
        private int idPlayer;

        private bool _openInventory = false;
        private MouseState _curMS, _oldMS;
        private KeyboardState _curKS, _oldKS;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

            playerPosition = pos(10, 10);
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
        }

        protected override void Update(GameTime gameTime)
        {
            _oldKS = _curKS;
            _curKS = Keyboard.GetState();
            _oldMS = _curMS;
            _curMS = Mouse.GetState();

            bool singleClick = _curMS.LeftButton == ButtonState.Pressed && _oldMS.LeftButton == ButtonState.Released;

            _camera.Follow(new Vector2(500, 300), GraphicsDevice.Viewport);
            if (_curKS.IsKeyDown(Keys.Escape) && _oldKS.IsKeyUp(Keys.Escape))
            {
                if (menuOpen == true)
                {
                    Exit();
                }
                else
                {
                    menuOpen = true;
                }
            }

            Rectangle buttonPlay = new Rectangle(37, 90, 288, 96);
            Rectangle buttonControls = new Rectangle(91, 192, 288, 96);
            Rectangle buttonExit = new Rectangle(145, 294, 288, 96);
            Rectangle buttonExitControls = new Rectangle(345, 348, 288, 96);

            if(menuOpen)
            {
                if(!controlsOpen)
                {
                    if(buttonPlay.Contains(_curMS.Position))
                    {
                        selectPlay = true;
                        if(singleClick) menuOpen = false;
                    }
                    else
                    {
                        selectPlay = false;
                    }
                    if(buttonControls.Contains(_curMS.Position))
                    {
                        selectControls = true;
                        if(singleClick)controlsOpen = true;
                    }
                    else
                    {
                        selectControls = false;
                    }
                    if(buttonExit.Contains(_curMS.Position))
                    {
                        selectExit = true;
                        if(singleClick) Exit();
                    }
                    else
                    {
                        selectExit = false;
                    }
                }
                else if(controlsOpen)
                {
                    if(buttonExitControls.Contains(_curMS.Position))
                    {
                        selectExitControls = true;
                        if(singleClick)controlsOpen = false;
                    }
                    else
                    {
                        selectExitControls = false;
                    }
                }
            }
            else if(!menuOpen)
            {
                inventory();
                movementPlayer(gameTime);
                interact();
                collision(gameTime);
                actualizarHerramienta();
                _camera.Follow(playerPosition, GraphicsDevice.Viewport);
            }
            base.Update(gameTime);
        }

        private void drawResource(Texture2D res, Texture2D resDrop, Vector2[] resPosition, bool[] resActive)
        {
            for(int i = 0; i < resPosition.Length; i++)
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
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: _camera.Transform);

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

            if (_openInventory == true)
            {
                _spriteBatch.Draw(_inventory, new Vector2(100, 110), null, Color.White, 0.0f, Vector2.Zero, 7.0f, SpriteEffects.None, 0.8f);
            }

            if (menuOpen == true)
            {
                _spriteBatch.Draw(_menu, new Vector2(100, 60), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.9f);
    
                if (selectPlay == true)
                {
                    _spriteBatch.Draw(_select_play, new Vector2(137, 150), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
                }
                if (selectControls == true)
                {
                    _spriteBatch.Draw(_select_controls, new Vector2(191, 252), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
                }
                if (selectExit == true)
                {
                    _spriteBatch.Draw(_select_exit, new Vector2(245, 354), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
                }  
                if (controlsOpen == true)
                {
                    _spriteBatch.Draw(_controls, new Vector2(100, 60), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.96f);
                    if (selectExitControls)
                    {
                        _spriteBatch.Draw(_select_exit, new Vector2(445, 408), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
                    }
                }
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
            /*else
            {
                playerTimer = 0f;
            }*/
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

        private void checkResource(ref Vector2 playerCenter, Vector2[] objPosition,Texture2D obj, bool[] objActive)
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

        private void interact()
        { //35x36
            Vector2 playerCenter = new Vector2(playerPosition.X + (playerWidth / 2f), playerPosition.Y + (playerHeight /2f));

            if (_curKS.IsKeyDown(Keys.R)) toolIndicate = 0;
            if(_curKS.IsKeyDown(Keys.T)) toolIndicate = 1;
            if(_curKS.IsKeyDown(Keys.Y)) toolIndicate = 2;
            if(_curKS.IsKeyDown(Keys.U)) toolIndicate = -1;

            for (int i = 0; i < toolEquipped.Length; i++)
            {
                toolEquipped[i] = (i == toolIndicate) ? true : false;
            }
            if(_curKS.IsKeyDown(Keys.E))
            {
                for(int i = 0; i < toolEquipped.Length; i++)
                {
                    if(toolEquipped[0])
                    {
                        checkResource(ref playerCenter, rockPosition, _rock, rockActive);
                    }
                    if(toolEquipped[1])
                    {
                        checkResource(ref playerCenter, treePosition, _bottomTree, treeActive);
                    }
                    if(toolEquipped[2])
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
                currentAnimationIndex = (byte)((currentAnimationIndex + 1) % 3);
                playerTimer -= threshold;
            }
        }

        /*private void animation(GameTime gameTime)
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
        }*/


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