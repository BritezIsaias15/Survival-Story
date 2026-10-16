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
        private float velocidadGlobal = 5.0f;
        float timer;

        int threshold;

        Rectangle[,] gligarRectangle;

        byte previousAnimationIndex;
        byte currentAnimationIndex;
        int direction;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //_background = Content.Load<Texture2D>("Fondo/Fondo");
            _gligar = Content.Load<Texture2D>("assets/images/gligarSheet");
            //_wooper = Content.Load<Texture2D>("images/wooperSheet");

            timer = 0;

            threshold = 250;

            gligarRectangle = new Rectangle[8, 3];
            int sheetX = 0;
            int sheetY = 0;
            int gligarWidth = 35;
            int gligarHeight = 36;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gligarRectangle[i, j] = new Rectangle(sheetX, sheetY, gligarWidth, gligarHeight);
                    sheetX += gligarWidth;
                }
                sheetX = 0;
                sheetY += gligarHeight;
            }

            previousAnimationIndex = 2;
            currentAnimationIndex = 1;

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            movimientoTeclado(gameTime);

            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            /*_spriteBatch.Draw
            (_background,
            new Vector2(-75, 0),
            null,
            Color.White,
            0.0f,
            Vector2.Zero,
            2.2f,
            SpriteEffects.None,
            0.0f);*/

            _spriteBatch.Draw(
            _gligar,
            gligarPosition,
            gligarRectangle[direction, currentAnimationIndex],
            Color.White,
            0.0f,               // Rotación
            Vector2.Zero,       // Origen (pivote)
            2.0f,       // ESCALA (Aquí cambias el tamaño)
            SpriteEffects.None, // Efectos de espejo
            0.0f                // Capa de profundidad
            );

            _spriteBatch.End();
            base.Draw(gameTime);
        }
        void movimientoTeclado(GameTime gameTime)
        {
            float velocidadMovimiento = velocidadGlobal;
            KeyboardState teclaPresionada = Keyboard.GetState();

            if (teclaPresionada.IsKeyDown(Keys.W) && teclaPresionada.IsKeyDown(Keys.A) && teclaPresionada.IsKeyUp(Keys.D))
            {
                gligarPosition.Y -= velocidadGlobal;
                gligarPosition.X -= velocidadGlobal;
                direction = 6;
            }

            if (teclaPresionada.IsKeyDown(Keys.W) && teclaPresionada.IsKeyDown(Keys.D) && teclaPresionada.IsKeyUp(Keys.A))
            {
                gligarPosition.Y -= velocidadGlobal;
                gligarPosition.X += velocidadGlobal;
                direction = 7;
            }

            if (teclaPresionada.IsKeyDown(Keys.S) && teclaPresionada.IsKeyDown(Keys.A) && teclaPresionada.IsKeyUp(Keys.D))
            {
                gligarPosition.Y += velocidadGlobal;
                gligarPosition.X -= velocidadGlobal;
                direction = 4;
            }

            if (teclaPresionada.IsKeyDown(Keys.S) && teclaPresionada.IsKeyDown(Keys.D) && teclaPresionada.IsKeyUp(Keys.A))
            {
                gligarPosition.Y += velocidadGlobal;
                gligarPosition.X += velocidadGlobal;
                direction = 5;
            }

            if (teclaPresionada.IsKeyDown(Keys.W) && teclaPresionada.IsKeyUp(Keys.A) && teclaPresionada.IsKeyUp(Keys.D))
            {
                gligarPosition.Y -= velocidadGlobal;
                direction = 1;
            }

            if (teclaPresionada.IsKeyDown(Keys.S) && teclaPresionada.IsKeyUp(Keys.A) && teclaPresionada.IsKeyUp(Keys.D))
            {
                gligarPosition.Y += velocidadGlobal;
                direction = 0;
            }

            if (teclaPresionada.IsKeyDown(Keys.A) && teclaPresionada.IsKeyUp(Keys.W) && teclaPresionada.IsKeyUp(Keys.S))
            {
                gligarPosition.X -= velocidadGlobal;
                direction = 2;
            }

            if (teclaPresionada.IsKeyDown(Keys.D) && teclaPresionada.IsKeyUp(Keys.W) && teclaPresionada.IsKeyUp(Keys.S))
            {
                gligarPosition.X += velocidadGlobal;
                direction = 3;
            }

            animation(gameTime);
        }

        void animation(GameTime gameTime)
        {
            if (timer > threshold)
            {
                if (currentAnimationIndex == 1)
                {
                    if (previousAnimationIndex == 0)
                    {
                        currentAnimationIndex = 2;
                    }
                    else
                    {
                        currentAnimationIndex = 0;
                    }
                    previousAnimationIndex = currentAnimationIndex;
                }
                else
                {
                    currentAnimationIndex = 1;
                }
                timer = 0;
            }
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }
    }
}