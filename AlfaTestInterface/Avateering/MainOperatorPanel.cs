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
    public class MainOperatorPanel : Environment
    {
        public Vector3 scale;
        public Vector3 bb_scale;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        public bool textWindowIsOpen = false;
        public bool previousTextWindowIsOpen;
        public float time;

        SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private string textcontent;


        private RenderTarget2D target;
        private Texture2D tex;

        private Texture2D whiteRectangle;

        private PanelIcon[] panelIcon = new PanelIcon[3];
        public List<PanelIcon> AllPanelIcons = new List<PanelIcon>();

        public MainOperatorPanel(Game game)
            : base(game)
        {
            Game.Components.Add(this);
        }

        protected override void LoadContent()
        {
            Model = Game.Content.Load<Model>("Panel/MainOperatorPanel");
            BoundingBox = new BoundingBox();
            debugDraw = new DebugDraw(Game.GraphicsDevice);

            Position = new Vector3(6, 15, 14);
            Rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1.0f, 1.0f, 1.0f);
            bb_size = new Vector3(8f, 4.4f, 0.02f);

            //tex = Game.Content.Load<Texture2D>("text_box_tex");
            
            //timer = new Timer();
            
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            
       /*     target = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                        Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
                                        false,
                                        Game.GraphicsDevice.PresentationParameters.BackBufferFormat,
                                        DepthFormat.Depth24,
                                        2,
                                        RenderTargetUsage.PlatformContents);
           // whiteRectangle = new Texture2D(Game.GraphicsDevice, 1, 1);
           // whiteRectangle.SetData(new[] { Color.White });
    */

           // panelIcon = new PanelIcon[3];
                


            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update saved state.
            previousKeyboard = currentKeyboard;


            bb_scale = new Vector3(scale.X, scale.Y, scale.Z);


                BoundingBox.Min = Position - bb_scale * bb_size / 2;
                BoundingBox.Max = Position + bb_scale * bb_size / 2;


            UpdateInput(gameTime);

            Collide();

            
        }




        public override void Draw(GameTime gameTime)
        {
            //Vector2 FontOrigin = spriteFont.MeasureString(textcontent) / 8f;

            //Game.GraphicsDevice.SetRenderTarget(target); // Now the spriteBatch will render to the RenderTarget2D
            //Game.GraphicsDevice.Clear(Color.White);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
             //                 SamplerState.AnisotropicClamp, DepthStencilState.Default,
             //                 RasterizerState.CullNone);



            //spriteBatch.Draw(whiteRectangle, new Rectangle(10,10,500,700), Color.Beige );
            //spriteBatch.DrawString(spriteFont, textcontent, new Vector2(Position.X + 300, Position.Y), Color.Black, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.5f);

            //spriteBatch.End();
           // Game.GraphicsDevice.SetRenderTarget(null);      //This will set the spriteBatch to render to the screen again.


            //tex = target;

            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            translateMatrix = Matrix.CreateTranslation(Position);

            Vector3 position = translateMatrix.Translation;
            translateMatrix *= Matrix.CreateFromAxisAngle(translateMatrix.Right, MathHelper.ToRadians(90));
            translateMatrix *= Matrix.CreateScale(scale);
            translateMatrix.Translation = position;
            Matrix modelMatrix = translateMatrix;


            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    //effect.Texture = tex;


                    effect.World = modelMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = AvateeringXNA.view;
                    effect.Projection = AvateeringXNA.projection;

                    effect.EnableDefaultLighting();

                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f); 
                    effect.DirectionalLight0.Direction = new Vector3(0.8f, -0.8f, -0.8f);  
                    effect.DirectionalLight0.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f); 


                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f); 
                    effect.DirectionalLight1.Direction = new Vector3(-0.8f, 0.8f, -0.8f); 
                    effect.DirectionalLight1.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f); 

                        //effect.EmissiveColor = new Vector3(1, 1, 1);
                        effect.PreferPerPixelLighting = true;
                        effect.Alpha = 1f;
                    
                    

                }

              

                mesh.Draw();
            }


            // Отрисовка иконок медиа-объектов на панели Входящих заданий главного оператора
            int maxcount;
            if (AllPanelIcons.Count < 3)
            {
                maxcount = AllPanelIcons.Count;
            }
            else maxcount = 3;

            for (int i = 0; i < maxcount; i++)
            {
                //panelIcon[i] = AllPanelIcons[AllPanelIcons.Count - i];
                panelIcon[i] = AllPanelIcons[i];
                panelIcon[i].Position = new Vector3(Position.X + 1.6f, Position.Y - (float)i * 1.2f + 1.4f, Position.Z+0.02f);
                panelIcon[i].scale = scale;
                //panelIcon[i].Draw(gameTime);
            }

            
            
            DrawBoundingBox();

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

                            if ((previousCollideResult1 == ContainmentType.Disjoint))
                            {
                                
                            }
                        }


                    }
                    break;
                default:
                    break;
            }
            previousCollideResult1 = collideResults1;
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




    }
}
