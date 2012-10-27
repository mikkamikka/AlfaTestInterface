using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.Avateering
{
    public class PanelIcon :Environment
    {
        public Vector3 scale;
        public Vector3 bb_scale;

        //public bool previousTextWindowIsOpen;
        public float time;

        SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private string textcontent;

        private Task.MediaType mediaType;
        private Task.ManagerStatus managerStatus;
        private Task.ProgressStatus progressStatus;
        private string mediaName;
        private int operatorID;


        private int iconSpaceWidth, iconSpaceHeight, iconSizeX, iconSizeY, statusSizeX, statusSizeY;

        private RenderTarget2D target;
        private Texture2D textureIconCombined;
        private Texture2D renderResult;

        private Rectangle iconMediaTypeAudioRectangle, 
                          iconMediaTypeVideoRectangle, 
                          iconMediaTypePictureRectangle,
                          currentRectangle;

        private Rectangle iconManagerStatusNotAssignedRectangle,
                          iconManagerStatusAssignedRectangle,
                          iconManagerStatusFinishedRectangle,
                          currentManagerStatus;

        //private Task task;

        //private Quad iconSpace;

        public PanelIcon(Game game, Task.MediaType _mediaType, Task.ManagerStatus _managerStatus, Task.ProgressStatus _progressStatus, String _mediaName, int _operatorID)
            : base(game)
        {
            Game.Components.Add(this);
            mediaType = _mediaType;
            managerStatus = _managerStatus;
            progressStatus = _progressStatus;
            mediaName = _mediaName;
            operatorID = _operatorID;
        }

        protected override void LoadContent()
        {
            Model = Game.Content.Load<Model>("Panel/PanelIcon");

            BoundingBox = new BoundingBox();
            debugDraw = new DebugDraw(Game.GraphicsDevice);
            bb_size = new Vector3(3.75f, 1.0f, 0.02f);

           // Position = new Vector3(6, 15, 14);
            Rotation = new Vector3(0, 0, 0);
            //scale = new Vector3(1.0f, 1.0f, 1.0f);
            

            textureIconCombined = Game.Content.Load<Texture2D>("Panel/panels_iconstatuses_all");
            
            iconSpaceHeight = 200;
            iconSpaceWidth = (int)(iconSpaceHeight*3.654f);

            iconSizeY = (int) (iconSpaceHeight*0.62f);
            iconSizeX = (int) (iconSizeY*1.17f);

            statusSizeY = (int)(iconSpaceHeight * 0.8f);
            statusSizeX = (int) (statusSizeY*2.86f);
            
            iconMediaTypeAudioRectangle = new Rectangle(0, 0, 143, 122);
            iconMediaTypeVideoRectangle = new Rectangle(0, 161, 143, 122);
            iconMediaTypePictureRectangle = new Rectangle(0, 315, 143, 122);

            iconManagerStatusNotAssignedRectangle = new Rectangle(145, 0, 461, 162);
            iconManagerStatusAssignedRectangle = new Rectangle(145, 161, 461, 161);
            iconManagerStatusFinishedRectangle = new Rectangle(145, 315, 461, 158);

            switch (mediaType)
            {
                case Task.MediaType.Audio:
                    currentRectangle = iconMediaTypeAudioRectangle;
                    break;
                case Task.MediaType.Video:
                    currentRectangle = iconMediaTypeVideoRectangle;
                    break;
                case Task.MediaType.Picture:
                    currentRectangle = iconMediaTypePictureRectangle; 
                    break;

            }


            //timer = new Timer();

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("Segoe16");
            textcontent = "Номер";

            target = new RenderTarget2D(Game.GraphicsDevice,
                                        iconSpaceWidth, iconSpaceHeight,
                                        //Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 4,
                                        //Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 5,
                                        false,
                                        Game.GraphicsDevice.PresentationParameters.BackBufferFormat,
                                        DepthFormat.Depth24,
                                        2,
                                        RenderTargetUsage.DiscardContents);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            bb_scale = new Vector3(scale.X, scale.Y, scale.Z);
            BoundingBox.Min = Position - bb_scale * bb_size / 2 - new Vector3(0.5f,-0.1f,0f);
            BoundingBox.Max = Position + bb_scale * bb_size / 2 + new Vector3(0.0f, 0.1f, 0f);
            
        //    Collide();


            switch (managerStatus)
            {
                case Task.ManagerStatus.NotAssigned:
                    currentManagerStatus = iconManagerStatusNotAssignedRectangle;
                    break;
                case Task.ManagerStatus.Assigned:
                    currentManagerStatus = iconManagerStatusAssignedRectangle;
                    break;
                case Task.ManagerStatus.Finished:
                    currentManagerStatus = iconManagerStatusFinishedRectangle;
                    break;

            }

        }

        public override void Draw(GameTime gameTime)
        {
            //Vector2 FontOrigin = spriteFont.MeasureString(textcontent) / 8f;

            Game.GraphicsDevice.SetRenderTarget(target); // Now the spriteBatch will render to the RenderTarget2D
            Game.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                              SamplerState.AnisotropicClamp, DepthStencilState.Default,
                              RasterizerState.CullNone);
            
            // отрисовка иконки
            spriteBatch.Draw(
                textureIconCombined,            //исходная текстура
                new Rectangle(0, 0, iconSizeX, iconSizeY),              //позиция вывода , ширина и высота
                currentRectangle,               //область на текстуре
                Color.White,                    //цвет
                0f,                             //поворот
                new Vector2(0,0),               //смещение origin
               // 0.3f,                           //масштаб
                SpriteEffects.None,             //
                1f                              //слой
                );

            // отрисовка статуса Manager
            spriteBatch.Draw(
                textureIconCombined,            //исходная текстура
                new Rectangle(0, 0, statusSizeX, statusSizeY),              //позиция вывода , ширина и высота
                currentManagerStatus,    //область на текстуре
                Color.White,                    //цвет
                0f,                             //поворот
                new Vector2(-110, 0),               //смещение origin
                // 0.3f,                           //масштаб
                SpriteEffects.None,             //
                0f                              //слой
                );

            //spriteBatch.Draw(whiteRectangle, new Rectangle(10,10,500,700), Color.Beige );
            //spriteBatch.DrawString(spriteFont, textcontent, new Vector2(Position.X + 300, Position.Y), Color.Black, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.5f);

            spriteBatch.End();
            Game.GraphicsDevice.SetRenderTarget(null);      //This will set the spriteBatch to render to the screen again.


            renderResult = target;



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

                    effect.Texture = renderResult;


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

                    //effect.EmissiveColor = new Vector3(0.2f, 0.2f, 0.2f);
                    effect.PreferPerPixelLighting = true;
                    //effect.Alpha = 1f;
                   
                }
                mesh.Draw();
            }


            DrawBoundingBox();
            
        }


    }
}
