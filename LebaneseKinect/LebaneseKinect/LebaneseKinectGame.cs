#define USE_KINECT // Comment out this line to test without a Kinect!!!

using System;
using System.Diagnostics; 
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;
using System.Timers;

#if USE_KINECT
using Microsoft.Kinect;
#endif


namespace LebaneseKinect
{
    /// <summary>
    /// Lebanese Kinect Game - "Being"
    /// This game teaches the user a simple traditional Arab dance called dabke.
    /// Currently only displays and rotates player model
    /// </summary>
    public class LebaneseKinectGame : Microsoft.Xna.Framework.Game
    {
        // Declaring variables...
        bool bWantsToQuit = false;
#if USE_KINECT
        KinectSensor kinect;
        Skeleton[] skeletonData;
        Skeleton skeleton;
        Boolean debugging = true;

        // Tracking direction changes in head and feet
        float previousHeadY = 0;
        float previousLeftFootZ = 0;
        float previousRightFootZ = 0;
        int headYCounter = 0;
        int leftFootZCounter = 0;
        int rightFootZCounter = 0;

#endif
        const int WINDOW_WIDTH = 640;//720;
        const int WINDOW_HEIGHT = 480;

        // Basic XNA 3d variables
        GraphicsDeviceManager graphics;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.5f, -1.35f);
        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 18;

        // Dabke dancer model variables
        Model modelMan, modelWoman;
        const int numberOfAnimationPlayers = 6;
        const float spaceBetweenAnimationPlayers = 4.4f;//4.5f;
        const float initialStartingPosition = -11.0f;
        AnimationPlayer[] animationPlayers;
        Matrix[] animationPlayersOffsets;
        AnimationClip[] clips;

        RenderTarget2D backBuffer;
        Effect kinectDepthVisualizer;
        Texture2D depthTexture;
        short[] depthData;
        bool needToRedrawBackBuffer;
        Rectangle shadowRect = new Rectangle(0, 0, 64, 64);

        // FSM variables
        SpriteBatch spriteBatch; //Draws all 2d images and text
        Texture2D jointTexture, shadowTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Stopwatch videoTime = new Stopwatch();
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5), new Vector2(355, 5) }; //currently unused
        
        /* Keyboard controls
         *  Up arrow: kick
         *  Down arrow: home
         *  Left/Right Arrow: cross
         */
        
        bool bSpaceKeyPressed = false;
        bool bCrossoverKeyPressed = false;
        bool bHomeKeyPressed = false;
        bool bKickKeyPressed = false;
        bool bDebugKeyPressed = false; //Display text, default is true
        bool bShowDebugText = true;

        /* Calculates the difference between the time at which it detects a user's action, and when the dancing animation does the same action.
         * TimeSpan(0, 0, 0, X, Y), X is seconds, Y is milliseconds.
         */
        //Expected dance move times;
        TimeSpan songDuration = new TimeSpan(0, 3, 6); // Song lasts 3 min, 6 sec. NEEDS TO RESET THE PROGRAM AT THE END.
        TimeSpan stopRepeatingDance = new TimeSpan(0, 3, 2); // Stop repeating dance at 3 min, 2 sec
        TimeSpan textFadeOut = new TimeSpan(0, 0, 0); // 2-second fadeout for result text
        TimeSpan crossStepTime1 = new TimeSpan(0,0,0,0,600);
        TimeSpan HomeTime1 = new TimeSpan(0, 0, 0, 1, 150);
        TimeSpan crossStepTime2 = new TimeSpan(0, 0, 0, 1, 700);
        TimeSpan HomeTime2 = new TimeSpan(0, 0, 0, 2, 250);
        TimeSpan KickTime = new TimeSpan(0, 0, 0, 2, 700);
        TimeSpan HomeTime3 = new TimeSpan(0, 0, 0, 3, 200);

        //First Male move
        TimeSpan LeftKneeLift1 = new TimeSpan(0,0,0,5,170);
        TimeSpan RightKneeLift1 = new TimeSpan(0,0,0,6,320);
        TimeSpan LeftKneeLift2 = new TimeSpan(0, 0, 0, 7, 570);
        TimeSpan RightKneeLift2 = new TimeSpan(0, 0, 0, 8, 450);

        SpriteFont font;
        SpriteFont resultFont;
        Color resultColor;
        string resultString = " ";
        string displayScoreText = " ";
        string displayRecentScoreText = " ";
        int totalScore = 0;
        int setScore = 0;
        int displayScore = 0;
        int cross1Score = 0;
        int cross2Score = 0;
        int kickScore = 0;
        int tempScore = 0;
        double previousDanceAnimationTimeMS = 0;

        Texture2D backgroundDabke;
        Rectangle backgroundRect = new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        List<Rectangle> shadowRects = new List<Rectangle>();

        List<string> eventsTriggeredList;

        // FSM for important keyframes in the dance
        enum DabkeSteps
        {
            Home1,
            Crossover1,
            Home2,
            Crossover2,
            Home3,
            Kick,
            WaitForReset
        };
        DabkeSteps currentDabke = DabkeSteps.Home1;

        protected Song song;
        VideoPlayer videoPlayer;
        Video video;
        Video video1;
        bool introPlaying = true;

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            eventsTriggeredList = new List<string>();
            for (int i = 0; i < numberOfAnimationPlayers; i++)
                shadowRects.Add(new Rectangle(0,0,64,64));
        }

        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if USE_KINECT
            kinect = KinectSensor.KinectSensors[0];
            kinect.Start();
