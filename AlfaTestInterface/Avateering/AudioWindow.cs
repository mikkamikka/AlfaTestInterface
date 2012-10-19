using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Samples.Kinect.Avateering
{
    class AudioWindow : WindowMedia
    {
        //public MediaLibrary sampleMediaLibrary;
        public SoundEffect audio;
        public SoundEffectInstance audioInstance;

        private Texture2D skin_playing, skin_paused, skin_current;

        public bool audioWindowIsOpen = false;
        public bool previousAudioWindowIsOpen;
     //   public bool audioWindowIsActive = false;
     //   public bool audioWindowIsOnFront = false;

        public bool BringToBackActivated = false;
        public bool BringToFrontActivated = false;

        public float time;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;

        public Vector3 scale;
        public Vector3 bb_scale;

        //private Texture2D tex;
        //public Vector2 position;
        //public float rotation;
        //private Vector2 origin;
        //public Vector2 scale;
        //private Nullable<Rectangle> sourceRectangle;
        //public SpriteBatch spriteBatch;


        public AudioWindow(Game game)
            : base(game)
        {
  
        }

        public void LoadContent(ContentManager content, string modelName, string audioclipName)
        {
            Model = content.Load<Model>(modelName);
            BoundingBox = new BoundingBox();

            Position = new Vector3(-8, 10, 0);
            Rotation = new Vector3(0 ,0 , 0);
  //        scale = new Vector3(1.0f, 1.0f, 1.0f);
            bb_size = new Vector3(12, 4, 0.02f);
            scale = new Vector3(1,1,1);

            //spriteBatch = new SpriteBatch(GraphicsDevice);
            //tex = Game.Content.Load<Texture2D>("audioplayer_tex");
            //position = new Vector2(100,100);
            //origin= new Vector2(10,10);
            //scale = new Vector2(1,1);
            
            audio = content.Load<SoundEffect>(audioclipName);
            audioInstance = audio.CreateInstance();
            //audioInstance.Play();

            skin_playing = content.Load<Texture2D>("audio_player_playing");
            skin_paused = content.Load<Texture2D>("audio_player_paused");

        }

        public override void Update(GameTime gameTime)
        {

            // Update saved state.
            previousKeyboard = currentKeyboard;

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            bb_scale = scale;

            if (audioWindowIsOpen)
            {
                BoundingBox.Min = Position - bb_scale * bb_size / 2;
                BoundingBox.Max = Position + bb_scale * bb_size / 2;
            }
            else
            {
                BoundingBox.Min = new Vector3(0, 0, -100f);
                BoundingBox.Max = new Vector3(0, 0, -100f);
            }
            
             
            //UpdateInput(gameTime);

            CollideWindow();
            DetectDragGesture();

            previousAudioWindowIsOpen = audioWindowIsOpen;

            if (BringToFrontActivated)
            {
                if (Position.Z < AvateeringXNA.FrontPositionZ)
                {
                    Position.Z += time * 0.03f;
                }

                if (Position.Z >= AvateeringXNA.FrontPositionZ)  BringToFrontActivated = false;
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
                        audioInstance.Pause();
                        skin_current = skin_paused;
                        BringToBackActivated = false;
                        
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
             if (WindowIsActive) current_hilite = new Vector3(0.1f, 0.1f, 0.1f);  else current_hilite = new Vector3(0,0,0);


           // spriteBatch.Begin();
          //  spriteBatch.Draw(tex, position, sourceRectangle, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
           // spriteBatch.End();

             Matrix[] transforms = new Matrix[Model.Bones.Count];
             Model.CopyAbsoluteBoneTransformsTo(transforms);
             translateMatrix = Matrix.CreateTranslation(Position);
                               //Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z) *
                               //Matrix.CreateScale(scale);
             Vector3 position = translateMatrix.Translation;
             translateMatrix *= Matrix.CreateFromAxisAngle(translateMatrix.Right, MathHelper.ToRadians(90));
             translateMatrix *= Matrix.CreateScale(scale);
             translateMatrix.Translation = position;


             Matrix worldMatrix = translateMatrix;



             foreach (ModelMesh mesh in Model.Meshes)
             {
                 foreach (BasicEffect effect in mesh.Effects)
                 {
                     
                     //effect.Texture = player.GetTexture();

                     //effect.LightingEnabled = true;

                     effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                     effect.View = AvateeringXNA.view;
                     effect.Projection = AvateeringXNA.projection;

                     //effect.EnableDefaultLighting();

                     effect.TextureEnabled = true;
                     effect.EmissiveColor = current_hilite;
                     effect.Texture = skin_current;
                     
                     if (WindowIsActive)
                     {
                         effect.DirectionalLight1.DiffuseColor = new Vector3(1, 1, 1);
                         effect.DirectionalLight1.Direction = new Vector3(-1, -1, 1);
                         effect.DirectionalLight1.SpecularColor = new Vector3(1, 1, 1);
                         effect.PreferPerPixelLighting = true;
                         effect.Alpha = 1f;
                     }
                     else
                     {
                         effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0, 0);
                         effect.DirectionalLight1.Direction = new Vector3(1, 1, -1);
                         effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);
                         effect.Alpha = 0.5f;
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
                        current_hilite = new Vector3(0.3f, 0.3f, 0.3f);
                        if (AvateeringXNA.cursorVectorDirection.Z > 0.5f)
                        {
                            if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                (audioInstance.State == SoundState.Playing))
                            {
                                Pause();
                            }
                            else if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                     (audioInstance.State == SoundState.Paused))
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
                    }
                    break;
                
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
            audioInstance.Play();
            audioWindowIsOpen = true;
            WindowIsActive = true;
            WindowIsOnFront = true;
            skin_current = skin_playing;
            AvateeringXNA.AllMediaWindows.Add(this);
        }
        
        public void Pause()
        {
            audioInstance.Pause();
            skin_current = skin_paused;
        }
        
        public void Resume()
        {
            audioInstance.Play();
            audioWindowIsOpen = true;
            WindowIsActive = true;
            WindowIsOnFront = true;
            skin_current = skin_playing;
        }

        public void CloseWindow()
        {
            audioInstance.Stop(); 
            WindowIsActive = false;
            WindowIsOnFront = false;
            audioWindowIsOpen = false;
            AvateeringXNA.AllMediaWindows.Remove(this);
            StepForward();
        }



        public void DetectGesture(GameTime gameTime)
        {


        /*    // Ищем жест масштабирования - движение рук в стороны по оси Х с ускорением

            if ((AvateeringXNA.leftCursorVectorDirection.X > gestureScaleAccelerationFactor)
                & (AvateeringXNA.cursorVectorDirection.X < -gestureScaleAccelerationFactor))
            {
                gestureScaleDetected = true;
                scale.X += time * gestureScaleIncrement;
                scale.Z += time * gestureScaleIncrement;



            }         
            else if ((AvateeringXNA.leftCursorVectorDirection.X < -gestureScaleAccelerationFactor)
                     & (AvateeringXNA.cursorVectorDirection.X > gestureScaleAccelerationFactor))
            {
                scale.X -= time * gestureScaleIncrement;
                scale.Z -= time * gestureScaleIncrement;
                gestureScaleDetected = true;
               // audioInstance.Volume
            }
            //               else gestureScaleDetected = false;   */





            //Ищем жест перемещения окна на задний план - обе руки вперед с ускорением
            if ((AvateeringXNA.leftCursorVectorDirection.Z > gestureBringToBackAccelFactor)
                    & (AvateeringXNA.cursorVectorDirection.Z > gestureBringToBackAccelFactor)
                    & (AvateeringXNA.leftCursorVectorDirection.X > -0.5f)
                    & (AvateeringXNA.cursorVectorDirection.X < 0.5f))
                BringToBack();

            //  if ((AvateeringXNA.leftCursorVectorDirection.Z < gestureScaleAccelerationFactor)
            //         & (AvateeringXNA.cursorVectorDirection.Z < gestureScaleAccelerationFactor))
            //     BringToFront();


        }


    }
}
