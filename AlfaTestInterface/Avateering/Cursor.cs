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
    public class Cursor : DrawableGameComponent
    {
        private string modelName;
        private Matrix view;
        private Matrix projection;
        private Model model;
        public Vector3 Position;
        public Vector3 Rotation;
        public float scale;
        public BoundingBox boundingBox;
        public Matrix translateMatrix;
      //  public BasicEffect _effect;
        public Vector3 current_hilite;
        public Vector3 bb_size_min;
        public Vector3 bb_size_max;
        public Vector3 half = new Vector3(0.5F, 0.5F, 0.5F);
        public Vector3 bb_size;
        public Vector3 boundingBoxTranslate;
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

        public bool OnIntersect;

        public Cursor(Game game, string _modelName)
            : base(game)
        {
            modelName = _modelName;
            game.Components.Add(this);

        }



        protected override void LoadContent()
        {
            model = Game.Content.Load<Model>(modelName);
            scale = 1.0f;
            boundingBox = new BoundingBox();

            boundingBoxTranslate = new Vector3(0f, 0f, 0f);
            bb_size = new Vector3(1f, 1f, 1f);
            boundingBoxPositionFix = new Vector3(0f, 0f, 0f);

            debugDraw = new DebugDraw(Game.GraphicsDevice);



            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            CursorStateChecker();

        }

        public override void Draw(GameTime gameTime)
        {
            
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            translateMatrix = Matrix.CreateTranslation(Position); 
                                     
            Vector3 position = translateMatrix.Translation;
            //translateMatrix *= Matrix.CreateFromAxisAngle(translateMatrix.Right, MathHelper.ToRadians(90));    // включаем только если нужно крутить модель
            translateMatrix *= Matrix.CreateScale(scale);
            translateMatrix.Translation = position;

            Matrix worldMatrix = translateMatrix;

            view = AvateeringXNA.view;
            projection = AvateeringXNA.projection;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    
                    effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.TextureEnabled = true;

                  //  effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
                   // effect.EmissiveColor = new Vector3(0, 0, 0);

                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f); 
                    effect.DirectionalLight0.Direction = new Vector3(-0.8f, -0.8f, -0.8f);  
                    effect.DirectionalLight0.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f); 

                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
                    effect.DirectionalLight1.Direction = new Vector3(0.8f, 0.8f, -0.8f);
                    effect.DirectionalLight1.SpecularColor = new Vector3(0.8f, 0.8f, 0.8f);

                    effect.EmissiveColor = new Vector3(0, 0, 0);

                    if (OnIntersect)
                    {
                        effect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0.3f);
                        effect.DirectionalLight0.Direction = new Vector3(-0.8f, -0.8f, -0.8f);
                        effect.DirectionalLight0.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);

                        effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                        effect.DirectionalLight1.Direction = new Vector3(0.8f, 0.8f, -0.8f);
                        effect.DirectionalLight1.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);  

                        effect.EmissiveColor = new Vector3(0.8f,0.8f,0.0f);
                    }


                    
                   
                }
                mesh.Draw();
            }

            boundingBox.Min = Position - bb_size / 2;
            boundingBox.Max = Position + bb_size / 2;

            drawBoundingBoxesOn = AvateeringXNA.drawBoundingBoxesOn;

            if (drawBoundingBoxesOn)
            {
                debugDraw.Begin(view, projection);

                debugDraw.DrawWireBox(boundingBox, Color.SteelBlue);

                debugDraw.End();

            }

            base.Draw(gameTime);
        }


        private void CursorStateChecker ()    // проверяем курсор на пересечения с объектами среды и меняем вид курсора
        {
            bool checkIfAnyIntersects = new bool();
            bool[] checkCurrent = new bool[AvateeringXNA.AllEnvironmentItems.Count];

            for (int i = 0; i < AvateeringXNA.AllEnvironmentItems.Count; i++)
            {
                collideResults1 = boundingBox.Contains(AvateeringXNA.AllEnvironmentItems[i].BoundingBox);
                checkCurrent[i] = collideResults1 == ContainmentType.Intersects;
                checkIfAnyIntersects = checkIfAnyIntersects || checkCurrent[i];
            }

            if (checkIfAnyIntersects)
            {
                OnIntersect = true;
                scale = 1.2f;
            }
            else
            {
                OnIntersect = false;
                scale = 1.0f;
            }
        }






    }
}
