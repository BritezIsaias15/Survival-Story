using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survival_Story
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _gligar;
        private Texture2D _wooper;

        private Vector2 gligarPosition;
        private Vector2 wooperPosition;
        private float velocidadGlobal = 250.0f; // píxeles por segundo

        private float gligarTimer;
        private float wooperTimer;
        private int threshold;

        private Rectangle[,] gligarRectangle;
        private Rectangle[,] wooperRectangle;

        private byte gligarPreviousAnimationIndex;
        private byte gligarCurrentAnimationIndex;
        private int gligarDirection;

        private byte wooperPreviousAnimationIndex;
        private byte wooperCurrentAnimationIndex;
        private int wooperDirection;

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

            gligarTimer = 0f;
            wooperTimer = 0f;
            threshold = 250;

            gligarDirection = 0;
            wooperDirection = 0;

            gligarPreviousAnimationIndex = 2;
            gligarCurrentAnimationIndex = 1;

            wooperPreviousAnimationIndex = 2;
            wooperCurrentAnimationIndex = 1;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _gligar = Content.Load<Texture2D>("assets/images/gligarSheet");
            _wooper = Content.Load<Texture2D>("assets/images/wooperSheet");

            // Gligar sheet (8x3)
            gligarRectangle = new Rectangle[8, 3];
            int gligarWidth = 35;
            int gligarHeight = 36;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gligarRectangle[i, j] = new Rectangle(j * gligarWidth, i * gligarHeight, gligarWidth, gligarHeight);
                }
            }

            // Wooper sheet (8x3)
            wooperRectangle = new Rectangle[8, 3];
            int wooperWidth = 27;
            int wooperHeight = 26;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    wooperRectangle[i, j] = new Rectangle(j * wooperWidth, i * wooperHeight, wooperWidth, wooperHeight);
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Vector2 previousGligarPosition = gligarPosition;
            Vector2 previousWooperPosition = wooperPosition;

            movimientoTecladoGligar(gameTime, ref gligarPosition);
            movimientoTecladoWooper(gameTime, ref wooperPosition);

            int gligarWidth = 35 * 2;
            int gligarHeight = 36 * 2;
            int wooperWidth = 27 * 2;
            int wooperHeight = 26 * 2;

            Rectangle bordedepantalla = new Rectangle(
                0, 0,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight
            );

            if (gligarPosition.X < bordedepantalla.Left) gligarPosition.X = bordedepantalla.Left;
            else if (gligarPosition.X + gligarWidth > bordedepantalla.Right) gligarPosition.X = bordedepantalla.Right - gligarWidth;

            if (gligarPosition.Y < bordedepantalla.Top) gligarPosition.Y = bordedepantalla.Top;
            else if (gligarPosition.Y + gligarHeight > bordedepantalla.Bottom) gligarPosition.Y = bordedepantalla.Bottom - gligarHeight;

            if (wooperPosition.X < bordedepantalla.Left) wooperPosition.X = bordedepantalla.Left;
            else if (wooperPosition.X + wooperWidth > bordedepantalla.Right) wooperPosition.X = bordedepantalla.Right - wooperWidth;

            if (wooperPosition.Y < bordedepantalla.Top) wooperPosition.Y = bordedepantalla.Top;
            else if (wooperPosition.Y + wooperHeight > bordedepantalla.Bottom) wooperPosition.Y = bordedepantalla.Bottom - wooperHeight;

            Rectangle gligarhitbox = new Rectangle((int)gligarPosition.X, (int)gligarPosition.Y, gligarWidth, gligarHeight);
            Rectangle wooperhitbox = new Rectangle((int)wooperPosition.X, (int)wooperPosition.Y, wooperWidth, wooperHeight);

            if (gligarhitbox.Intersects(wooperhitbox))
            {
                gligarPosition = previousGligarPosition;
                wooperPosition = previousWooperPosition;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(
                _gligar,
                gligarPosition,
                gligarRectangle[gligarDirection, gligarCurrentAnimationIndex],
                Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f
            );

            _spriteBatch.Draw(
                _wooper,
                wooperPosition,
                wooperRectangle[wooperDirection, wooperCurrentAnimationIndex],
                Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f
            );

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void movimientoTecladoGligar(GameTime gameTime, ref Vector2 personaje)
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

                if (movimiento.X < 0 && movimiento.Y < 0) gligarDirection = 6;
                else if (movimiento.X > 0 && movimiento.Y < 0) gligarDirection = 7;
                else if (movimiento.X < 0 && movimiento.Y > 0) gligarDirection = 4;
                else if (movimiento.X > 0 && movimiento.Y > 0) gligarDirection = 5;
                else if (movimiento.Y < 0) gligarDirection = 1;
                else if (movimiento.Y > 0) gligarDirection = 0;
                else if (movimiento.X < 0) gligarDirection = 2;
                else if (movimiento.X > 0) gligarDirection = 3;

                animationGligar(gameTime);
            }
            else
            {
                gligarCurrentAnimationIndex = 1;
                gligarTimer = 0f;
            }
        }

        private void movimientoTecladoWooper(GameTime gameTime, ref Vector2 personaje)
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

                animationWooper(gameTime);
            }
            else
            {
                wooperCurrentAnimationIndex = 1;
                wooperTimer = 0f;
            }
        }

        private void animationGligar(GameTime gameTime)
        {
            gligarTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (gligarTimer > threshold)
            {
                if (gligarCurrentAnimationIndex == 1)
                {
                    gligarCurrentAnimationIndex = (gligarPreviousAnimationIndex == 0) ? (byte)2 : (byte)0;
                    gligarPreviousAnimationIndex = gligarCurrentAnimationIndex;
                }
                else
                {
                    gligarCurrentAnimationIndex = 1;
                }
                gligarTimer -= threshold;
            }
        }

        private void animationWooper(GameTime gameTime)
        {
            wooperTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

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
                wooperTimer -= threshold;
            }
        }
    }
}
