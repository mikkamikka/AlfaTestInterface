using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Samples.Kinect.Avateering
{
    public class IconBox : Environment
    {
        public string InterfaceBoxType { get; set; }
        public bool PressButtonEffectOn;
        public Vector3 prevButtonPos;
        protected static Timer timer;
        protected static bool GestureAnyIsActive;
        private float iconBoxLineLength = 50f;
        private float iconBoxSpacing;
        protected int index;

        public IconBox(Game game)
            : base(game)
        {
            InterfaceBoxType = null;
            AvateeringXNA.AllIconBoxes.Add(this);
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            InterfaceBoxType = modelName;
            BoundingBox = new BoundingBox();
            //   transformMatrix = new Matrix();
            bb_size_min = new Vector3();
            bb_size_max = new Vector3();
            //  bb_size_min = GetBoundingBoxSize(Model, transformMatrix).Min;
            //  bb_size_max = GetBoundingBoxSize(Model, transformMatrix).Max;

            boundingBoxTranslate = new Vector3(0.5f, 1.0f, 0.125f);
            //bb_size = new Vector3(4f, 3f, 1f);
            boundingBoxPositionFix = new Vector3(0f, 1.5f, 0f);

            Position.Z = 20f;

            
  
            debugDraw = new DebugDraw(Game.GraphicsDevice);

            timer = new Timer();
        }

        public override void Update(GameTime gameTime)
        {
            BoundingBox.Min = Position * boundingBoxTranslate - bb_size / 2f + boundingBoxPositionFix;
            BoundingBox.Max = Position * boundingBoxTranslate + bb_size / 2f + boundingBoxPositionFix;

            if (PressButtonEffectOn)      //эффект нажатия на иконку
            {
                if (timer.IsActive)
                {
                    timer.Update(gameTime);
                }
                else
                {
                    Position = prevButtonPos;
                    PressButtonEffectOn = false;
                }
            }



            // рисуем все иконки рабочего стола
            iconBoxSpacing = iconBoxLineLength/AvateeringXNA.AllIconBoxes.Count;

            for (int i = 0; i < AvateeringXNA.AllIconBoxes.Count; i++)
            {
                AvateeringXNA.AllIconBoxes[i].Position.X = iconBoxSpacing * (i) - iconBoxLineLength / 2f + iconBoxSpacing / 2f;
            }

            
        }

        protected void HideIconBox()                       // триггер исключения иконки из списка иконок рабочего стола
        {
            //parkBoundingBox = BoundingBox;
            index = AvateeringXNA.AllIconBoxes.IndexOf(this);
            AvateeringXNA.AllIconBoxes.Remove(this);   //убираем иконку из списка иконок рабочего стола
            //BoundingBox = hideBoundingBox;
            Position.Y = -50f;                         //прячем позицию иконки, чтобы убрать с рабочего стола bounding box.
        }

        public void Draw()
        {
            transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            translateMatrix = Matrix.CreateTranslation(Position);

            Vector3 position = translateMatrix.Translation;
            //translateMatrix *= Matrix.CreateFromAxisAngle(translateMatrix.Right, MathHelper.ToRadians(90));
            translateMatrix *= Matrix.CreateScale(Scale);
            translateMatrix.Translation = position;
            Matrix worldMatrix = translateMatrix;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = AvateeringXNA.view;
                    effect.Projection = AvateeringXNA.projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.TextureEnabled = true;

                    // effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
                    // effect.EmissiveColor = new Vector3(0, 0, 0);

                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f); // a red light
                    effect.DirectionalLight0.Direction = new Vector3(0.8f, -0.8f, -0.8f);  // coming along the x-axis
                    effect.DirectionalLight0.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f); // with green highlights


                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f); // a red light
                    effect.DirectionalLight1.Direction = new Vector3(-0.8f, 0.8f, -0.8f);  // coming along the x-axis
                    effect.DirectionalLight1.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f); // with green highlights

                    //effect.SpecularColor = new Vector3(0.2f,0.1f,0.3f);
                    //effect.SpecularPower = 0.001f;
                    effect.EmissiveColor = current_hilite;
                }
                mesh.Draw();
            }



            //drawBoundingBoxesOn = true;
            drawBoundingBoxesOn = AvateeringXNA.drawBoundingBoxesOn;

            if (drawBoundingBoxesOn)
            {
                debugDraw.Begin(AvateeringXNA.view, AvateeringXNA.projection);
                debugDraw.DrawWireBox(BoundingBox, Color.Yellow);
                debugDraw.End();
            }
        }



        public void PressButtonEffectActivate()
        {
            PressButtonEffectOn = true; timer.Start(600f);   // эффект нажатия на иконку
            prevButtonPos = Position;
            Position = new Vector3(Position.X, Position.Y, Position.Z - 3f);
            
        }

        private void Collide(GameTime gameTime)
        {
            collideResults1 = BoundingBox.Contains(AvateeringXNA.CursorRight.boundingBox);

        }


    }
}
