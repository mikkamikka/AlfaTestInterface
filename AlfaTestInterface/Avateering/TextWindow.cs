using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input.Devices;
using Nuclex.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Nuclex.Input;
using Nuclex.UserInterface.Controls.Desktop;


namespace Microsoft.Samples.Kinect.Avateering
{
    public class TextWindow : WindowMedia
    {
        public Vector3 scale;
        public Vector3 bb_scale;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        public bool textWindowIsOpen = false;
        public bool previousTextWindowIsOpen;
      //  public bool textWindowIsActive = false;
      //  public bool textWindowIsOnFront = false;
        public bool BringToBackActivated = false;
        public bool BringToFrontActivated = false;
        public float time;
        //public float gestureScaleAccelerationFactor;
        public float gestureScaleIncrement;
        public bool gestureScaleDetected;

        SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private string textcontent;


        private RenderTarget2D target;
        private Texture2D tex;

        private BufferedKeyboard bufferedKeyboard;

        /// <summary>Manages input devices for the game</summary>
        private InputManager input;

        /// <summary>Temporary string builder used for various purposes</summary>
        private StringBuilder tempStringBuilder = new StringBuilder();
        /// <summary>Contains the text the user has entered on the keyboard</summary>
        private StringBuilder userInputStringBuilder = new StringBuilder();

        private Texture2D whiteRectangle;

        public TextWindow (Game game) : base(game)
        {
            
            LoadContent();
            
            // Automatically query the input devices once per update
            Game.Components.Add(this.input);
        }

        public override void Initialize()
        {
            base.Initialize();


            
        }

        private void keyboardCharacterEntered(char character)
        {

            if (AvateeringXNA.TextInputEnabled) 
            {if (character == '\b')
            { // backspace
                if (userInputStringBuilder.Length > 0)
                {
                    userInputStringBuilder.Remove(userInputStringBuilder.Length - 1, 1);
                }
            }
            else
            {
               
                userInputStringBuilder.Append(character);
            }
            }
        }

        
        private String parseText(String text)
        {
            String line = String.Empty;
            //String line = textcontent;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (spriteFont.MeasureString(line + word).Length() > 1100f)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }
                line = line + word + ' ';
            }
            return returnString + line;
        }



        protected override void LoadContent()
        {
            Model = Game.Content.Load<Model>("plane_text");
            BoundingBox = new BoundingBox();


            Position = new Vector3(10, 12, 10);
            Rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1.0f, 1.0f, 1.0f);
            bb_size = new Vector3(6, 8, 0.02f);

            //input.GetKeyboard().CharacterEntered += keyboardCharacterEntered;

            
            //pic = Texture2D.FromStream(GraphicsDevice, picCollection[currentPic].GetImage());
            tex = Game.Content.Load<Texture2D>("text_box_tex");


            input = new InputManager(Game.Services, Game.Window.Handle);
            // Whenever a key is pressed on the keyboard, call the keyboardCharacterEntered() method
            // (see below) so the game can do something with the character.
            IKeyboard keyboard = this.input.GetKeyboard();
            keyboard.CharacterEntered += (keyboardCharacterEntered);

            //gestureScaleAccelerationFactor = 1.0f;
            gestureScaleIncrement = 0.004f;

            timer = new Timer();


            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("Courier New");
            textcontent = "Съешь ещё этих мягких французских булок, да выпей чаю";
     
