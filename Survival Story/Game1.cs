using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Entradas;

namespace Survival_Story
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _player;
        private Texture2D _rock;
        private Texture2D _tree;
        private Texture2D _sand;
        private Texture2D _map;
        private Texture2D _woodPicaxe;
        private Texture2D _woodAxe;
        private Texture2D _woodShovel;
        private Texture2D _inventory;

        private Vector2 playerPosition;
        private Vector2[] rockPosition;
        private Vector2[] treePosition;
        private Vector2[] sandPosition;
        private Vector2 toolPosition;
        private Vector2 mapPosition;

        private bool[] rockActive;
        private bool[] treeActive;
        private bool[] sandActive;
        private bool axeEquipped;
        private bool shovelEquipped;
        private bool picaxeEquipped; 

        private float velocidadGlobal = 250.0f;

        private float playerTimer;
        private int threshold;

        private Rectangle[,] playerRectangle;

        private byte previousAnimationIndex;
        private byte currentAnimationIndex;
        private int playerDirection;

        private bool _openInventory = false;
        private KeyboardState _oldKS;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        private Vector2 pos(int posX, int posY)
        {
            int grPosX = 71;
            int grPosY = 73;
            return new Vector2(grPosX * posX, grPosY * posY);
        }

        protected override void Initialize()
        {
            playerPosition = new Vector2(100, 100);
            // poner grPosX * posHorizontal y grPosY * posVertical (0-inf??)

            rockPosition = new Vector2[3];
            rockPosition[0] = pos(1,1);
            rockPosition[1] = pos(3,3);
            rockPosition[2] = pos(5,5);

            rockActive = new bool[3];
            rockActive[0] = true;
            rockActive[1] = true;
            rockActive[2] = true;

            treePosition = new Vector2[3];
            treePosition[0] = pos(1,3);
            treePosition[1] = pos(3,5);
            treePosition[2] = pos(5,1);

            treeActive = new bool[3];
            treeActive[0] = true;
            treeActive[1] = true;
            treeActive[2] = true;

            sandPosition = new Vector2[3];
            sandPosition[0] = pos(1,5);
            sandPosition[1] = pos(3,1);
            sandPosition[2] = pos(5,3);

            sandActive = new bool[3];
            sandActive[0] = true;
            sandActive[1] = true;
            sandActive[2] = true;

            picaxeEquipped = true;
            axeEquipped = false;
            shovelEquipped = false;

            mapPosition = new Vector2(0, 0);
            playerTimer = 0f;
            threshold = 250;
            playerDirection = 0;
            previousAnimationIndex = 2;
            currentAnimationIndex = 1;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _player = Content.Load<Texture2D>("assets/images/playerSheet");
            _rock = Content.Load<Texture2D>("assets/images/rock");
            _sand = Content.Load<Texture2D>("assets/images/sand");
            _tree = Content.Load<Texture2D>("assets/images/tree");
            _map = Content.Load<Texture2D>("assets/images/map");
            _inventory = Content.Load<Texture2D>("assets/images/inventory");
            _woodPicaxe = Content.Load<Texture2D>("assets/images/woodPicaxe");
            _woodAxe = Content.Load<Texture2D>("assets/images/woodAxe");
            _woodShovel = Content.Load<Texture2D>("assets/images/woodShovel"); 

            playerRectangle = new Rectangle[8, 3];
            int playerWidth = 35;
            int playerHeight = 36;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            collision(gameTime);
            inventory();
            interact();

            _oldKS = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            
            _spriteBatch.Draw(
                _map,
                mapPosition,
                null,
                Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f
            );

            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (rockActive[i])
                {
                    _spriteBatch.Draw(
                        _rock, rockPosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f
                    );
                }
            }

            for (int i = 0; i < treePosition.Length; i++)
            {
                if (treeActive[i])
                {
                    _spriteBatch.Draw(
                        _tree, treePosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f
                    );
                }
            }

            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (sandActive[i])
                {
                    _spriteBatch.Draw(
                        _sand, sandPosition[i], null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f
                    );
                }
            }

            _spriteBatch.Draw(
                _player, playerPosition, playerRectangle[playerDirection, currentAnimationIndex], Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f
            );

            if (picaxeEquipped == true)
            {
                _spriteBatch.Draw(_woodPicaxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
            }
            else if (axeEquipped == true)
            {
                _spriteBatch.Draw(_woodAxe, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
            }
            else if (shovelEquipped == true)
            {
                _spriteBatch.Draw(_woodShovel, toolPosition, null, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
            }

            if (_openInventory)
            {
                _spriteBatch.Draw(_inventory, new Vector2(100, 110), null, Color.White,0.0f, Vector2.Zero, 7.0f, SpriteEffects.None, 0.0f
                );
            }
                _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void registrarJugador()
        {
            ConnectDB.RegistrarJugador("Javier", "goat", "algo.gmail.com", 1167676767);
        }
        private void collision(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState ks = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            if (ks.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (ks.IsKeyDown(Keys.S)) movement.Y += 1;
            if (ks.IsKeyDown(Keys.A)) movement.X -= 1;
            if (ks.IsKeyDown(Keys.D)) movement.X += 1;

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
            toolPosition.X = playerPosition.X + 50;
            toolPosition.Y = playerPosition.Y + 30;
            int playerWidth = 35 * 2;
            int playerHeight = 36 * 2;
            Rectangle windowEdge = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            playerPosition.X += movement.X * velocidadGlobal * delta;
            if (playerPosition.X < windowEdge.Left) playerPosition.X = windowEdge.Left;
            else if (playerPosition.X + playerWidth > windowEdge.Right) playerPosition.X = windowEdge.Right - playerWidth;

            Rectangle playerHitboxX = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);

            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (!rockActive[i]) continue;

                Rectangle rockHitbox = new Rectangle((int)rockPosition[i].X, (int)rockPosition[i].Y, _rock.Width, _rock.Height);
                if (playerHitboxX.Intersects(rockHitbox))
                {
                    if (movement.X > 0) playerPosition.X = rockHitbox.Left - playerWidth;
                    else if (movement.X < 0) playerPosition.X = rockHitbox.Right;
                }
            }

            for (int i = 0; i < treePosition.Length; i++)
            {
                if (!treeActive[i]) continue;

                Rectangle treeHitbox = new Rectangle((int)treePosition[i].X, (int)treePosition[i].Y, _tree.Width, _tree.Height);
                if (playerHitboxX.Intersects(treeHitbox))
                {
                    if (movement.X > 0) playerPosition.X = treeHitbox.Left - playerWidth;
                    else if (movement.X < 0) playerPosition.X = treeHitbox.Right;
                }
            }
            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (!sandActive[i]) continue;
                Rectangle sandHitbox = new Rectangle((int)sandPosition[i].X, (int)sandPosition[i].Y, _sand.Width, _sand.Height);
                if (playerHitboxX.Intersects(sandHitbox))
                {
                    if (movement.X > 0) playerPosition.X = sandHitbox.Left - playerWidth;
                    else if (movement.X < 0) playerPosition.X = sandHitbox.Right;
                }
            }
            playerPosition.Y += movement.Y * velocidadGlobal * delta;
            if (playerPosition.Y < windowEdge.Top) playerPosition.Y = windowEdge.Top;
            else if (playerPosition.Y + playerHeight > windowEdge.Bottom) playerPosition.Y = windowEdge.Bottom - playerHeight;
            Rectangle playerHitboxY = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);

            for (int i = 0; i < rockPosition.Length; i++)
            {
                if (!rockActive[i]) continue;
                Rectangle rockHitbox = new Rectangle((int)rockPosition[i].X, (int)rockPosition[i].Y, _rock.Width, _rock.Height);
                if (playerHitboxY.Intersects(rockHitbox))
                {
                    if (movement.Y > 0) playerPosition.Y = rockHitbox.Top - playerHeight;
                    else if (movement.Y < 0) playerPosition.Y = rockHitbox.Bottom;
                }
            }
            for (int i = 0; i < treePosition.Length; i++)
            {
                if (!treeActive[i]) continue;
                Rectangle treeHitbox = new Rectangle((int)treePosition[i].X, (int)treePosition[i].Y, _tree.Width, _tree.Height);
                if (playerHitboxY.Intersects(treeHitbox))
                {
                    if (movement.Y > 0) playerPosition.Y = treeHitbox.Top - playerHeight;
                    else if (movement.Y < 0) playerPosition.Y = treeHitbox.Bottom;
                }
            }
            for (int i = 0; i < sandPosition.Length; i++)
            {
                if (!sandActive[i]) continue;
                Rectangle sandHitbox = new Rectangle((int)sandPosition[i].X, (int)sandPosition[i].Y, _sand.Width, _sand.Height);
                if (playerHitboxY.Intersects(sandHitbox))
                {
                    if (movement.Y > 0) playerPosition.Y = sandHitbox.Top - playerHeight;
                    else if (movement.Y < 0) playerPosition.Y = sandHitbox.Bottom;
                }
            }
        }
        private void inventory()
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Tab) && _oldKS.IsKeyUp(Keys.Tab))
            {
                _openInventory = !_openInventory;
            }
        }
        private void interact()
        {
            Vector2 playerCenter = new Vector2(playerPosition.X + 35, playerPosition.Y + 36);
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.R) && _oldKS.IsKeyUp(Keys.R)) { picaxeEquipped = true; axeEquipped = false; shovelEquipped = false; }
            if (ks.IsKeyDown(Keys.T) && _oldKS.IsKeyUp(Keys.T)) { picaxeEquipped = false; axeEquipped = true; shovelEquipped = false; }
            if (ks.IsKeyDown(Keys.Y) && _oldKS.IsKeyUp(Keys.Y)) { picaxeEquipped = false; axeEquipped = false; shovelEquipped = true; }
            if (ks.IsKeyDown(Keys.U) && _oldKS.IsKeyUp(Keys.U)) { picaxeEquipped = false; axeEquipped = false; shovelEquipped = false; }
            
            if (ks.IsKeyDown(Keys.G) && _oldKS.IsKeyUp(Keys.G))
            {
                registrarJugador();
            }

            if (ks.IsKeyDown(Keys.E) && picaxeEquipped == true)
            {
                for (int i = 0; i < rockPosition.Length; i++)
                {
                    if (rockActive[i])
                    {
                        Vector2 rockCenter = new Vector2(rockPosition[i].X + (_rock.Width / 2), rockPosition[i].Y + (_rock.Height / 2));
                        float distance = Vector2.Distance(playerCenter, rockCenter);
                        if (distance < 80.0f)
                        {
                            rockActive[i] = false;
                            break;
                        }
                    }
                }
            }
            if (ks.IsKeyDown(Keys.E) && axeEquipped == true)
            {
                for (int i = 0; i < treePosition.Length; i++)
                {
                    if (treeActive[i])
                    {
                        Vector2 treeCenter = new Vector2(treePosition[i].X + (_tree.Width / 2), treePosition[i].Y + (_tree.Height / 2));
                        float distance = Vector2.Distance(playerCenter, treeCenter);
                        if (distance < 90.0f)
                        {
                            treeActive[i] = false;
                            break;
                        }
                    }
                }
            }
            if (ks.IsKeyDown(Keys.E) && shovelEquipped == true)
            {
                for (int i = 0; i < sandPosition.Length; i++)
                {
                    if (sandActive[i])
                    {
                        Vector2 sandCenter = new Vector2(sandPosition[i].X + (_sand.Width / 2), sandPosition[i].Y + (_sand.Height / 2));
                        float distance = Vector2.Distance(playerCenter, sandCenter);
                        if (distance < 80.0f)
                        {
                            sandActive[i] = false;
                            break;
                        }
                    }
                }
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
    }
}