#endif

            base.Initialize();  // Base XNA init...

#if USE_KINECT
            try
            {
                // Init Kinect for skeletal tracking
                kinect = KinectSensor.KinectSensors[0];
                kinect.Start();
                Debug.WriteLineIf(debugging, kinect.Status);
                kinect.SkeletonStream.Enable();
                kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                //kinect.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);
                kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinect_DepthFrameReady);
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_AllFramesReady);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            kinect.ElevationAngle = 0;
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load our 3D dancer model(s)
            modelMan = Content.Load<Model>("Models\\man");
            modelWoman = Content.Load<Model>("Models\\woman");

            // Our 3D models are skinned... notify if there's an error.  
            SkinningData skinningDataMale = modelMan.Tag as SkinningData;
            if (skinningDataMale == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            SkinningData skinningDataFemale = modelWoman.Tag as SkinningData;
            if (skinningDataFemale == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // "Take_001" is the basic dance animation.  More (better-named) clips will also be loaded here.
            clips = new AnimationClip[6];
            clips[0] = skinningDataMale.AnimationClips["Take_001"];
            clips[1] = skinningDataFemale.AnimationClips["Take_001"];
            clips[2] = skinningDataMale.AnimationClips["Take_001"];
            clips[3] = skinningDataFemale.AnimationClips["Take_001"];
            clips[4] = skinningDataMale.AnimationClips["Take_001"];
            clips[5] = skinningDataFemale.AnimationClips["Take_001"];
            //AnimationClip clip = skinningData.AnimationClips["Take_001"];

            font = Content.Load<SpriteFont>("myFont");
            resultFont = Content.Load<SpriteFont>("resultFont");
            
            animationPlayers = new AnimationPlayer[numberOfAnimationPlayers];
            animationPlayersOffsets = new Matrix[numberOfAnimationPlayers];
            for (int i = 0; i < numberOfAnimationPlayers; i++)
            {
                if (i % 2 == 0)
                {
                    animationPlayers[i] = new AnimationPlayer(skinningDataMale);
                }
                else
                {
                    animationPlayers[i] = new AnimationPlayer(skinningDataFemale);
                }

                animationPlayers[i].StartClip(clips[i]);


                // Create an animation player, and start decoding an animation clip.
                //animationPlayers[i] = (i % 2 == 0) ? new AnimationPlayer(skinningDataMale) : new AnimationPlayer(skinningDataFemale);

                // Animate all three dancers.  TODO: Have slightly varying dances to look more genuine.
                //animationPlayers[i].StartClip(clip);
                //animationPlayers[i].StartClip(clips[i%3]);
                //animationPlayers[i].StartClip(clips[i % 6]);
                //animationPlayers[i].CurrentClip.Keyframes[0].

                //animationPlayersOffsets[i] = new Matrix();
                float offsetX = initialStartingPosition + (spaceBetweenAnimationPlayers * (float)i);
                animationPlayersOffsets[i] = Matrix.CreateTranslation(new Vector3(offsetX, 0.0f, 0.0f));
                
            }

            kinectDepthVisualizer = Content.Load<Effect>("KinectDepthVisualizer");

            // Create and load all the dance step icons
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            shadowTexture = Content.Load<Texture2D>("Textures\\shadow");
            backgroundDabke = Content.Load<Texture2D>("Textures\\backgroundDabke");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");

            song = Content.Load<Song>("Music\\dabke");
            //MediaPlayer.Play(song);

            video = Content.Load<Video>("Video\\tempIntro");
            video1 = Content.Load<Video>("Video\\Lebanon"); 
            videoPlayer = new VideoPlayer();
            videoPlayer.Play(video);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (bWantsToQuit)
                Exit();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                bWantsToQuit = true;
#if USE_KINECT
                kinect.Dispose();
#endif
            }

            // Press spacebar to advance your current dance steps without actually dancing.
            // Helpful for testing, making videos, ect...
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!bSpaceKeyPressed)
                {
                    if (introPlaying)
                    {
                        // SPACEBAR stops the intro video, if it is playing...
                        videoPlayer.Dispose();
                        videoPlayer = new VideoPlayer();
                        videoPlayer.Play(video1);
                        introPlaying = false;
                        videoTime.Start();    
                    }
                    else
                    {
                        // Otherwise, SPACEBAR is our developer shortcut for telling the game we've completed a dance step
                        bSpaceKeyPressed = true;
                        currentDabke++;
                        if (currentDabke > DabkeSteps.WaitForReset)
                            currentDabke = 0;
                    }
                }
            }
            else
                bSpaceKeyPressed = false;

            if (!introPlaying)
            {

                // Place and update 3d models...
                for (int i = 0; i < numberOfAnimationPlayers; i++)
                {
                    // Change the speed of the model's animation to match the music (using magic number)
                    TimeSpan temp = new TimeSpan(0, 0, 0, 0, (int)(gameTime.ElapsedGameTime.Milliseconds * 0.9375)); //.8857 //0.94
                    animationPlayers[i].Update(temp, true, animationPlayersOffsets[i]);
                }


                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    if (!bCrossoverKeyPressed)
                    {
                        bCrossoverKeyPressed = true;
                        CrossoverTriggered();
                    }
                }
                else
                {
                    bCrossoverKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    if (!bHomeKeyPressed)
                    {
                        bHomeKeyPressed = true;
                        HomeTriggered();
                    }
                }
                else
                {
                    bHomeKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    if (!bKickKeyPressed)
                    {
                        bKickKeyPressed = true;
                        KickTriggered();
                    }
                }
                else
                {
                    bKickKeyPressed = false;
                }

                // Check if animation looped
                double halfTime = animationPlayers[0].CurrentClip.Duration.TotalMilliseconds / 2.0;
                if (animationPlayers[0].CurrentTime.TotalMilliseconds < previousDanceAnimationTimeMS ||
                    (previousDanceAnimationTimeMS < halfTime && animationPlayers[0].CurrentTime.TotalMilliseconds > halfTime))
                {
                    danceAnimationEnd();
                }
                previousDanceAnimationTimeMS = animationPlayers[0].CurrentTime.TotalMilliseconds;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (!bDebugKeyPressed)
                {
                    bDebugKeyPressed = true;
                    bShowDebugText = !bShowDebugText;
                }
            }
            else
            {
                bDebugKeyPressed = false;
            }

            if (totalScore > displayScore)
                displayScore += 11;

            if (totalScore < displayScore)
                displayScore = totalScore;

            base.Update(gameTime);  // Base XNA update...
        }

        private void danceAnimationEnd()
        {
            //setScore = cross1Score + cross2Score + kickScore;
            setScore = tempScore;
            tempScore = 0;
            if (setScore > 1500)
            {
                resultColor = Color.Green;
                resultString = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > 1000)
            {
                resultColor = Color.Yellow;
                resultString = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > 400)
            {
                resultColor = Color.Red;
                resultString = "KEEP TRYING";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }

            if (setScore > 400)
            {
                displayRecentScoreText = "+" + setScore.ToString();
                totalScore += setScore;
            }
            else
            {
                displayRecentScoreText = " ";
                setScore = 0;
            }
            cross1Score = 0;
            cross2Score = 0;
            kickScore = 0;
        }

        private void LeftKneeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLift1).TotalMilliseconds);
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLift2).TotalMilliseconds);

            if(diff1 < 600)
            {
                tempScore +=Math.Max((int)(600 - diff1), 0);
            }

            else if (diff2 < 600)
            {
                tempScore +=Math.Max((int)(600 - diff2), 0);
            }
            tempScore *= 1000000;
        }

        private void CrossoverTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(crossStepTime1)).TotalMilliseconds);
            double diff2 = Math.Abs((currentTime.Subtract(crossStepTime2)).TotalMilliseconds);

            double halfTime = animationPlayers[0].CurrentClip.Duration.TotalMilliseconds / 2.0;
            if (currentTime.TotalMilliseconds > halfTime)
            {
                diff1 = Math.Abs(diff1 - halfTime);
                diff2 = Math.Abs(diff2 - halfTime);
            }

            if(diff1 < diff2)
                cross1Score = Math.Max((int)(600 - diff1), 0);
            else
                cross2Score = Math.Max((int)(600 - diff2), 0);

            AddEventTriggeredText("CrossoverTriggered: " + cross1Score + " ... " + cross2Score);
        }

        private void HomeTriggered()
        {
            //double diff1 = Math.Abs((animationPlayers[0].CurrentTime.Subtract(HomeTime1)).TotalMilliseconds);
            //double diff2 = Math.Abs((animationPlayers[0].CurrentTime.Subtract(HomeTime2)).TotalMilliseconds);
            //score = Math.Max(500 - (int)((diff1 < diff2) ? diff1 : diff2), 0);

            AddEventTriggeredText("HomeTriggered");
        }

        private void AddEventTriggeredText(string val)
        {
            eventsTriggeredList.Add(val);
            if (eventsTriggeredList.Count > 10)
                eventsTriggeredList.Remove(eventsTriggeredList[0]);
        }

        /*Calculates difference between player action time and dancer's action time.
         */
        private void KickTriggered()
        {
            double diff1 = Math.Abs((animationPlayers[0].CurrentTime.Subtract(KickTime)).TotalMilliseconds);


            TimeSpan currentTime = animationPlayers[0].CurrentTime;
            double halfTime = animationPlayers[0].CurrentClip.Duration.TotalMilliseconds / 2.0;
            if (currentTime.TotalMilliseconds > halfTime)
            {
                diff1 = Math.Abs(diff1 - halfTime);
            }


            kickScore = Math.Max((int)(600 - diff1), 0);

            AddEventTriggeredText("KickTriggered: " + kickScore);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            // Default XNA draw stuff...
            GraphicsDevice.Clear(Color.White);
            
            //device.Clear(Color.CornflowerBlue);

            // Intro video is playing...
            if (introPlaying)
            {
                spriteBatch.Begin();
                Texture2D texture = videoPlayer.GetTexture();
                if (texture != null)
                {
                    // Draw intro video
                    //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                }
                spriteBatch.End();
            }
            else // Intro video is no longer playing...
            {


                // Draw all the dance step icons.  Yellow if step has been performed.  White, otherwise.
                //spriteBatch.Begin();
                //spriteBatch.Draw(StepCrossOFF, buttonPositions[0], null, (currentDabke > DabkeSteps.Home1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[1], null, (currentDabke > DabkeSteps.Crossover1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepCrossOFF, buttonPositions[2], null, (currentDabke > DabkeSteps.Home2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[3], null, (currentDabke > DabkeSteps.Crossover2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepKickOFF, buttonPositions[4], null, (currentDabke > DabkeSteps.Home3) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[5], null, (currentDabke > DabkeSteps.Kick) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.End();




                // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect 
                //DrawSkeleton(spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), jointTexture);

                //spriteBatch.Begin();

                //spriteBatch.End();


#if USE_KINECT
                // If we don't have a depth target, exit
                if (this.depthTexture == null)
                {
                    return;
                }
#else
                this.needToRedrawBackBuffer = true;
#endif

                //if (depthTexture != null && needToRedrawBackBuffer)
                if (this.needToRedrawBackBuffer)
                {
                    GraphicsDevice.SetRenderTarget(backBuffer);
                    //GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
#if USE_KINECT
                    depthTexture.SetData<short>(depthData);
                    SharedSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, kinectDepthVisualizer);
#endif
                    spriteBatch.Begin();
              
                    Texture2D texture = videoPlayer.GetTexture();
                    if (texture != null)
                    {
                        // Draw intro video
                        //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                        spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    }
                    spriteBatch.End();

                    /*spriteBatch.Begin();
                    for(int i=0; i<numberOfAnimationPlayers; i++)
                    {
                        spriteBatch.Draw(shadowTexture, shadowRects[i], Color.White);
                    }
                    spriteBatch.End();
#if USE_KINECT
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, kinectDepthVisualizer);
#endif
                   DrawModels();*/
#if USE_KINECT
                    SharedSpriteBatch.Draw(depthTexture, Vector2.Zero, Color.White);
#endif
                    DrawText();
                    textFadeOut = textFadeOut.Subtract(gameTime.ElapsedGameTime);
#if USE_KINECT
                    SharedSpriteBatch.End();
#endif
                    GraphicsDevice.SetRenderTarget(null);
                    needToRedrawBackBuffer = false;
                }
#if USE_KINECT
                SharedSpriteBatch.Begin();
                SharedSpriteBatch.Draw(
                    this.backBuffer,
                    new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT),
                    null,
                    Color.White);
                SharedSpriteBatch.End();
#endif
            }

            base.Draw(gameTime); // Draw base XNA stuff...
        }

        private void DrawSkeleton(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img)
        {
#if USE_KINECT
            // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect
            if (skeleton != null)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Red);
                }
            }
