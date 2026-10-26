using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survival_Story
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _player;
        private Texture2D _rock;
        private Texture2D _map;
        private Texture2D _inventory;

        private Vector2 playerPosition;
        private Vector2 rockPosition;
        private Vector2 mapPosition;

        private float velocidadGlobal = 250.0f; // píxeles por segundo

        private float playerTimer;
        private int threshold;

        private Rectangle[,] playerRectangle;

        private byte previousAnimationIndex;
        private byte currentAnimationIndex;
        private int playerDirection;

        private bool _openInventory = false; // Guarda si el inventario se muestra o no
        private KeyboardState _oldKS; // Guarda el estado del teclado del frame anterior (evita parpadeo al pulsar 'I')


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            playerPosition = new Vector2(100, 100);
            rockPosition = new Vector2(280, 216);
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
            _map = Content.Load<Texture2D>("assets/images/map");
            _inventory = Content.Load<Texture2D>("assets/images/inventory");

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
            _spriteBatch.Draw( //rock = 70x72
                _rock,
                rockPosition,
                null,
                Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f
            );

            _spriteBatch.Draw(
                _player,
                playerPosition,
                playerRectangle[playerDirection, currentAnimationIndex],
                Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f
            );

            if (_openInventory)
            {
                _spriteBatch.Draw(
                    _inventory,
                    new Vector2(100, 110),
                    null,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    7.0f,
                    SpriteEffects.None,
                    0.0f
                );
            }

                _spriteBatch.End();
            base.Draw(gameTime);
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
                animation(gameTime);
            }
            else
            {
                currentAnimationIndex = 1;
                playerTimer = 0f;
            }

            int playerWidth = 35 * 2;
            int playerHeight = 36 * 2;
            Rectangle windowEdge = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            Rectangle rockHitbox = new Rectangle((int)rockPosition.X, (int)rockPosition.Y, _rock.Width, _rock.Height);

            playerPosition.X += movement.X * velocidadGlobal * delta;

            if (playerPosition.X < windowEdge.Left) playerPosition.X = windowEdge.Left;
            else if (playerPosition.X + playerWidth > windowEdge.Right) playerPosition.X = windowEdge.Right - playerWidth;

            Rectangle playerHitboxX = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            if (playerHitboxX.Intersects(rockHitbox))
            {
                if (movement.X > 0) playerPosition.X = rockHitbox.Left - playerWidth;
                else if (movement.X < 0) playerPosition.X = rockHitbox.Right;
            }

            playerPosition.Y += movement.Y * velocidadGlobal * delta;

            if (playerPosition.Y < windowEdge.Top) playerPosition.Y = windowEdge.Top;
            else if (playerPosition.Y + playerHeight > windowEdge.Bottom) playerPosition.Y = windowEdge.Bottom - playerHeight;

            Rectangle playerHitboxY = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            if (playerHitboxY.Intersects(rockHitbox))
            {
                if (movement.Y > 0) playerPosition.Y = rockHitbox.Top - playerHeight;
                else if (movement.Y < 0) playerPosition.Y = rockHitbox.Bottom;
            }
        }

        private void inventory()
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Tab) && _oldKS.IsKeyUp(Keys.Tab))
            {
                _openInventory = !_openInventory; // Si estaba falso pasa a verdadero, si estaba verdadero pasa a falso
            }

            _oldKS = ks;
        }

        private void keyboardMovement(GameTime gameTime, ref Vector2 character)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            if (ks.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (ks.IsKeyDown(Keys.S)) movement.Y += 1;
            if (ks.IsKeyDown(Keys.A)) movement.X -= 1;
            if (ks.IsKeyDown(Keys.D)) movement.X += 1;

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                character += movement * velocidadGlobal * delta;

                if (movement.X < 0 && movement.Y < 0) playerDirection = 6;
                else if (movement.X > 0 && movement.Y < 0) playerDirection = 7;
                else if (movement.X < 0 && movement.Y > 0) playerDirection = 4;
                else if (movement.X > 0 && movement.Y > 0) playerDirection = 5;
                else if (movement.Y < 0) playerDirection = 1;
                else if (movement.Y > 0) playerDirection = 0;
                else if (movement.X < 0) playerDirection = 2;
                else if (movement.X > 0) playerDirection = 3;

                animation(gameTime);
            }
            else
            {
                currentAnimationIndex = 1;
                playerTimer = 0f;
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
