using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Samples.Kinect.Avateering
{
    class VideoElement : WindowMedia
    {

        public string VideoElementType { get; set; }


        public Video video;
        public VideoPlayer player;
        public Vector3 scale;
        public Vector3 bb_scale;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        public bool videoWindowIsOpen = false;
        public bool prevoiusVideoWindowIsOpen;
      //public bool videoElementIsActive;
      //public bool videoElementIsOnFront;
        public bool BringToBackActivated =false;
        public bool BringToFrontActivated=false;
        public float time;
        //public float gestureScaleAccelerationFactor;
        public float gestureScaleIncrement;
        public bool gestureScaleDetected;

      //  private ContentManager Content;


         public VideoElement(Game game) : base(game)
        {
            VideoElementType = null;

        }

        public void LoadContent(ContentManager content, string modelName, string videoName)
        {
            Model = content.Load<Model>(modelName);
            VideoElementType = modelName;
            BoundingBox = new BoundingBox();
            //  scale = new Vector3();
            //   transformMatrix = new Matrix();
            //  bb_size_min = GetBoundingBoxSize(Model, transformMatrix).Min;
            //  bb_size_max = GetBoundingBoxSize(Model, transformMatrix).Max;

            Position = new Vector3(0, 12, 0);
            Rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1.0f, 1.0f, 1.0f);
            bb_size = new Vector3(16, 9, 0.02f);

            // Load a video, and initialize a player
            video = content.Load<Video>(videoName);
            player = new VideoPlayer();
            player.IsLooped = true;

            //videoElementIsActive = false;
            //videoElementIsOnFront = false;

            //gestureScaleAccelerationFactor = 1.0f;
            gestureScaleIncrement = 0.004f;

            timer = new Timer();
        }

        

        public override void Update(GameTime gameTime)
        {

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            // Update saved state.
            previousKeyboard = currentKeyboard;


            bb_scale = new Vector3(scale.X, scale.Y, scale.Z);

            if (videoWindowIsOpen)
            {
                BoundingBox.Min = Position - bb_scale * bb_size / 2;
                BoundingBox.Max = Position + bb_scale * bb_size / 2;
            }
            else
            {
                BoundingBox.Min = new Vector3(0, 0, -100f);
                BoundingBox.Max = new Vector3(0, 0, -100f);
            }

            UpdateInput(gameTime);

            CollideWindow();
            DetectDragGesture();

            prevoiusVideoWindowIsOpen = videoWindowIsOpen;

            if (BringToFrontActivated)
            {
                time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (Position.Z < AvateeringXNA.FrontPositionZ)
                {
                    Position.Z += time * 0.03f;
                }

                if (Position.Z >= AvateeringXNA.FrontPositionZ)
                {
                    Position.Z = AvateeringXNA.FrontPositionZ;
                    BringToFrontActivated = false;

                }
            }

            if (BringToBackActivated)
            {
                if (Position.Z > AvateeringXNA.BackPositionZ)
                {
                    Position.Z -= time * 0.07f;
                }
                else 
                if (Position.Z <= AvateeringXNA.FrontPositionZ)
                {
                    BringToBackActivated = false;
                    player.Stop();
                }
            }

            Collide();

            if (WindowIsActive)
            {
                DetectGesture(gameTime);
            }

        }

        public void Draw()
        {

            
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            translateMatrix = Matrix.CreateTranslation(Position);
                              //Matrix.CreateRotationX(Rotation.X)*Matrix.CreateRotationY(Rotation.Y)*
                             // Matrix.CreateRotationZ(Rotation.Z)*
                              
            Vector3 position = translateMatrix.Translation;
            translateMatrix *= Matrix.CreateFromAxisAngle(translateMatrix.Right, MathHelper.ToRadians(90));
            translateMatrix *= Matrix.CreateScale(scale);
            translateMatrix.Translation = position;
            Matrix modelMatrix = translateMatrix;

            

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (player.State == MediaState.Playing)
                        effect.Texture = player.GetTexture();

                    

                    effect.World = modelMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = AvateeringXNA.view;
                    effect.Projection = AvateeringXNA.projection;

    //                effect.TextureEnabled = true;

                  //  effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1); 
                  //  effect.DirectionalLight0.Direction = new Vector3(-0.5f, -0.5f, 0.5f);  
                  //  effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1);

                    if (WindowIsActive)
                    {
                       // effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                       // effect.DirectionalLight1.Direction = new Vector3(1, -2, -1);
                       // effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);

                        effect.EmissiveColor = current_hilite;
                       // effect.LightingEnabled = true;
                       // effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.Alpha = 1f;
                    }
                    else
                    {
                        effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0, 0);
                        effect.DirectionalLight1.Direction = new Vector3(1, 1, -1);
                        effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);

                        effect.EmissiveColor = new Vector3(0,0,0);
                        effect.Alpha = 0.5f;
                       // effect.PreferPerPixelLighting = true;
                    }
                }

                DrawBoundingBox();

                mesh.Draw();
            }
        }
        

        private void Collide()
        {
            collideResults1 = BoundingBox.Contains(AvateeringXNA.CursorRight.boundingBox);
            collideResults2 = BoundingBox.Contains(AvateeringXNA.CursorLeft.boundingBox);

            switch (collideResults1)
            {
                case ContainmentType.Contains:
                    // Window.Title = "Contains";
                    break;
                case ContainmentType.Disjoint:
                    // Window.Title = "Disjoint";
                    current_hilite = new Vector3(0, 0, 0);
                    break;
                case ContainmentType.Intersects:
                    {
                        //Window.Title = "Intersects";
                        current_hilite = new Vector3(0.1f, 0.1f, 0.1f);
                        if (AvateeringXNA.cursorVectorDirection.Z > 0.2f)
                        {
                            if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                (player.State == MediaState.Playing))
                            {
                                Pause();
                            }
                            else if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                     (player.State == MediaState.Paused))
                            {
                                Resume();
                            }
                        }

                        if (AvateeringXNA.cursorVectorDirection.Z > gestureCloseRightHandAccelFactor)
                        {
                            if ((previousCollideResult1 == ContainmentType.Disjoint) & (AvateeringXNA.leftCursorVectorDirection.Z < gestureCloseLeftHandAccelFactor))
                            {
                                CloseWindow();
                            }
                        }

                        //Закрытие окна - курсор на правый верхний угол
                        float diffX = Math.Abs(BoundingBox.Max.X - AvateeringXNA.CursorRight.boundingBox.Max.X);
                        float diffY = Math.Abs(BoundingBox.Max.Y - AvateeringXNA.CursorRight.boundingBox.Max.Y);
                        if ((AvateeringXNA.cursorVectorDirection.Z > 0.5f) & (diffY < 2f) & (diffX < 2f))
                        {
                            CloseWindow();
                        }

                    }
                    break;
                //  (vid_sample.player.State == MediaState.Playing)
                default:
                    break;
            }
            previousCollideResult1 = collideResults1;
        }


        public void BringToFront()
        {
            BringToFrontActivated = true;
            BringToBackActivated = false;
        }

        public void BringToBack()
        {
            BringToBackActivated = true;
            BringToFrontActivated = false;
            WindowIsActive = false;
            WindowIsOnFront = false;
        }

        public void StartPlaying()
        {
            StepBack();
            player.Play(video);
            videoWindowIsOpen = true;
            WindowIsActive = true;
            WindowIsOnFront = true;
            AvateeringXNA.AllMediaWindows.Add(this);
        }
        
        public void Pause()
        {
            player.Pause();
        }
        
        public void Resume()
        {
            player.Play(video);
            WindowIsActive = true;
            WindowIsOnFront = true;
        }
        
        public void CloseWindow()
        {
            player.Stop();
            WindowIsActive = false;
            WindowIsOnFront = false;
            videoWindowIsOpen = false;
            AvateeringXNA.AllMediaWindows.Remove(this);
            StepForward();
        }

        public void UpdateInput(GameTime gameTime)
        {
            currentKeyboard = Keyboard.GetState();
            
            if (currentKeyboard.IsKeyDown(Keys.Add) || currentKeyboard.IsKeyDown(Keys.OemPlus))
            {
                scale.X += time * 0.001f;    scale.Y += time * 0.001f;
              //  bb_size.X += time * 0.004f; bb_size.Y += time * 0.004f;
            }
            if (currentKeyboard.IsKeyDown(Keys.Subtract) || currentKeyboard.IsKeyDown(Keys.OemMinus))
            {
                scale.X -= time * 0.001f; scale.Y -= time * 0.001f;
              //  bb_size.X -= time * 0.004f; bb_size.Y -= time * 0.004f;
            }
            if (currentKeyboard.IsKeyDown(Keys.NumPad6))
            {
                Position.X += time * 0.01f; 
            }
            if (currentKeyboard.IsKeyDown(Keys.NumPad4))
            {
                Position.X -= time * 0.01f;
            }
            if (currentKeyboard.IsKeyDown(Keys.NumPad2))
            {
                Position.Y -= time * 0.01f;
            }
            if (currentKeyboard.IsKeyDown(Keys.NumPad8))
            {
                Position.Y += time * 0.01f;
            }
            if (currentKeyboard.IsKeyDown(Keys.Multiply))
            {
                Position.Z += time * 0.01f;
            }
            if (currentKeyboard.IsKeyDown(Keys.Divide))
            {
                Position.Z -= time * 0.01f;
            }
 
        }

        public void DetectGesture(GameTime gameTime)
        {


            // Ищем жест масштабирования - движение рук в стороны по оси Х и Y с ускорением
            if (!GestureAnyIsActive)
            {
                if ((AvateeringXNA.leftCursorVectorDirection.X > gestureScaleAccelerationFactor)
                    & (AvateeringXNA.cursorVectorDirection.X < -gestureScaleAccelerationFactor)
                    & (AvateeringXNA.leftCursorVectorDirection.Y > gestureScaleAccelerationFactor)
                    & (AvateeringXNA.cursorVectorDirection.Y < -gestureScaleAccelerationFactor))
                {
                    GestureAnyIsActive = false; timer.Start(1000f);
                   // gestureScaleDetected = true;
                    scale.X += time*gestureScaleIncrement;
                    scale.Y += time * gestureScaleIncrement;

                }
                else if ((AvateeringXNA.leftCursorVectorDirection.X < -gestureScaleAccelerationFactor)
                         & (AvateeringXNA.cursorVectorDirection.X > gestureScaleAccelerationFactor)
                         & (AvateeringXNA.leftCursorVectorDirection.Y < -gestureScaleAccelerationFactor)
                         & (AvateeringXNA.cursorVectorDirection.Y > gestureScaleAccelerationFactor))
                {
                    GestureAnyIsActive = false; timer.Start(1000f);
                   // gestureScaleDetected = true;
                    scale.X -= time * gestureScaleIncrement;
                    scale.Y -= time * gestureScaleIncrement;
                    

                }
   
                if (gestureScaleDetected)
                {
                    //        time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //        if (time > 200) gestureScaleDetected = false;
                }


                //Ищем жест перемещения окна на задний план - обе руки вперед с ускорением
                if ((AvateeringXNA.leftCursorVectorDirection.Z > gestureBringToBackAccelFactor)
                    & (AvateeringXNA.cursorVectorDirection.Z > gestureBringToBackAccelFactor)
                    & (AvateeringXNA.leftCursorVectorDirection.X > -0.5f)
                    & (AvateeringXNA.cursorVectorDirection.X < 0.5f)
                    & WindowIsActive)
                {
                    BringToBack();
                    GestureAnyIsActive = true; timer.Start(1000f);
                }
                    

                //  if ((AvateeringXNA.leftCursorVectorDirection.Z < gestureScaleAccelerationFactor)
                //         & (AvateeringXNA.cursorVectorDirection.Z < gestureScaleAccelerationFactor))
                //     BringToFront();


            }
            else
            {
                if (timer.IsActive) { timer.Update(gameTime); }
                else
                {
                    GestureAnyIsActive = false;
                }
            }
        }




    }
}