#endif
        }

        private void DrawDebugString(SpriteFont fontVal, Color colorVal, int x, int y, string stringVal)
        {
            Color textShadowColor = Color.Black;
            textShadowColor.A = colorVal.A;
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x + 2, y + 2), textShadowColor);
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x, y), colorVal);
        }

        private void DrawModels()
        {
            Matrix[] bones;
            Vector3 partnerRightHand = new Vector3(0, 0, 0);

            for (int i = 0; i < numberOfAnimationPlayers; i++)
            {
                // Grab 3d model skeleton matricies
                bones = animationPlayers[i].GetSkinTransforms();

                // Move the left hand of every dancer to the right hand of the previous
                if (i != 0)
                    bones[12].Translation = partnerRightHand; //11 is left hand
                partnerRightHand = bones[16].Translation + new Vector3(6.0f, -0.8f, 0.0f); // Right hand loc + Magic offset


                // Compute camera matrices.
                Matrix view = Matrix.CreateTranslation(cameraPosition) *
                              Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                              Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                              Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                                  new Vector3(0, 0, 0), Vector3.Up);

                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                        GraphicsDevice.Viewport.AspectRatio,
                                                                        1,
                                                                        10000);

                Matrix world = Matrix.Identity;
                Vector3 shadowVect = GraphicsDevice.Viewport.Project(bones[3].Translation, projection, view, world);
                shadowRects[i] = new Rectangle((int)(shadowVect.X*1.6)-120, 435, 200, 34);


                ModelMeshCollection tempMeshes = (i % 2 == 0) ? modelMan.Meshes : modelWoman.Meshes;
                // Render the skinned mesh.
                foreach (ModelMesh mesh in tempMeshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.SetBoneTransforms(bones);

                        effect.View = view;
                        effect.Projection = projection;

                        effect.EnableDefaultLighting();

                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }

                    mesh.Draw();
                }
            }
        }

        private void DrawText()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            if (bShowDebugText)
            {
                displayScoreText = String.Format("{0,5}", displayScore);
                //Vector2 scoreTextSize = resultFont.MeasureString(displayScoreText);
                //DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreTextSize.X), 10, displayScoreText);
                Vector2 scoreSize = font.MeasureString(displayScoreText);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreSize.X - 10), 10, displayScoreText);
                //DrawDebugString(font, Color.White, 20, 400, "Animation Time: " + animationPlayers[0].CurrentTime.Seconds.ToString() + "." + animationPlayers[0].CurrentTime.Milliseconds.ToString() + "s");

                //for (int i = 0; i < eventsTriggeredList.Count; i++)
                //    DrawDebugString(font, Color.White, 10, 10 + (i * 20), eventsTriggeredList[i]);
            }

            double textFadeOutAmt = textFadeOut.TotalMilliseconds / 2000.0;

            resultColor.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
            Vector2 resultSize = resultFont.MeasureString(resultString);
            DrawDebugString(resultFont, resultColor, (int)(WINDOW_WIDTH-resultSize.X)/2, 10, resultString);

            Color recentScoreColor = Color.White;
            recentScoreColor.A = (byte)Math.Max(255 * Math.Pow(textFadeOutAmt, 10), 0);
            Vector2 tempScoreSize = font.MeasureString(displayRecentScoreText);
            DrawDebugString(font, recentScoreColor, (int)(WINDOW_WIDTH - tempScoreSize.X - 10), 30, displayRecentScoreText);
            

            spriteBatch.End();
            //GraphicsDevice.BlendState = DefaultBlendState;
        }

        // Reset the FSM to start
        void timer_Reset(object sender, EventArgs e)
        {
            currentDabke = DabkeSteps.Home1;
        }

