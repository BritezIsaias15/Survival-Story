using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survival_Story
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;

        private Texture2D _gligar;
        private Texture2D _wooper;

        private Vector2 gligarPosition;
        private Vector2 wooperPosition;
        private float velocidadGlobal = 150.0f; // píxeles por segundo

        float timer;
        float wooperTimer;

        int threshold;

        Rectangle[,] gligarRectangle;
        Rectangle[,] wooperRectangle;

        byte previousAnimationIndex;
        byte currentAnimationIndex;
        int direction;

        byte wooperPreviousAnimationIndex;
        byte wooperCurrentAnimationIndex;
        int wooperDirection;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            gligarPosition = new Vector2(100, 100);
            wooperPosition = new Vector2(200, 100);

            timer = 0f;
            wooperTimer = 0f;

            threshold = 250;

            direction = 0;
            wooperDirection = 0;

            previousAnimationIndex = 2;
            currentAnimationIndex = 1;

            wooperPreviousAnimationIndex = 2;
            wooperCurrentAnimationIndex = 1;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //_background = Content.Load<Texture2D>("Fondo/Fondo");
            _gligar = Content.Load<Texture2D>("assets/images/gligarSheet");
            _wooper = Content.Load<Texture2D>("assets/images/wooperSheet");

            // Gligar sheet (8x3)
            gligarRectangle = new Rectangle[8, 3];
            int gligarX = 0;
            int gligarY = 0;
            int gligarWidth = 35;
            int gligarHeight = 36;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gligarRectangle[i, j] = new Rectangle(gligarX, gligarY, gligarWidth, gligarHeight);
                    gligarX += gligarWidth;
                }
                gligarX = 0;
                gligarY += gligarHeight;
            }

            // Wooper sheet (8x3)
            wooperRectangle = new Rectangle[8, 3];
            int wooperX = 0;
            int wooperY = 0;
            int wooperWidth = 27;
            int wooperHeight = 26;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    wooperRectangle[i, j] = new Rectangle(wooperX, wooperY, wooperWidth, wooperHeight);
                    wooperX += wooperWidth;
                }
                wooperX = 0;
                wooperY += wooperHeight;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            movimientoTecladoGligar(gameTime, ref gligarPosition);
            movimientoTecladoWooper(gameTime, ref wooperPosition);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Dibujar gligar
            _spriteBatch.Draw(
                _gligar,
                gligarPosition,
                gligarRectangle[direction, currentAnimationIndex],
                Color.White,
                0.0f,
                Vector2.Zero,
                2.0f,
                SpriteEffects.None,
                0.0f
            );

            // Dibujar wooper
            _spriteBatch.Draw(
                _wooper,
                wooperPosition,
                wooperRectangle[wooperDirection, wooperCurrentAnimationIndex],
                Color.White,
                0.0f,
                Vector2.Zero,
                2.0f,
                SpriteEffects.None,
                0.0f
            );

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        void movimientoTecladoGligar(GameTime gameTime, ref Vector2 personaje)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 movimiento = Vector2.Zero;

            if (ks.IsKeyDown(Keys.W)) movimiento.Y -= 1;
            if (ks.IsKeyDown(Keys.S)) movimiento.Y += 1;
            if (ks.IsKeyDown(Keys.A)) movimiento.X -= 1;
            if (ks.IsKeyDown(Keys.D)) movimiento.X += 1;

            if (movimiento != Vector2.Zero)
            {
                movimiento.Normalize();
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                personaje += movimiento * velocidadGlobal * delta;

                if (movimiento.X < 0 && movimiento.Y < 0) direction = 6;
                else if (movimiento.X > 0 && movimiento.Y < 0) direction = 7;
                else if (movimiento.X < 0 && movimiento.Y > 0) direction = 4;
                else if (movimiento.X > 0 && movimiento.Y > 0) direction = 5;
                else if (movimiento.Y < 0) direction = 1;
                else if (movimiento.Y > 0) direction = 0;
                else if (movimiento.X < 0) direction = 2;
                else if (movimiento.X > 0) direction = 3;
            }

            animationGligar(gameTime);
        }

        void movimientoTecladoWooper(GameTime gameTime, ref Vector2 personaje)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 movimiento = Vector2.Zero;

            if (ks.IsKeyDown(Keys.Up)) movimiento.Y -= 1;
            if (ks.IsKeyDown(Keys.Down)) movimiento.Y += 1;
            if (ks.IsKeyDown(Keys.Left)) movimiento.X -= 1;
            if (ks.IsKeyDown(Keys.Right)) movimiento.X += 1;

            if (movimiento != Vector2.Zero)
            {
                movimiento.Normalize();
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                personaje += movimiento * velocidadGlobal * delta;

                if (movimiento.X < 0 && movimiento.Y < 0) wooperDirection = 6;
                else if (movimiento.X > 0 && movimiento.Y < 0) wooperDirection = 7;
                else if (movimiento.X < 0 && movimiento.Y > 0) wooperDirection = 4;
                else if (movimiento.X > 0 && movimiento.Y > 0) wooperDirection = 5;
                else if (movimiento.Y < 0) wooperDirection = 1;
                else if (movimiento.Y > 0) wooperDirection = 0;
                else if (movimiento.X < 0) wooperDirection = 2;
                else if (movimiento.X > 0) wooperDirection = 3;
            }

            animationWooper(gameTime);
        }

        void animationGligar(GameTime gameTime)
        {
            if (timer > threshold)
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
                timer = 0f;
            }
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }

        void animationWooper(GameTime gameTime)
        {
            if (wooperTimer > threshold)
            {
                if (wooperCurrentAnimationIndex == 1)
                {
                    wooperCurrentAnimationIndex = (wooperPreviousAnimationIndex == 0) ? (byte)2 : (byte)0;
                    wooperPreviousAnimationIndex = wooperCurrentAnimationIndex;
                }
                else
                {
                    wooperCurrentAnimationIndex = 1;
                }
                wooperTimer = 0f;
            }
            else
            {
                wooperTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }
    }
}
