using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Samples.Kinect.Avateering
{
 
    public class Environment : DrawableGameComponent
    {
        public Model Model;
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale;
        public BoundingBox BoundingBox;
        public BoundingBox parkBoundingBox;
        public BoundingBox hideBoundingBox;
        public Matrix translateMatrix;
        public Matrix[] transforms;
        public BasicEffect _effect;
        public Vector3 current_hilite;
        public Vector3 bb_size_min;
        public Vector3 bb_size_max;
        public Vector3 half = new Vector3(0.5F, 0.5F, 0.5F);
        public Vector3 bb_size;
        public Vector3 boundingBoxTranslate ;
        public Vector3 boundingBoxPositionFix;

        public DebugDraw debugDraw;
        public bool drawBoundingBoxesOn;
        public ContainmentType collideResults1 = new ContainmentType();
        public ContainmentType collideResults2 = new ContainmentType();
        public ContainmentType collideResults3 = new ContainmentType();
        public ContainmentType collideResults4 = new ContainmentType();

        public ContainmentType previousCollideResult1 = new ContainmentType();
        public ContainmentType previousCollideResult2 = new ContainmentType();
        public ContainmentType previousCollideResult3 = new ContainmentType();
        public ContainmentType previousCollideResult4 = new ContainmentType();


        public Environment(Game game)
            : base(game)
          {
            Model = null;
            Scale = 1.0f;
            AvateeringXNA.AllEnvironmentItems.Add(this);
          }




    }

    public class InterfaceBox : Environment
    {
        public string InterfaceBoxType { get; set; }
        public bool PressButtonEffectOn;
        public Vector3 prevButtonPos;
        public Timer timer;
        public bool GestureAnyIsActive;
        private float iconBoxLineLength;
        private float iconBoxSpacing;


        public InterfaceBox(Game game)
            : base(game)
        {
            InterfaceBoxType = null;
           // AvateeringXNA.AllIconBoxes.Add(this);
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
  
            debugDraw = new DebugDraw(Game.GraphicsDevice);

            timer = new Timer();
        }

        public override void Update(GameTime gameTime)
        {
           

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

            BoundingBox.Min = Position * boundingBoxTranslate - bb_size / 2f + boundingBoxPositionFix;
            BoundingBox.Max = Position * boundingBoxTranslate + bb_size / 2f + boundingBoxPositionFix;

            //drawBoundingBoxesOn = true;
            drawBoundingBoxesOn = AvateeringXNA.drawBoundingBoxesOn;

            if (drawBoundingBoxesOn)
            {
                debugDraw.Begin(AvateeringXNA.view, AvateeringXNA.projection);
                debugDraw.DrawWireBox(BoundingBox, Color.Yellow);
                debugDraw.End();
            }
        }





        private void Collide(GameTime gameTime)
        {
            collideResults1 = BoundingBox.Contains(AvateeringXNA.CursorRight.boundingBox);

        }


    }


}