#if USE_KINECT
        void kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            //throw new NotImplementedException();
            //DepthImageFrame frame = e.OpenDepthImageFrame();
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                if (frame != null)
                {
                    if (null == depthData || depthData.Length != frame.PixelDataLength)
                    {
                        depthData = new short[frame.PixelDataLength];

                        depthTexture = new Texture2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Bgra4444);

                        backBuffer = new RenderTarget2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Color,
                            DepthFormat.None,
                            GraphicsDevice.PresentationParameters.MultiSampleCount,
                            RenderTargetUsage.PreserveContents);

                        spriteBatch = new SpriteBatch(GraphicsDevice);
                    }


                    frame.CopyPixelDataTo(depthData);
                    needToRedrawBackBuffer = true;
                }
            }
        }

        // This runs every time Kinect has an updated image
        void kinect_AllFramesReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            //throw new NotImplementedException();

            // Store all current Kinect skeletal info...
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    //Copy the skeleton data to our array
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }

            // If there is valid skelton data, examine for dance steps
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;
                        int headCounterTrigger = 0;
                        int leftFootCounterTrigger = 0;
                        int rightFootCounterTrigger = 0;

                        #region HeadAndFootDirection
                        // Kinda cheating here... Keeping track of the head movement instead of relying on 
                        // the Kinect tracking crossed legs.  So, yes, you can cheat by bobbing up and down...
                        if (previousHeadY > skeleton.Joints[JointType.Head].Position.Y)
                        {
                            // Head was moving down, but now is moving up.
                            if (headYCounter > 0)
                            {
                                headCounterTrigger = headYCounter; // Store this number and see if it triggers a move
                                headYCounter = -1;
                            }
                            else
                                headYCounter--;  // Head is still moving down.
                        }
                        else
                        {
                            // Head was moving up, but now is moving down.
                            if (headYCounter < 0)
                            {
                                headCounterTrigger = headYCounter; // Store this number and see if it triggers a move
                                headYCounter = 1;
                            }
                            else
                                headYCounter++; // Head is still moving up.
                        }

                        // Do the same thing for Left Foot
                        if (previousLeftFootZ > skeleton.Joints[JointType.FootLeft].Position.Z)
                        {
                            // Left foot was moving away from Kinect, but is now mocing towards Kinect.
                            if (leftFootZCounter > 0)
                            {
                                leftFootCounterTrigger = leftFootZCounter; // Store this number and see if it triggers a move
                                leftFootZCounter = -1;
                            }
                            else
                                leftFootZCounter--;  // Left foot still moving away from Kinect
                        }
                        else
                        {
                            // Left foot was moving towards Kinect, now is moving away from it.
                            if (leftFootZCounter < 0)
                            {
                                leftFootCounterTrigger = leftFootZCounter; // Store this number and see if it triggers a move
                                leftFootZCounter = 1;
                            }
                            else
                                leftFootZCounter++; // Left foot is still away.
                        }

                        if (previousRightFootZ > skeleton.Joints[JointType.FootRight].Position.Z)
                        {
                            // Head was moving down, but now is moving up.
                            if (rightFootZCounter > 0)
                            {
                                rightFootCounterTrigger = rightFootZCounter; // Store this number and see if it triggers a move
                                rightFootZCounter = -1;
                            }
                            else
                                rightFootZCounter--;  // Head is still moving down.
                        }
                        else
                        {
                            // Head was moving up, but now is moving down.
                            if (rightFootZCounter < 0)
                            {
                                rightFootCounterTrigger = rightFootZCounter; // Store this number and see if it triggers a move
                                rightFootZCounter = 1;
                            }
                            else
                                rightFootZCounter++; // Head is still moving up.
                        }
                        #endregion

                        if (headYCounter < -10)
                            CrossoverTriggered();
                        if( (skeleton.Joints[JointType.KneeLeft].Position.Y - skeleton.Joints[JointType.KneeRight].Position.Y) > 30
                            && Math.Abs(skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.KneeRight].Position.X) > 15)
                            LeftKneeTriggered();
                        if (headYCounter > 10)
                            HomeTriggered();
                        if (rightFootCounterTrigger  < -5 || leftFootCounterTrigger < -5)
                            KickTriggered();
                        //if ((skeleton.Joints[JointType.FootLeft].Position.Z + .2f) < skeleton.Joints[JointType.KneeLeft].Position.Z)
                        //    KickTriggered();

                        // Old, nasty code to advance horrible dance FSM.
                        switch (currentDabke)
                        {
                            case DabkeSteps.Home1:
                                //if (Math.Abs(skeleton.Joints[JointType.FootRight].Position.X - skeleton.Joints[JointType.FootLeft].Position.X) < .025)
                                //if (Math.Abs(skeleton.Joints[JointType.FootLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z) > .075)
                                if (headYCounter < -10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover1:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headYCounter > 10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Home2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X + .05f) < skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) > 0.075)
                                if (headYCounter < -5)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headYCounter > 5)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Home3:
                                if ((skeleton.Joints[JointType.FootLeft].Position.Z + .2f) < skeleton.Joints[JointType.KneeLeft].Position.Z)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Kick:
                                if (Math.Abs(skeleton.Joints[JointType.FootLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z) < .05)
                                    currentDabke++;
                                break;

                            case DabkeSteps.WaitForReset:

                                currentDabke = DabkeSteps.Home1;
                                //videoTime.Tick += new EventHandler(timer_Reset); // Everytime timer ticks, timer_Tick will be called
                                //videoTime.Interval = (1000) * (1);             // Timer will tick every 10 seconds
                                //videoTime.Enabled = true;                       // Enable the timer
                                //videoTime.Start();                              // Start the timer

                                break;
                        }

                        previousHeadY = skeleton.Joints[JointType.Head].Position.Y;
                        previousLeftFootZ = skeleton.Joints[JointType.FootLeft].Position.Z;
                        previousRightFootZ = skeleton.Joints[JointType.FootRight].Position.Z;

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This method maps a SkeletonPoint to the depth frame.
        /// </summary>
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the depth frame.</returns>
        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            if ((null != kinect) && (null != kinect.DepthStream))
            {
                // This is used to map a skeleton point to the depth image location
                var depthPt = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(point, kinect.DepthStream.Format);
                return new Vector2(depthPt.X, depthPt.Y);
            }

            return Vector2.Zero;
        }
#endif
    }
}