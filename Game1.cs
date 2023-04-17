
using Accessibility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MonoGames2
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont MyText;
        List<Bullet> bullets;



        Texture2D playerTexture; // наш спрайт
        Texture2D playerTextureStay;
        Texture2D playerJump;
        Texture2D playerFall;
        Texture2D playerRun;
        Texture2D evilTexture; // второй спрайт
        Texture2D[] background = new Texture2D[5];
        Texture2D black_boxT;
        Texture2D healthBar;
        Texture2D healthBarBG;
        Texture2D[] bulletTexture = new Texture2D[5];
        Texture2D whiteDotTexture;
        Texture2D whiteDotTexture2;
        Texture2D DialogTexture;

        Vector2 black_box;
        Vector2 playerPosition; // позиция нашего спрайта
        Vector2 evilSpritePosition; // позиция второго спрайта
        Vector2 whiteDotPosition;
        Vector2 whiteDotPosition2;
        Vector2 healthBarPosition;
        Vector2 healthBarBGPosition;
        Vector2 DialogPosition;

        Point playerSize; // размер нашего спрайта
        Point evilSpriteSize; // размер второго спрайта
        Point framePlayer = new Point(0, 0);
        Point frameSize = new Point(6, 0);
        Point framePlayerStay = new Point(0, 0);
        Point frameSizePlayerStay = new Point(6, 0);
        Point framePlayerJumpOrFall = new Point(0, 0);
        Point frameSizeJumpOrFall = new Point(2, 0);

        public SpriteFont DialogFont { get; private set; }

        const int ground = 995 - playerHeight;
        const int frameWidth = 231;
        const int frameHeight = 85;
        const int playerWidth = 56;
        const int playerHeight = 85;

        float playerSpeed = 10f;
        float evilSpriteSpeed = 2f;
        float jumpSpeed = 0;
        float gravity = 0.5f;


        int maxHealth = 100;
        int health = 50;
        int LVL = 0;


        bool turnRight;


        bool isFalling = false;
        float totalTimer = 0;
        float totalTimer2 = 0;

        private readonly float fireRate = 4f; // Количество выстрелов в секунду

        Color color = Color.CadetBlue;
        Matrix projectionMatrix; // матрица проекции
        //Matrix viewMatrix; // матрица вида
        //Matrix worldMatrix; // мировая матрица

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            Content.RootDirectory = "Content";

            //  помещаем второй спрайт на середину по оси Х
            evilSpritePosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight - 119);
            playerPosition = new Vector2(20, ground - playerHeight);//==============================================================
            healthBarPosition = new Vector2(16, 17);
            healthBarBGPosition = new Vector2(16, 16);
            black_box = new Vector2(graphics.PreferredBackBufferWidth - 32, graphics.PreferredBackBufferHeight - 64);
            whiteDotPosition = new Vector2(playerPosition.X, playerPosition.Y);
            DialogPosition = new Vector2(200, 200);

        }

        protected override void Initialize()
        {
            graphics.ApplyChanges();
            //projectionMatrix = Matrix.CreateOrthographicOffCenter(0, graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferWidth, 0,0,1);
            bullets = new List<Bullet>();
            //worldMatrix = Matrix.Identity;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background[0] = Content.Load<Texture2D>("trees");
            background[1] = Content.Load<Texture2D>("trees2");
            background[2] = Content.Load<Texture2D>("trees3");
            background[3] = Content.Load<Texture2D>("trees4");
            bulletTexture[0] = Content.Load<Texture2D>("02");
            bulletTexture[1] = Content.Load<Texture2D>("07");
            bulletTexture[2] = Content.Load<Texture2D>("08");
            bulletTexture[3] = Content.Load<Texture2D>("10");
            playerTextureStay = Content.Load<Texture2D>("PlayerAnim/Idle");
            playerJump = Content.Load<Texture2D>("PlayerAnim/Jump");
            playerFall = Content.Load<Texture2D>("PlayerAnim/Fall");
            evilTexture = Content.Load<Texture2D>("evil");
            black_boxT = Content.Load<Texture2D>("black_box");
            healthBar = Content.Load<Texture2D>("red");
            healthBarBG = Content.Load<Texture2D>("bg");
            MyText = Content.Load<SpriteFont>("MyText");
            whiteDotTexture = Content.Load<Texture2D>("whiteDot");
            whiteDotTexture2 = Content.Load<Texture2D>("whiteDot");
            DialogTexture = Content.Load<Texture2D>("DialogBoxBackground");
            playerTexture = playerTextureStay;
            // Устанавливаем размеры спрайтов
            playerSize = new Point(playerWidth, playerHeight);
            evilSpriteSize = new Point(evilTexture.Width, evilTexture.Height);

        }
        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            totalTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            totalTimer2 += gameTime.ElapsedGameTime.Milliseconds;
            //ВЫХОД
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();


            whiteDotPosition2 = new Vector2(playerPosition.X + (float)playerSize.X, playerPosition.Y + (float)playerSize.Y);
            Player();
            if (playerPosition.X >= (1920 - playerWidth))
                NewLocation(+1);
            if (playerPosition.X <= (0 + playerWidth))
                NewLocation(-1);

            Falling();

            //ПРЫЖКИ
            if (isFalling)
                playerPosition.Y -= gravity;
            playerPosition.Y -= jumpSpeed;
            if (jumpSpeed != 0 || isFalling)
            {
                jumpSpeed -= gravity;
                if (playerPosition.Y >= ground)
                {
                    jumpSpeed = 0;
                    isFalling = false;
                    playerPosition.Y = ground;
                }
            }
            Enemy();

            foreach (Bullet bullet in bullets)
            {
                bullet.BulletPosition += new Vector2(bullet.Speed, 0);
            }
            if (Collide())
            {
                if (health >= 0)
                    health = health - 1;
            }
            else
            {
                if (health <= 100)
                    health = health + 1;
            }
            whiteDotPosition = new Vector2(playerPosition.X - 10, playerPosition.Y - 10);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(color);
            spriteBatch.Begin();

            spriteBatch.Draw(background[LVL], Vector2.Zero, Color.White);
            if (turnRight)
                spriteBatch.Draw(playerTexture, playerPosition,
                    new Rectangle(framePlayer.X * frameWidth,
                        framePlayer.Y * frameHeight,
                        frameWidth, frameHeight),
                    Color.White, 0, Vector2.Zero,
                    1, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(playerTexture, playerPosition - new Vector2(176, 0),
                    new Rectangle(framePlayer.X * frameWidth,
                        framePlayer.Y * frameHeight,
                        frameWidth, frameHeight),
                    Color.White, 0, Vector2.Zero,
                    1, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(evilTexture, evilSpritePosition, Color.White);
            if (isFalling)
                spriteBatch.Draw(black_boxT, black_box, Color.White);
            spriteBatch.Draw(healthBarBG, healthBarBGPosition, Color.White);
            //spriteBatch.Draw(healthBar, healthBarPosition, Color.White);
            spriteBatch.Draw(healthBar, healthBarPosition, new Rectangle(0, 0, (int)((health / (float)maxHealth) * (healthBar.Width - 4)), healthBar.Height), Color.Red);
            foreach (Bullet bullet in bullets)
            {
                spriteBatch.Draw(bulletTexture[LVL], bullet.BulletPosition, Color.White);
            }
            spriteBatch.DrawString(MyText, playerPosition.X.ToString() + "X", new Vector2(10, 120), Color.Azure); // draw text
            spriteBatch.DrawString(MyText, playerPosition.Y.ToString() + "Y " + (float)gameTime.ElapsedGameTime.TotalMilliseconds + "\n" + "Some text", new Vector2(10, 145), Color.Azure);
            spriteBatch.Draw(whiteDotTexture, whiteDotPosition, Color.White);
            spriteBatch.Draw(whiteDotTexture2, whiteDotPosition2, Color.White);
            spriteBatch.Draw(DialogTexture, DialogPosition, null, Color.White,0,Vector2.Zero, 2.0f,SpriteEffects.None,0);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected bool Collide()
        {
            Rectangle goodSpriteRect = new Rectangle((int)playerPosition.X,
                (int)playerPosition.Y, playerSize.X, playerSize.Y);
            Rectangle evilSpriteRect = new Rectangle((int)evilSpritePosition.X,
                (int)evilSpritePosition.Y, evilSpriteSize.X, evilSpriteSize.Y);

            return goodSpriteRect.Intersects(evilSpriteRect);
        }
        protected bool Gravity(int position)
        {
            if (position > 119)
                return true;
            else
                return false;
        }
        public void Enemy()
        {
            evilSpritePosition.X += evilSpriteSpeed;
            if (evilSpritePosition.X > Window.ClientBounds.Width - evilTexture.Width || evilSpritePosition.X < 0)
                evilSpriteSpeed *= -1;
        }
        public void Player()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            //Отрисовка спрайта игрока
            if (totalTimer2 >= 150)
            {
                ++framePlayer.X;
                totalTimer2 = 0;
                if (framePlayer.X >= frameSize.X)
                {
                    framePlayer.X = 0;
                    ++framePlayer.Y;
                    if (framePlayer.Y >= frameSize.Y)
                        framePlayer.Y = 0;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                playerPosition.X -= playerSpeed;
                turnRight = false;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                playerPosition.X += playerSpeed;
                turnRight = true;
            }
            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, GraphicsDevice.Viewport.Width - playerWidth);
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                if (!isFalling)
                    jumpSpeed = 10f;
            }
            if (keyboardState.IsKeyDown(Keys.Space) && (totalTimer >= (1.0f / fireRate)))
            {
                FireBullet(turnRight);
                totalTimer = 0;
            }
            /*if (keyboardState.IsKeyDown(Keys.Down))
                goodSpritePosition.Y += goodSpriteSpeed;*/
            if (jumpSpeed > 0)
            {
                playerTexture = playerJump;
                framePlayer = framePlayerJumpOrFall;
                frameSize = frameSizeJumpOrFall;
            }
            else if (jumpSpeed < 0)
            {
                playerTexture = playerFall;
                framePlayer = framePlayerJumpOrFall;
                frameSize = frameSizeJumpOrFall;
            }
            else
            {
                playerTexture = playerTextureStay;
                framePlayer = framePlayerStay;
                frameSize = frameSizePlayerStay;
            }
        }
        //Проверка на падение
        public void Falling()
        {
            if (playerPosition.Y < ground)
                isFalling = true;
            else
                isFalling = false;
        }
        public void NewLocation(int i)
        {
            if (i > 0 && LVL < 3)
            {
                LVL++;
                playerPosition.X = playerWidth + 10;
            }
            if (i < 0 && LVL > 0)
            {
                LVL--;
                playerPosition.X = 1920 - playerWidth - 10;
            }
        }
        public void FireBullet(bool turnRight)
        {
            Vector2 bulletPosition;
            float velocity;
            if (turnRight)
            {
                velocity = 1;
                bulletPosition = new Vector2(playerPosition.X, playerPosition.Y - (playerHeight / 4));
            }
            else
            {
                velocity = -1;
                bulletPosition = new Vector2(playerPosition.X - playerWidth, playerPosition.Y - (playerHeight / 4));
            }
            Bullet bullet = new Bullet(bulletPosition, velocity);
            bullets.Add(bullet);
        }

    }
    public class Bullet
    {
        public Vector2 BulletPosition { get; set; }
        //public Point BulletPoint { get; set; }
        public float Damage { get; }
        public float Speed { get; set; }

        public Bullet(Vector2 bulletPosition, float velocity)
        {
            BulletPosition = bulletPosition;
            //BulletPoint = bulletPoint;
            Damage = 10f;
            float speed = 15f;
            Speed = speed * velocity;
        }
    }
    public class DialogBox
    {
        public string Text { get; set; }

        public int MaxLength { get; set; }
    }
}