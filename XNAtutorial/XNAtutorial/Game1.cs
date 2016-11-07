using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNAtutorial
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public struct PlayerData//savaş topu verilerini
    {
        public Vector2 Position;//savaş topunun koordinatları
        public bool IsAlive;//
        public Color Color;//savaş topunun rengi
        public float Angle;//
        public float Power;//
    }

    public struct ParticleData//patlama özelliklerini içeren yapı
    {
        public float BirthTime;
        public float MaxAge;
        public Vector2 OrginalPosition;
        public Vector2 Accelaration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public Color ModColor;
    }
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;//Aygıt tanımlama değişkeni

        Texture2D backgroundTexture;//Arkaplan ekleme değişkeni
        int screenWidth;//Ekran genişliği ile resmi aynı boyuta getirmek için
        int screenHeight;//Ekran yüksekliği ile resmi aynı boyuta getirmek için
        Texture2D foregroundTexture;//Önplan ekleme değişkeni

        Texture2D carriageTexture;//top arabası ekleme değişkeni
        Texture2D cannonTexture;//mermi ekleme değişkeni
        PlayerData[] players;//savaş topu dizisi oluşturulur
        int numberOfPlayers = 4;//bilmiyorum

        float playerScaling;//topun ölçeklenmesi

        int currentPlayer = 0;//klavye okuma

        SpriteFont font;//font değişkeni

        Texture2D rocketTexture;//roket değişkeni
        bool rocketFlying = false;//roket değişkenleri
        Vector2 rocketPosition;
        Vector2 rocketDirection;
        float rocketAngle;
        float rocketScaling = 0.1f;

        Texture2D smokeTexture;//duman değişkeni
        List<Vector2> smokeList = new List<Vector2>();//duman parçacıkları listesi
        Random randomizer = new Random();//dumanın roketin arkasındaki rastgele alana konumlanmasını sağlar

        int[] terrainContour;//kayalık resminin y koordinatları tutulur

        Texture2D groundTexture;//toprak değişkeni

        Color[,] rocketColorArray;//renk dizinleri
        Color[,] foregroundColorArray;
        Color[,] carriageColorArray;
        Color[,] cannonColorArray;

        Texture2D explosionTexture;//ateş değişkeni
        List<ParticleData> particleList = new List<ParticleData>();//patlama anındaki tüm özellikleri hafızaya alan değişken

        Color[,] explosionColorArray;//patlama kriter eklemek için kullanılır

        SoundEffect StartGame;//ses değişkenleri
        SoundEffect hitCannon;
        SoundEffect hitTerrain;
        SoundEffect launch;

        const bool resultionIndependent = false;//çözünürlük ayarı için kullanılır
        Vector2 baseScreenSize = new Vector2(800, 600);
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 500;//Genişlik boyutu
            graphics.PreferredBackBufferHeight = 500;//Yükseklik boyutu
            graphics.IsFullScreen = false;//Tam ekran modu
            graphics.ApplyChanges();//Bilmiyorum
            Window.Title = "Riemer's 2D XNA Tutorial";//Başlık değiştirilir
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            // TODO: use this.Content to load your game content here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;//Grafic aygıt yüklemeler yapılır.

            backgroundTexture = Content.Load<Texture2D>("background");//Arkaplan resmi değişkene atanır
            screenWidth = device.PresentationParameters.BackBufferWidth;//Pencere boyutunun genişliği atanır
            screenHeight = device.PresentationParameters.BackBufferHeight;//Pencere boyutunun yüksekliği atanır
            //foregroundTexture = Content.Load<Texture2D>("foreground");//Önaplan resmi değişkene atanır

            GenerateTerrainContour();//arazinin yamacını üretir
            SetUpPlayers();//Oyun araçlarının ayarları yapılır.
            FlattenTerrainBelowPlayers();//top arabalarının arazi üzerindeki yerini açar
            groundTexture = Content.Load<Texture2D>("ground");//toprak resmi değişkene atanır

            CreateForeground();//ön plan doku üretir

            carriageTexture = Content.Load<Texture2D>("carriage");//top arabası resmi değişkene atanır
            cannonTexture = Content.Load<Texture2D>("cannon");//mermi resmi değişkene atanır

            playerScaling = 40.0f / (float)carriageTexture.Width;//top arabasının yerini belirleyen ölçektir

            font = Content.Load<SpriteFont>("myFont");//font değişkene atanır

            rocketTexture = Content.Load<Texture2D>("rocket");//roket resmi değişkene atanır

            smokeTexture = Content.Load<Texture2D>("smoke");//duman resmi değişkene atanır

            rocketColorArray = TextureTo2DArray(rocketTexture);//resim galiba 2 boyutlu array olarak atanır
            carriageColorArray = TextureTo2DArray(carriageTexture);
            cannonColorArray = TextureTo2DArray(cannonTexture);

            explosionTexture = Content.Load<Texture2D>("explosion");//ateş resmi değişkene atanır

            explosionColorArray = TextureTo2DArray(explosionTexture);//patlama kriter eklemek için kullanılır

            StartGame = Content.Load<SoundEffect>("StartGame");

            //hitCannon = Content.Load<SoundEffect>("hitCannon");//ses dosyaları değişkene atanır
            hitTerrain = Content.Load<SoundEffect>("hitTerrain");
            //launch = Content.Load<SoundEffect>("launch");


            if (resultionIndependent)//çözünürlük ayarı için
            {
                screenWidth = (int)baseScreenSize.X;
                screenHeight = (int)baseScreenSize.Y;
            }
            else
            {
                screenWidth = device.PresentationParameters.BackBufferWidth;
                screenHeight = device.PresentationParameters.BackBufferHeight;
            }

            StartGame.Play();//kendim ekledim
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // TODO: Add your update logic here

            ProcessKeyboard();//klavye işlemi yapılır

            UpdateRocket();//roket yeri güncelleştirilir

            if (rocketFlying)//çarpışma algılamak için
            {
                UpdateRocket();
                CheckCollisions(gameTime);
            }

            if (particleList.Count > 0)//ateşe parçacıklı efekt verir
                UpdateParticles(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector3 screenScalingFactor;//çözünürlük ayarı için
            if (resultionIndependent)
            {
                float horScaling = (float)device.PresentationParameters.BackBufferWidth / baseScreenSize.X;
                float verScaling = (float)device.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
                screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            }
            else
            {
                screenScalingFactor = new Vector3(1, 1, 1);
            }
            Matrix globalTransformation = Matrix.CreateScale(screenScalingFactor);

            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, globalTransformation);//çözünürlük ayarıyla SpriteBatch i başlatır
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, globalTransformation);

            DrawScenery();//arkaplan resimlerini SpriteBatch işlemi yapar

            DrawPlayers();//oyun araçlarını çizer

            DrawText();//yazı çizimini çağırır

            DrawRocket();//roketi çizer

            DrawSmoke();//dumanı çizer

            spriteBatch.End();//SpriteBatch i sonlandırır

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, globalTransformation);
            //spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Deferred, SaveStateMode.None, globalTransformation);//çözünürlük ayarıyla ateş resmindeki siyah rengi saydam etmek için
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            DrawExplosion();//ateş partiküllerini çizer
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawScenery()//arkaplan resimlerini yerleştirir
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);//Dikdörtgen tanımlanır, içine resim koymak için
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);//tanımlanan dikdörtgene arkaplan resim konur
            spriteBatch.Draw(foregroundTexture, screenRectangle, Color.White);//tanımlanan dikdörtgene önaplan resim konur

        }

        private void SetUpPlayers()//Oyun araçlarının ayarları yapılır.
        {
            Color[] playerColors = new Color[10];
            playerColors[0] = Color.Red;
            playerColors[1] = Color.Green;
            playerColors[2] = Color.Blue;
            playerColors[3] = Color.Purple;
            playerColors[4] = Color.Orange;
            playerColors[5] = Color.Indigo;
            playerColors[6] = Color.Yellow;
            playerColors[7] = Color.SaddleBrown;
            playerColors[8] = Color.Tomato;
            playerColors[9] = Color.Turquoise;
            players = new PlayerData[numberOfPlayers];
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].IsAlive = true;
                players[i].Color = playerColors[i];
                players[i].Angle = MathHelper.ToRadians(90);
                players[i].Power = 100;
                players[i].Position = new Vector2();
                players[i].Position.X = screenWidth / (numberOfPlayers + 1) * (i + 1);
                players[i].Position.Y = terrainContour[(int)players[i].Position.X];//arazi üzerindeki x konumuna göre y konumunu hesaplar
            }
        }
        private void DrawPlayers()//oyun araçlarını çizer
        {
            foreach (PlayerData player in players)
            {
                if (player.IsAlive)
                {
                    int xPos = (int)player.Position.X;
                    int yPos = (int)player.Position.Y;
                    Vector2 cannonOrigin = new Vector2(11, 50);
                    spriteBatch.Draw(cannonTexture, new Vector2(xPos + 20, yPos - 10), null, player.Color, player.Angle, cannonOrigin, playerScaling, SpriteEffects.None, 1);
                    spriteBatch.Draw(carriageTexture, player.Position, null, player.Color, 0, new Vector2(0, carriageTexture.Height), playerScaling, SpriteEffects.None, 0);
                }
            }
        }

        private void ProcessKeyboard()//kavye işlemleri
        {
            KeyboardState keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Left))
                players[currentPlayer].Angle -= 0.01f;
            if (keybState.IsKeyDown(Keys.Right))
                players[currentPlayer].Angle += 0.01f;
            if (players[currentPlayer].Angle > MathHelper.PiOver2)//araziye çarpması engellenir
                players[currentPlayer].Angle = -MathHelper.PiOver2;
            if (players[currentPlayer].Angle < -MathHelper.PiOver2)
                players[currentPlayer].Angle = MathHelper.PiOver2;
            if (keybState.IsKeyDown(Keys.Down))//topun gücü ayarlanır
                players[currentPlayer].Power -= 1;
            if (keybState.IsKeyDown(Keys.Up))
                players[currentPlayer].Power += 1;
            if (keybState.IsKeyDown(Keys.PageDown))
                players[currentPlayer].Power -= 20;
            if (keybState.IsKeyDown(Keys.PageUp))
                players[currentPlayer].Power += 20;
            if (players[currentPlayer].Power > 1000)//topun gücünü 0 ile 1000 arasında olmasını sağlar
                players[currentPlayer].Power = 1000;
            if (players[currentPlayer].Power < 0)
                players[currentPlayer].Power = 0;

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))//roketi fırlatmak için space ya da enter tuşuna basıldımı
            {
                rocketFlying = true;//roketin uçuyor olduğunu belli etmek için

                //launch.Play();//ses efektini çalar

                rocketPosition = players[currentPlayer].Position;
                rocketPosition.X += 20;
                rocketPosition.Y -= 10;
                rocketAngle = players[currentPlayer].Angle;
                Vector2 up = new Vector2(0, -1);
                Matrix rotMatrix = Matrix.CreateRotationZ(rocketAngle);//açıyı ayarlamak için
                rocketDirection = Vector2.Transform(up, rotMatrix);
                rocketDirection *= players[currentPlayer].Power / 50.0f;
            }
        }

        private void DrawText()//yazı çizimi için kullanılır
        {
            PlayerData player = players[currentPlayer];
            int currentAngle = (int)MathHelper.ToDegrees(player.Angle);
            spriteBatch.DrawString(font, "Cannon angle: " + currentAngle.ToString(), new Vector2(20, 20), player.Color);
            spriteBatch.DrawString(font, "Cannon power: " + player.Power.ToString(), new Vector2(20, 45), player.Color);
        }

        private void DrawRocket()//roketin çizimi için kullanılır
        {
            if (rocketFlying)
                spriteBatch.Draw(rocketTexture, rocketPosition, null, players[currentPlayer].Color, rocketAngle, new Vector2(42, 240), 0.1f, SpriteEffects.None, 1);
        }

        private void UpdateRocket()//roketin hareketi sağlanır
        {
            if (rocketFlying)
            {
                Vector2 gravity = new Vector2(0, 1);//yer çekimi eklenir
                rocketDirection += gravity / 10.0f;
                rocketPosition += rocketDirection;
                rocketAngle = (float)Math.Atan2(rocketDirection.X, -rocketDirection.Y);//roketin uçarken açısı belirlenir

                for (int i = 0; i < 5; i++)
                {
                    Vector2 smokePos = rocketPosition;//roketin dumanları
                    smokePos.X += randomizer.Next(10) - 5;
                    smokePos.Y += randomizer.Next(10) - 5;
                    smokeList.Add(smokePos);
                }
            }
        }

        private void DrawSmoke()//duman çizimi için kullanılır
        {
            foreach (Vector2 smokePos in smokeList)
                spriteBatch.Draw(smokeTexture, smokePos, null, Color.White, 0, new Vector2(40, 35), 0.2f, SpriteEffects.None, 1);
        }

        private void GenerateTerrainContour()//arazinin yamacını rastgele üretir
        {
            terrainContour = new int[screenWidth];
            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 3; 
            float offset = screenHeight / 2;
            float peakheight = 100;
            float flatness = 70;
            for (int x = 0; x < screenWidth; x++)
            {
                //double height = peakheight * Math.Sin((float)x / flatness)+offset;
                double height = peakheight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                height += peakheight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                height += peakheight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);
                height += offset;
                terrainContour[x] = (int)height;
            }
        }
        private void CreateForeground()//ön plan doku yaratır
        {
            Color[,] groundColors = TextureTo2DArray(groundTexture);//toprak deseni verilir

            Color[] foregroundColors = new Color[screenWidth * screenHeight];
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y > terrainContour[x])
                        //foregroundColors[x + y * screenWidth] = Color.Green;
                        //foregroundColors[x + y * screenWidth] = groundColors[x, y];
                        foregroundColors[x + y * screenWidth] = groundColors[x % groundTexture.Width, y % groundTexture.Height];
                    else
                        foregroundColors[x + y * screenWidth] = Color.Transparent;
                }
            }
            foregroundTexture = new Texture2D(device, screenWidth, screenHeight, false, SurfaceFormat.Color);
            foregroundTexture.SetData(foregroundColors);

            foregroundColorArray = TextureTo2DArray(foregroundTexture);//arkaplan resmi galiba 2 boyutlu resim olarak atanır
        }

        private void FlattenTerrainBelowPlayers()//top arabalarının arazi üzerindeki yerini açar
        {
            foreach (PlayerData player in players)
                if (player.IsAlive)
                    for (int x = 0; x < 40; x++)
                        terrainContour[(int)player.Position.X + x] = terrainContour[(int)player.Position.X];
        }

        private Color[,] TextureTo2DArray(Texture2D texture)//toprak resmini araziye vermek için
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];
            return colors2D;
        }

        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)//çarpışma algılama
        {
            Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
            int width1 = tex1.GetLength(0);
            int height1 = tex1.GetLength(1);
            int width2 = tex2.GetLength(0);
            int height2 = tex2.GetLength(1);
            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1to2);
                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    Vector2 screenPos = Vector2.Transform(pos1, mat1);
                                    return screenPos;
                                }
                            }
                        }
                    }
                }
            }
            return new Vector2(-1, -1);
        }

        private Vector2 CheckTerrainCollision()//roket ve arazi arasındaki çarpışmayı algılamak
        {
            Matrix rocketMat = Matrix.CreateTranslation(-42, -240, 0) * Matrix.CreateRotationZ(rocketAngle) * Matrix.CreateScale(rocketScaling) * Matrix.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0);
            Matrix terrainMat = Matrix.Identity;
            Vector2 terrainCollisionPoint = TexturesCollide(rocketColorArray, rocketMat, foregroundColorArray, terrainMat);
            return terrainCollisionPoint;
        }

        private Vector2 CheckPlayersCollision()//bilmiyorum
        {
            Matrix rocketMat = Matrix.CreateTranslation(-42, -240, 0) * Matrix.CreateRotationZ(rocketAngle) * Matrix.CreateScale(rocketScaling) * Matrix.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0);
            for (int i = 0; i < numberOfPlayers; i++)
            {
                PlayerData player = players[i];
                if (player.IsAlive)
                {
                    if (i != currentPlayer)
                    {
                        int xPos = (int)player.Position.X;
                        int yPos = (int)player.Position.Y;
                        Matrix carriageMat = Matrix.CreateTranslation(0, -carriageTexture.Height, 0) * Matrix.CreateScale(playerScaling) * Matrix.CreateTranslation(xPos, yPos, 0);
                        Vector2 carriageCollisionPoint = TexturesCollide(carriageColorArray, carriageMat, rocketColorArray, rocketMat);
                        if (carriageCollisionPoint.X > -1)
                        {
                            players[i].IsAlive = false;
                            return carriageCollisionPoint;
                        }
                        Matrix cannonMat = Matrix.CreateTranslation(-11, -50, 0) * Matrix.CreateRotationZ(player.Angle) * Matrix.CreateScale(playerScaling) * Matrix.CreateTranslation(xPos + 20, yPos - 10, 0);
                        Vector2 cannonCollisionPoint = TexturesCollide(cannonColorArray, cannonMat, rocketColorArray, rocketMat);
                        if (cannonCollisionPoint.X > -1)
                        {
                            players[i].IsAlive = false;
                            return cannonCollisionPoint;
                        }
                    }
                }
            }
            return new Vector2(-1, -1);
        }

        private bool CheckOutOfScreen()//roket pencerenin içinde olup olmadığını denetlemek için
        {
            bool rocketOutOfScreen = rocketPosition.Y > screenHeight;
            rocketOutOfScreen |= rocketPosition.X < 0;
            rocketOutOfScreen |= rocketPosition.X > screenWidth;
            return rocketOutOfScreen;
        }

        private void CheckCollisions(GameTime gameTime)//bütün olası çarpışmayı algılar
        {
            Vector2 terrainCollisionPoint = CheckTerrainCollision();
            Vector2 playerCollisionPoint = CheckPlayersCollision();
            bool rocketOutOfScreen = CheckOutOfScreen();
            if (playerCollisionPoint.X > -1)
            {

                AddExplosion(playerCollisionPoint, 10, 80.0f, 2000.0f, gameTime);//roket oyuncuya çarptı ve ateş partekül efekti ver

                rocketFlying = false;
                smokeList = new List<Vector2>();

//                hitCannon.Play();//ses efektini çalar

                NextPlayer();
            }
            if (terrainCollisionPoint.X > -1)
            {

                AddExplosion(terrainCollisionPoint, 4, 30.0f, 1000.0f, gameTime);//roket araziye çarptı ve ateş partekül efekti ver

                rocketFlying = false;
                smokeList = new List<Vector2>();

//                hitTerrain.Play();//ses efektini çalar

                NextPlayer();
            }
            if (rocketOutOfScreen)
            {
                rocketFlying = false;
                smokeList = new List<Vector2>();
                NextPlayer();
            }
        }

        private void NextPlayer()//bütün olası çarpışmayı algılama fonksiyonundan çağrılır
        {
            currentPlayer = currentPlayer + 1;
            currentPlayer = currentPlayer % numberOfPlayers;
            while (!players[currentPlayer].IsAlive)
                currentPlayer = ++currentPlayer % numberOfPlayers;
        }

        private void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, float maxAge, GameTime gameTime)//bir kaç ateş partekülü üretir
        {
            for (int i = 0; i < numberOfParticles; i++)
                AddExplosionParticle(explosionPos, size, maxAge, gameTime);

            float rotation = (float)randomizer.Next(10);//patlama kriterinde rastgele döndürmek için
            Matrix mat = Matrix.CreateTranslation(-explosionTexture.Width / 2, -explosionTexture.Height / 2, 0) * Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(size / (float)explosionTexture.Width * 2.0f) * Matrix.CreateTranslation(explosionPos.X, explosionPos.Y, 0);//patlama görüntüsü için matris oluşturulur
            AddCrater(explosionColorArray, mat);
            for (int i = 0; i < players.Length; i++)
                players[i].Position.Y = terrainContour[(int)players[i].Position.X];
            FlattenTerrainBelowPlayers();
            CreateForeground();
        }
        private void AddExplosionParticle(Vector2 explosionPos, float explosionSize, float maxAge, GameTime gameTime)//galiba ateş partikülünün gerçek yeri listesine eklenir
        {
            ParticleData particle = new ParticleData();
            particle.OrginalPosition = explosionPos;
            particle.Position = particle.OrginalPosition;
            particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            particle.MaxAge = maxAge;
            particle.Scaling = 0.25f;
            particle.ModColor = Color.White;
            float particleDistance = (float)randomizer.NextDouble() * explosionSize;//restgele ateş efekti verir
            Vector2 displacement = new Vector2(particleDistance, 0);
            float angle = MathHelper.ToRadians(randomizer.Next(360));//rastgele açı yön efekti verir
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            //particle.Direction = displacement;
            //particle.Accelaration = 3.0f * particle.Direction;
            particle.Direction = displacement * 2.0f;
            particle.Accelaration = -particle.Direction;            

            particleList.Add(particle);//ateş parçacıkları listeye eklenir   
        }
        private void DrawExplosion()//listeye eklenen ateş partikülleri çizilir
        {
            for (int i = 0; i < particleList.Count; i++)
            {
                ParticleData particle = particleList[i];
                spriteBatch.Draw(explosionTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        private void UpdateParticles(GameTime gameTime)//ateşe parçacıklarla efekt verir
        {
            float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = particleList.Count - 1; i >= 0; i--)
            {
                ParticleData particle = particleList[i];
                float timeAlive = now - particle.BirthTime;
                if (timeAlive > particle.MaxAge)
                {
                    particleList.RemoveAt(i);
                }
                else
                {//update current particle
                    float relAge = timeAlive / particle.MaxAge;
                    particle.Position = 0.5f * particle.Accelaration * relAge * relAge + particle.Direction * relAge + particle.OrginalPosition;
                    float invAge = 1.0f - relAge;
                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));
                    Vector2 positionFromCenter = particle.Position - particle.OrginalPosition;
                    float distance = positionFromCenter.Length();
                    particle.Scaling = (50.0f + distance) / 200.0f;
                    particleList[i] = particle;
                }
            }
        }

        private void AddCrater(Color[,] tex, Matrix mat)//patlama kriter eklemek için
        {
            int width = tex.GetLength(0);
            int height = tex.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tex[x, y].R > 10)
                    {
                        Vector2 imagePos = new Vector2(x, y);
                        Vector2 screenPos = Vector2.Transform(imagePos, mat);
                        int screenX = (int)screenPos.X;
                        int screenY = (int)screenPos.Y;
                        if ((screenX) > 0 && (screenX < screenWidth))
                            if (terrainContour[screenX] < screenY)
                                terrainContour[screenX] = screenY;
                    }
                }
            }
        }
    }
}
