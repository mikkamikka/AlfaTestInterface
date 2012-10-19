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
    class PictureWindow : WindowMedia
    {
        public string PictureWindowType { get; set; }

        ICollection<MediaSource> mediaSources;
        MediaLibrary mediaLib;
        PictureCollection picCollection;
        Texture2D pic;
        public Picture picture;
        public VideoPlayer player;
        public Vector3 scale;
        public Vector3 bb_scale;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        public bool pictureWindowIsOpen = false;
        public bool previousPictureWindowIsOpen;
      //  public bool pictureWindowIsActive = false;
      //  public bool pictureWindowIsOnFront = false;
        public bool BringToBackActivated = false;
        public bool BringToFrontActivated = false;
        public float time;
        //public float gestureScaleAccelerationFactor;
        public float gestureScaleIncrement;
        public bool gestureScaleDetected;
        public int currentPic;

    
        public PictureWindow(Game game) : base(game)
        {
            PictureWindowType = null;
        }

    public void LoadContent(ContentManager content, string modelName, string pictureName)
        {
            Model = content.Load<Model>(modelName);
            PictureWindowType = modelName;
            BoundingBox = new BoundingBox();
            //  scale = new Vector3();
            //   transformMatrix = new Matrix();
            //  bb_size_min = GetBoundingBoxSize(Model, transformMatrix).Min;
            //  bb_size_max = GetBoundingBoxSize(Model, transformMatrix).Max;

            Position = new Vector3(0, 12, 0);
            Rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1.0f, 1.0f, 1.0f);
            bb_size = new Vector3(8, 6, 0.02f);

            // Load a picture
            mediaSources = MediaSource.GetAvailableMediaSources();
            mediaLib = new MediaLibrary();
            picCollection = mediaLib.Pictures;

            currentPic = 1;
            //pic = Texture2D.FromStream(GraphicsDevice, picCollection[currentPic].GetImage());
            pic = content.Load<Texture2D>(pictureName);

            //pictureWindowIsActive = false;
            //pictureWindowIsOnFront = false;

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


        if (pictureWindowIsOpen)
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

        previousPictureWindowIsOpen = pictureWindowIsOpen;

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
                effect.Texture = pic;



                effect.World = modelMatrix * transforms[mesh.ParentBone.Index];
                effect.View = AvateeringXNA.view;
                effect.Projection = AvateeringXNA.projection;

                if (WindowIsActive)
                {
                    // effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                    // effect.DirectionalLight1.Direction = new Vector3(1, -2, -1);
                    // effect.DirectionalLight1.SpecularColor = new Vector3(0, 0, 0);

                    effect.EmissiveColor = current_hilite;
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
    }

    public void BringToBack()
    {
        BringToBackActivated = true;
        BringToFrontActivated = false;
        WindowIsActive = false;
        WindowIsOnFront = false;
    }

    public void StartShowing()
    {
        StepBack();
        pictureWindowIsOpen = true;
        WindowIsActive = true;
        WindowIsOnFront = true;
        AvateeringXNA.AllMediaWindows.Add(this);
    }

    public void Activate()
    {
        WindowIsActive = true;
        Position.Z += 1f;
    }

    public void CloseWindow()
    {
        WindowIsActive = false;
        pictureWindowIsOpen = false;
        WindowIsOnFront = false;
        AvateeringXNA.AllMediaWindows.Remove(this);
        StepForward();
    }

    public void UpdateInput(GameTime gameTime)
    {
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
