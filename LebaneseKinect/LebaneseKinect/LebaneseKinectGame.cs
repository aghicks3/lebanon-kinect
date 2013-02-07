using System;
using System.Diagnostics; 
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;

namespace LebaneseKinect
{
    /// <summary>
    /// Lebanese Kinect Game - "Being"
    /// This game teaches the user a simple traditional Arab dance called dabke.
    /// Currently only displays and rotates player model
    /// </summary>
    public class LebaneseKinectGame : Microsoft.Xna.Framework.Game
    {
        Boolean debugging = true;
        GraphicsDeviceManager graphics;
        KinectSensor kinect;
        SpriteBatch spriteBatch;
        Model myModel;
        float aspectRatio;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.0f, -1.25f);
        AnimationPlayer animationPlayer;
        AnimationPlayer animationPlayer2;
        AnimationPlayer animationPlayer3;
        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 20;
        Skeleton[] skeletonData;
        Skeleton skeleton;
        Texture2D jointTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5) };
        enum DabkeSteps
        {
            Home1,
            Crossover1,
            Home2,
            Crossover2,
            Kick,
            Home3
        };
        DabkeSteps currentDabke = DabkeSteps.Home1;

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 600;
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
            kinect = KinectSensor.KinectSensors[0];
            kinect.Start();
            base.Initialize();

            try
            {
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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            myModel = Content.Load<Model>("Models\\man");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;


            // Look up our custom skinning information.
            SkinningData skinningData = myModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);
            animationPlayer2 = new AnimationPlayer(skinningData);
            animationPlayer3 = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take_001"];

            animationPlayer.StartClip(clip);
            animationPlayer2.StartClip(clip);
            animationPlayer3.StartClip(clip);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");
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
                Keyboard.GetState().IsKeyDown(Keys.Q) )
                this.Exit();

            //modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f);
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            Matrix offset2 = Matrix.CreateTranslation(new Vector3(6.5f, 0.0f, 0.0f));
            Matrix offset3 = Matrix.CreateTranslation(new Vector3(-6.5f, 0, 0));
            animationPlayer2.Update(gameTime.ElapsedGameTime, true, offset2);
            animationPlayer3.Update(gameTime.ElapsedGameTime, true, offset3);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            Matrix[] bones = animationPlayer.GetSkinTransforms();

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

            bones = animationPlayer2.GetSkinTransforms();

            // Render the skinned mesh.
            foreach (ModelMesh mesh2 in myModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh2.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh2.Draw();
            }


            bones = animationPlayer3.GetSkinTransforms();
            // Render the skinned mesh.
            foreach (ModelMesh mesh3 in myModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh3.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh3.Draw();
            }

            //DrawSkeleton(spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), jointTexture);
            spriteBatch.Draw(StepHomeOFF, buttonPositions[0], null, (currentDabke > DabkeSteps.Home1)?Color.Yellow:Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(StepCrossOFF, buttonPositions[1], null, (currentDabke > DabkeSteps.Crossover1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(StepHomeOFF, buttonPositions[2], null, (currentDabke > DabkeSteps.Home2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(StepCrossOFF, buttonPositions[3], null, (currentDabke > DabkeSteps.Crossover2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(StepKickOFF, buttonPositions[4], null, (currentDabke > DabkeSteps.Kick) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawSkeleton(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img)
        {
            if (skeleton != null)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Red);
                }
            }
        }

        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();

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

            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;

                        switch (currentDabke)
                        {
                            case DabkeSteps.Home1:
                                if ((skeleton.Joints[JointType.FootRight].Position.X + .1f) < skeleton.Joints[JointType.FootLeft].Position.X)
                                    currentDabke++;
                            break;

                            case DabkeSteps.Crossover1:
                            if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                currentDabke++;
                            break;

                            case DabkeSteps.Home2:
                            if ((skeleton.Joints[JointType.FootRight].Position.X + .05f) < skeleton.Joints[JointType.FootLeft].Position.X)
                                currentDabke++;
                            break;

                            case DabkeSteps.Crossover2:
                            if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                currentDabke++;
                            break;

                            case DabkeSteps.Kick:
                            if ((skeleton.Joints[JointType.FootLeft].Position.Z + .05f) < skeleton.Joints[JointType.KneeLeft].Position.Z)
                                currentDabke++;
                            break;

                            case DabkeSteps.Home3:
                            if ((skeleton.Joints[JointType.FootLeft].Position.Z - .01f) > skeleton.Joints[JointType.KneeLeft].Position.Z)
                                currentDabke = DabkeSteps.Home1;
                            break;
                        }
                    }
                }
            }
        }
    }
}
