//#define USE_KINECT // Comment out this line to test without a Kinect!!!

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
#if USE_KINECT
        KinectSensor kinect;
        Skeleton[] skeletonData;
        Skeleton skeleton;
        Boolean debugging = true;
        float previousHeadZ = 0;
        int headZCounter = 0;
#endif
        // Basic XNA 3d variables
        GraphicsDeviceManager graphics;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.5f, -1.35f);
        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 18;

        // Dabke dancer model variables
        Model myModel;
        const int numberOfAnimationPlayers = 6;
        const float spaceBetweenAnimationPlayers = 4.4f;//4.5f;
        const float initialStartingPosition = -11.0f;
        AnimationPlayer[] animationPlayers;
        Matrix[] animationPlayersOffsets;
        AnimationClip[] clips;

        // FSM variables
        SpriteBatch spriteBatch;
        Texture2D jointTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Timer delay = new Timer();
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5), new Vector2(355, 5) };
        bool bSpaceKeyPressed = false;

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

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 720;
            graphics.PreferredBackBufferHeight = 480;
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
                kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);
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
            myModel = Content.Load<Model>("Models\\man");

            // Our 3D models are skinned... notify if there's an error.  
            SkinningData skinningData = myModel.Tag as SkinningData;
            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // "Take_001" is the basic dance animation.  More (better-named) clips will also be loaded here.
            clips = new AnimationClip[3];
            clips[0] = skinningData.AnimationClips["Take_001"];
            clips[1] = skinningData.AnimationClips["Take_002"];
            clips[2] = skinningData.AnimationClips["Take_003"];
            //AnimationClip clip = skinningData.AnimationClips["Take_001"];


            
            animationPlayers = new AnimationPlayer[numberOfAnimationPlayers];
            animationPlayersOffsets = new Matrix[numberOfAnimationPlayers];
            for (int i = 0; i < numberOfAnimationPlayers; i++)
            {
                // Create an animation player, and start decoding an animation clip.
                animationPlayers[i] = new AnimationPlayer(skinningData);

                // Animate all three dancers.  TODO: Have slightly varying dances to look more genuine.
                //animationPlayers[i].StartClip(clip);
                animationPlayers[i].StartClip(clips[i%3]);

                //animationPlayersOffsets[i] = new Matrix();
                float offsetX = initialStartingPosition + (spaceBetweenAnimationPlayers * (float)i);
                animationPlayersOffsets[i] = Matrix.CreateTranslation(new Vector3(offsetX, 0.0f, 0.0f));
                
            }

            // Create and load all the dance step icons
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");

            song = Content.Load<Song>("Music\\dabke");
            //MediaPlayer.Play(song);

            video = Content.Load<Video>("Video\\tempIntro");
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                Keyboard.GetState().IsKeyDown(Keys.Q))
                this.Exit();

            // Press spacebar to advance your current dance steps without actually dancing.
            // Helpful for testing, making videos, ect...
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!bSpaceKeyPressed)
                {
                    if (videoPlayer.State != MediaState.Stopped)
                    {
                        videoPlayer.Stop();
                    }
                    else
                    {
                        bSpaceKeyPressed = true;
                        currentDabke++;
                        if (currentDabke > DabkeSteps.WaitForReset)
                            currentDabke = 0;
                    }
                }
            }
            else
                bSpaceKeyPressed = false;

            if (videoPlayer.State == MediaState.Stopped)
            {
                if (MediaPlayer.State != MediaState.Playing)
                {
                    MediaPlayer.Play(song);
                }

                // Place and update 3d models...
                for (int i = 0; i < numberOfAnimationPlayers; i++)
                {
                    TimeSpan temp = new TimeSpan(0, 0, 0, 0, (int)(gameTime.ElapsedGameTime.Milliseconds * 0.94)); //.8857
                    //animationPlayers[i].Update(gameTime.ElapsedGameTime, true, animationPlayersOffsets[i]);
                    animationPlayers[i].Update(temp, true, animationPlayersOffsets[i]);
                    //animationPlayers[i].
                    //originalBoneTransform = testModel.Bones["Bip01_L_UpperArm"].Transform;
                }
            }

            base.Update(gameTime);  // Base XNA update...
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Matrix[] bones;
            // Default XNA draw stuff...
            GraphicsDevice device = graphics.GraphicsDevice;
            device.Clear(Color.CornflowerBlue);


            if (videoPlayer.State != MediaState.Stopped)
            {
                spriteBatch.Begin();
                Texture2D texture = videoPlayer.GetTexture();
                if (texture != null)
                {
                    //spriteBatch.Draw(texture, new Rectangle(0, 0, 1280, 720),
                    spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480),
                        Color.White);
                }
                spriteBatch.End();
            }
            else
            {
                Vector3 partnerRightHand = new Vector3(0,0,0);

                for (int i = 0; i < numberOfAnimationPlayers; i++)
                {
                    // Grab 3d model skeleton matricies
                    bones = animationPlayers[i].GetSkinTransforms();

                    // Move the left hand of every dancer to the right hand of the previous
                    if(i!=0)
                        bones[11].Translation = partnerRightHand; //11 is left hand
                    partnerRightHand = bones[16].Translation + new Vector3(6.0f, -0.8f, 0.0f); // Right hand loc + Magic offset

                    // Compute camera matrices.
                    Matrix view = Matrix.CreateTranslation(cameraPosition) *
                                  Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                                  Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                                  Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                                      new Vector3(0, 0, 0), Vector3.Up);

                    Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                            device.Viewport.AspectRatio,
                                                                            1,
                                                                            10000);

                    // Render the skinned mesh.
                    foreach (ModelMesh mesh in myModel.Meshes)
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

                // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect 
                //DrawSkeleton(spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), jointTexture);

                // Draw all the dance step icons.  Yellow if step has been performed.  White, otherwise.
                spriteBatch.Begin();
                spriteBatch.Draw(StepCrossOFF, buttonPositions[0], null, (currentDabke > DabkeSteps.Home1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.Draw(StepHomeOFF, buttonPositions[1], null, (currentDabke > DabkeSteps.Crossover1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.Draw(StepCrossOFF, buttonPositions[2], null, (currentDabke > DabkeSteps.Home2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.Draw(StepHomeOFF, buttonPositions[3], null, (currentDabke > DabkeSteps.Crossover2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.Draw(StepKickOFF, buttonPositions[4], null, (currentDabke > DabkeSteps.Home3) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.Draw(StepHomeOFF, buttonPositions[5], null, (currentDabke > DabkeSteps.Kick) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                spriteBatch.End();
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

        // Reset the FSM to start
        void timer_Reset(object sender, EventArgs e)
        {
            currentDabke = DabkeSteps.Home1;
        }

#if USE_KINECT
        // This runs every time Kinect has an updated image
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
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

                        // Kinda cheating here... Keeping track of the head movement instead of relying on 
                        // the Kinect tracking crossed legs.  So, yes, you can cheat by bobbing up and down...
                        if (previousHeadZ > skeleton.Joints[JointType.Head].Position.Y)
                        {
                            // Head was moving down, but now is moving up.
                            if (headZCounter > 0)
                                headZCounter = -1;
                            else
                                headZCounter--;  // Head is still moving down.
                        }
                        else
                        {
                            // Head was moving up, but now is moving down.
                            if (headZCounter < 0)
                                headZCounter = 1;
                            else
                                headZCounter++; // Head is still moving up.
                        }

                        // Old, nasty code to advance horrible dance FSM.
                        switch (currentDabke)
                        {
                            case DabkeSteps.Home1:
                                //if (Math.Abs(skeleton.Joints[JointType.FootRight].Position.X - skeleton.Joints[JointType.FootLeft].Position.X) < .025)
                                //if (Math.Abs(skeleton.Joints[JointType.FootLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z) > .075)
                                if (headZCounter < -10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover1:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headZCounter > 10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Home2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X + .05f) < skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) > 0.075)
                                if (headZCounter < -5)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headZCounter > 5)
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
                                //delay.Tick += new EventHandler(timer_Reset); // Everytime timer ticks, timer_Tick will be called
                                //delay.Interval = (1000) * (1);             // Timer will tick evert 10 seconds
                                //delay.Enabled = true;                       // Enable the timer
                                //delay.Start();                              // Start the timer

                                break;
                        }

                        previousHeadZ = skeleton.Joints[JointType.Head].Position.Y;
                    }
                }
            }
        }
#endif
    }
}