            target = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                        Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
                                        false,
                                        Game.GraphicsDevice.PresentationParameters.BackBufferFormat,
                                        DepthFormat.Depth24,
                                        2,
                                        RenderTargetUsage.PlatformContents);
            whiteRectangle = new Texture2D(Game.GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            
            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update saved state.
            previousKeyboard = currentKeyboard;

            

            // Update the status of all input devices
            if (AvateeringXNA.TextInputEnabled)  input.Update();


            bb_scale = new Vector3(scale.X, scale.Y, scale.Z);

            if (textWindowIsOpen)
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
            
            previousTextWindowIsOpen = textWindowIsOpen;

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

                    }
            }

            Collide();

            if (WindowIsActive)
            {
                DetectGesture(gameTime);
            }

        }




        public override void Draw(GameTime gameTime)
        {
            Vector2 FontOrigin = spriteFont.MeasureString(textcontent) / 8f;

            textcontent = parseText("Съешь ещё этих мягких французских булок, да выпей чаю" + userInputStringBuilder);

            Game.GraphicsDevice.SetRenderTarget(target); // Now the spriteBatch will render to the RenderTarget2D
            Game.GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                              SamplerState.AnisotropicClamp, DepthStencilState.Default,
                              RasterizerState.CullNone);
            //spriteBatch.Draw(whiteRectangle, new Rectangle(10,10,500,700), Color.Beige );
            spriteBatch.DrawString(spriteFont, textcontent, new Vector2(Position.X + 300, Position.Y), Color.Black, 0f, new Vector2(0, 0), 
                                    1f, SpriteEffects.None, 0.5f);
            
            spriteBatch.End();
            Game.GraphicsDevice.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

            tex = target; 

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
                    
                    effect.Texture = tex;


                    effect.World = modelMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = AvateeringXNA.view;
                    effect.Projection = AvateeringXNA.projection;

                    if (WindowIsActive)
                    {
                        // effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                        // effect.DirectionalLight1.Direction = new Vector3(1, -2, -1);
                        // effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);

                        effect.EmissiveColor = new Vector3(1,1,1);
                        effect.PreferPerPixelLighting = true;
                        effect.Alpha = 1f;
                    }
                    else
                    {
                        effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0, 0);
                        effect.DirectionalLight1.Direction = new Vector3(1, 1, -1);
                        effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);

                        effect.EmissiveColor = new Vector3(0, 0, 0);
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
                        current_hilite = new Vector3(0.1f, 0.1f, 0.1f);
                           if (AvateeringXNA.cursorVectorDirection.Z > 0.2f)
                           {
                               
                               if ((previousCollideResult1 == ContainmentType.Disjoint) & !WindowIsActive)
                               {
                                   Activate();
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
                default:
                    break;
            }
            previousCollideResult1 = collideResults1;
        }


        public void BringToFront()
        {
            BringToFrontActivated = true;
            BringToBackActivated = false;
            AvateeringXNA.TextInputEnabled = true;
        }

        public void BringToBack()
        {
            BringToBackActivated = true;
            BringToFrontActivated = false;
            WindowIsActive = false;
            WindowIsOnFront = false;
            AvateeringXNA.TextInputEnabled = false;
        }

        public void StartShowing()
        {
            StepBack();
            textWindowIsOpen = true;
            WindowIsActive = true;
            WindowIsOnFront = true;
            AvateeringXNA.TextInputEnabled = true;
            AvateeringXNA.AllMediaWindows.Add(this);
        }

        public void Activate()
        {
            WindowIsActive = true;
            AvateeringXNA.TextInputEnabled = true;
            Position.Z += 1f;
        }

        public void Deactivate()
        {
            WindowIsActive = false;
            AvateeringXNA.TextInputEnabled = false;
        }

        public void CloseWindow()
        {
            WindowIsActive = false;
            WindowIsOnFront = false;
            textWindowIsOpen = false;
            AvateeringXNA.TextInputEnabled = false;
            AvateeringXNA.AllMediaWindows.Remove(this);
            StepForward();
        }

        public void UpdateInput(GameTime gameTime)
        {
            currentKeyboard = Keyboard.GetState();

            if (currentKeyboard.IsKeyDown(Keys.Add) || currentKeyboard.IsKeyDown(Keys.OemPlus))
            {
                scale.X += time * 0.001f; scale.Y += time * 0.001f;
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
                    scale.X += time * gestureScaleIncrement;
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
