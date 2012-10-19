//------------------------------------------------------------------------------
// <copyright file="AvateeringXNA.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Samples.Kinect.Avateering.BoundingBoxes;
using Microsoft.Samples.Kinect.Avateering.Filters;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Nuclex.Testing.Xna;
using VideoQuad;
using MediaState = Microsoft.Xna.Framework.Media.MediaState;


namespace Microsoft.Samples.Kinect.Avateering
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.GamerServices;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Net;
    using Microsoft.Xna.Framework.Storage;
    
    /// Sample game showing how to display skinned character and avateer with Kinect for Windows.
    
    public class AvateeringXNA : Game
    {
        #region Fields


        /// This is used to adjust the window size. The height is set automatically from the width using a 4:3 ratio.
        private const int WindowedWidth = 1800;

        
        /// This is used to adjust the fullscreen window size. Only valid resolutions can be set.
        private const int FullScreenWidth = 1920;

       /// This is used to adjust the fullscreen window size. Only valid resolutions can be set.
        private const int FullScreenHeight = 1080;

        /// Camera Arc Increment value.
        private const float CameraArcIncrement = 0.05f;

        /// Camera Arc angle limit value.
        private const float CameraArcAngleLimit = 90.0f;

        /// Camera Zoom Increment value.
        private const float CameraZoomIncrement = 0.025f;

        private float nominalVerticalFieldOfView = 55.0f;

        /// Camera FOV Increment value.
        private float CameraFOV;

        /// Camera FOV Increment value.
        private const float CameraFOVIncrement = 0.015f;

        /// Camera Max Distance value.
        private const float CameraMaxDistance = 500.0f;

        /// Camera Min Distance value.
        private const float CameraMinDistance = 0.0f;

        /// Camera starting Distance value.
        private float CameraHeight = 10.0f;

        /// Camera starting Distance value.
        private const float CameraStartingTranslation = 28.0f;
        
        /// The "Dude" model mesh is defined at an arbitrary size in centimeters.
        /// Here we re-scale the Kinect translation so the model appears to walk more correctly on the ground.
        
        private static readonly Vector3 skeletonTranslationScaleFactor = new Vector3(20.0f, 20.0f, 20.0f);

        /// The graphics device manager provided by XNA.
        private readonly GraphicsDeviceManager graphics;

        /// This control selects a sensor, and displays a notice if one is not connected.
        private readonly KinectChooser chooser;
        
        /// This manages the rendering of the depth stream.
        private readonly DepthStreamRenderer depthStream;

        /// This manages the rendering of the skeleton over the depth stream.
        private readonly SkeletonStreamRenderer skeletonStream;
        
        /// This is the XNA Basic Effect used in drawing.
        private BasicEffect effect;    
        
        /// This is the SpriteBatch used for rendering the header/footer.
        private SpriteBatch spriteBatch;

        /// This is used when toggling between windowed and fullscreen mode.
        private bool fullscreenMode = false;

        private bool cullingModeOn = false;

        private PresentationParameters presentationParameters;
        

        /// This tracks the previous keyboard state.
        private KeyboardState previousKeyboard;

        /// This tracks the current keyboard state.
        private KeyboardState currentKeyboard;
        
        /// This is the texture for the header.
        //private Texture2D header;

        /// This is the coordinate cross we use to draw the world coordinate system axes.
        private CoordinateCross worldAxes;

        /// The 3D avatar mesh.
        private Model currentModel;

        /// Store the mapping between the NuiJoint and the Avatar Bone index.
        private Dictionary<JointType, int> nuiJointToAvatarBoneIndex;

        /// The 3D avatar mesh animator.
        //private AvatarAnimator animator, curs;

        /// Viewing Camera arc.
        private float cameraArc = 0;

        /// Viewing Camera current rotation.
        /// The virtual camera starts where Kinect is looking i.e. looking along the Z axis, with +X left, +Y up, +Z forward
        private float cameraRotation = 0; 

        /// Viewing Camera distance from origin.
        /// The "Dude" model is defined in centimeters, hence all the units we use here are cm.
        private float cameraDistance = CameraStartingTranslation;

        /// Viewing Camera view matrix.
        public static Matrix view;

        /// Viewing Camera projection matrix.
        public static Matrix projection;

        /// Draw the simple planar grid for avatar to stand on if true.
        private bool drawGrid;
        
        /// Simple planar grid for avatar to stand on.
        private GridXz planarXzGrid;

        
        /// Flag for first detection of skeleton.
        private bool skeletonDetected;

        public static bool drawBoundingBoxesOn;



        private bool keepCursorCoordinateZ = false;

 
        private InterfaceBox InterfaceBox;

        public static Cursor CursorRight, CursorLeft;

        private IconBoxVideo Video_box2;
        public IconBoxAudio iconboxAudio1;
        public IconBoxPicture iconboxPicture1;
        public IconBoxText iconboxText1;

        //public InterfaceBox focusedObject;

        private Vector3 scaled_right_hand_position = new Vector3 (0,0,0);
        private Vector3 scaled_left_hand_position = new Vector3(0, 0, 0);
        public static Vector3 head;
        public static Vector3 right_hand_joint;
        public static Vector3 left_hand_joint;

        private Vector3 previousCursorPosition;
        private Vector3 previousLeftCursorPosition;
        public static Vector3 cursorVectorDirection;
        public static Vector3 leftCursorVectorDirection;

        Matrix videoWorld, videoView, videoProjection;


        


        ContainmentType collideResults1 = new ContainmentType();
        ContainmentType collideResults2 = new ContainmentType();
        ContainmentType collideResults3 = new ContainmentType();
        ContainmentType collideResults4 = new ContainmentType();

        ContainmentType previousCollideResult1 = new ContainmentType();
        ContainmentType previousCollideResult2 = new ContainmentType();
        ContainmentType previousCollideResult3 = new ContainmentType();
        ContainmentType previousCollideResult4 = new ContainmentType();

        public const float FrontPositionZ = 10f;
        public const float BackPositionZ = -5f;
        public static bool TextInputEnabled;



        public static List<Environment> AllEnvironmentItems = new List<Environment>();
        public static List<IconBox> AllIconBoxes = new List<IconBox>();
        public static List<WindowMedia> AllMediaWindows = new List<WindowMedia>();




        #endregion

        #region Initialization
        
        /// Initializes a new instance of the AvateeringXNA class.
        
        public AvateeringXNA()
        {
            Window.Title = "Test1";
            IsFixedTimeStep = false;
            IsMouseVisible = true;

        //    Components.Add(new FrameRateCounter(this));  - откл. т.к. вызывает баг - меняет порядок отрисовки 3Д объектов


            // Setup the graphics device for rendering
            graphics = new GraphicsDeviceManager(this);
            presentationParameters = new PresentationParameters();
            SetScreenMode();
            graphics.PreparingDeviceSettings += GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = true;     // включили anti-aliasing
            graphics.SynchronizeWithVerticalRetrace = true;
          //  presentationParameters.MultiSampleCount = 4;



            Content.RootDirectory = "Content";

            // The Kinect sensor will use 640x480 for the color stream (default) and 640x480 for depth
            chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            Services.AddService(typeof(KinectChooser), chooser);

            // Optionally set near mode for close range avateering (0.4m up to 3m)
            chooser.NearMode = true;

            // Optionally set seated mode for upper-body only tracking here (typically used with near mode for close to camera tracking)
            chooser.SeatedMode = true;

            // Adding these objects as XNA Game components enables automatic calls to the overridden LoadContent, Update, etc.. methods
            Components.Add(chooser);

            // Create a ground plane for the model to stand on
            //planarXzGrid = new GridXz(this, new Vector3(0, 0, 0), new Vector2(500, 500), new Vector2(10, 10), Color.Black);
            //Components.Add(planarXzGrid);
            //drawGrid = false;

            worldAxes = new CoordinateCross(this, 500);
            Components.Add(worldAxes);

            // Create the avatar animator
            //animator = new AvatarAnimator(this, RetargetMatrixHierarchyToAvatarMesh, AvateeringXNA.skeletonTranslationScaleFactor);
            //Components.Add(animator);

            

            // Drawing options

            skeletonDetected = true;

            // Setup the depth stream
            depthStream = new DepthStreamRenderer(this);

            // Setup the skeleton stream the same as depth stream 
            skeletonStream = new SkeletonStreamRenderer(this, SkeletonToDepthMap);
            
            // Update Depth and Skeleton Stream size and location based on the back-buffer
            UpdateStreamSizeAndLocation();

            previousKeyboard = Keyboard.GetState();

            previousCollideResult2 = ContainmentType.Disjoint;


            CursorRight = new Cursor(this, "cursor_ball");

            CursorLeft = new Cursor(this, "cursor_ball");

            Video_box2 = new IconBoxVideo(this, "wildlife1");

            iconboxAudio1 = new IconBoxAudio(this, "Kalimba");
            
            iconboxPicture1 = new IconBoxPicture(this, "Lighthouse");
 
            iconboxText1 = new IconBoxText(this);





        }






//=========================================================test start






        protected override void Initialize()
        {
            base.Initialize();
        }





//==========================================================test end


        
        /// Gets the KinectChooser from the services.
        public KinectChooser Chooser
        {
            get
            {
                return (KinectChooser)Services.GetService(typeof(KinectChooser));
            }
        }

        
        /// Gets the SpriteBatch from the services.
        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Services.GetService(typeof(SpriteBatch));
            }
        }

        
        /// Gets or sets the last frames skeleton data.
        private static Skeleton[] SkeletonData { get; set; }


        /// Load the graphics content.
        #region Load Content
        protected override void LoadContent()
        {
            CameraFOV = nominalVerticalFieldOfView;

 

            // Create the spritebatch to draw the 3D items
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            InterfaceBox = new InterfaceBox(this);
            InterfaceBox.LoadContent(Content, "interfacebox2");
            InterfaceBox.Scale = 1.0f;




            //focusedObject = new InterfaceBox(this);

            base.LoadContent();
        }

        #endregion


        #endregion

        #region Update
        /// Allows the game to run logic.
        
        /// <param name="gameTime">The gametime.</param>
        protected override void Update(GameTime gameTime)
        {

            // Update saved state.
            previousKeyboard = currentKeyboard;

            // If the sensor is not found, not running, or not connected, stop now
            if (null == chooser || null == Chooser.Sensor || false == Chooser.Sensor.IsRunning || Chooser.Sensor.Status != KinectStatus.Connected)
            {
                
                return;
            }
                  
            bool newFrame = false;

            using (var skeletonFrame = Chooser.Sensor.SkeletonStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (null != skeletonFrame)
                {
                    newFrame = true;
                    Window.Title = "skeletonFrame  готов";

                    // Reallocate if necessary
                    if (null == SkeletonData || SkeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        SkeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(SkeletonData);

                    // Select the first tracked skeleton we see to avateer
                    Skeleton rawSkeleton =
                        (from s in SkeletonData
                         where s != null && s.TrackingState == SkeletonTrackingState.Tracked
                         select s).FirstOrDefault();

                    //if (null != animator)
                    //{
                        if (null != rawSkeleton)
                        {
                            //animator.CopySkeleton(rawSkeleton);
                            //animator.FloorClipPlane = skeletonFrame.FloorClipPlane;

/////////////////test

                            right_hand_joint = KinectHelper.Position(rawSkeleton, JointType.HandRight);
                            left_hand_joint = KinectHelper.Position(rawSkeleton, JointType.HandLeft);

                            head = KinectHelper.Position(rawSkeleton, JointType.Head);

                            //SkeletonJointsPositionDoubleExponentialFilter.FilterJoint(rawSkeleton, JointType.HandRight, )

                           // var right_hand_joint = rawSkeleton.Joints[JointType.HandRight];
                           // var left_hand_joint = rawSkeleton.Joints[JointType.HandLeft];
                            var right_hand_joint_position = right_hand_joint;
                            var left_hand_joint_position = left_hand_joint;

                            UpdateCursorAction(right_hand_joint_position, left_hand_joint_position);



                            

///////////////////test
                            // Reset the filters if the skeleton was not seen before now
                            if (skeletonDetected == false)
                            {
                                //animator.Reset();
                            }

                            skeletonDetected = true;
                            //animator.SkeletonVisible = true;
                        }
                        else
                        {
                            skeletonDetected = false;
                            //animator.SkeletonVisible = false;
                        }
                    //}
                }
            }

            if (newFrame)
            {
                // Call the stream update manually as they are not a game component
                if (null != depthStream && null != skeletonStream)
                {
                    depthStream.Update(gameTime);
                    skeletonStream.Update(gameTime, SkeletonData);
                }

                // Update the avatar renderer
                //if (null != animator)
                //{
                //   animator.SkeletonDrawn = false;
                //}
            }


            if (cullingModeOn)
            {
                    RasterizerState rasterizator_state = new RasterizerState();
                    rasterizator_state.CullMode = CullMode.None;
                    GraphicsDevice.RasterizerState = rasterizator_state;
            }
            else
            {
                RasterizerState rasterizator_state = new RasterizerState();
                rasterizator_state.CullMode = CullMode.CullCounterClockwiseFace;
                GraphicsDevice.RasterizerState = rasterizator_state;
            }
          
            if (!TextInputEnabled)
            {
                HandleInput();
                UpdateCamera(gameTime);
            }

            
            
            
            
            Collide();


   





            base.Update(gameTime);
        }
        #endregion

        #region Установка камеры
        /// Создаем камеру
        
        protected void UpdateViewingCamera()
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            // Compute camera matrices.
            view = Matrix.CreateTranslation(0, -CameraHeight, 0) *
                   Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                   Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                   Matrix.CreateLookAt(new Vector3(0, 0, cameraDistance), new Vector3(0, 0, 0), Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView (MathHelper.ToRadians(CameraFOV),
                                                                device.Viewport.AspectRatio,
                                                                1,
                                                                10000);
        }
        #endregion

        #region Draw

        protected override void Draw(GameTime gameTime)
        {
            
            // Clear the screen
            GraphicsDevice.Clear(Color.Black);

            UpdateViewingCamera();

            InterfaceBox.Draw();

  
           // iconboxAudio1.Draw();


            //iconboxPicture1.Draw();


            //iconboxText1.Draw();


            // Render the depth and skeleton stream
            if (null != depthStream && null != skeletonStream)
            {
                depthStream.Draw(gameTime);
                skeletonStream.Draw(gameTime);
            }

            // Optionally draw a ground plane grid and world axes that the avatar stands on.
            // For our axes, red is +X, green is +Y, blue is +Z
            //if (drawGrid && null != planarXzGrid && null != worldAxes)
            //{
            //   planarXzGrid.Draw(gameTime, Matrix.Identity, view, projection);
            //   worldAxes.Draw(gameTime, Matrix.Identity, view, projection);
            //}


            // Render header/footer image
            //    SharedSpriteBatch.Begin();
            //    SharedSpriteBatch.Draw(header, Vector2.Zero, null, Color.White);
            //    SharedSpriteBatch.End();



//================================video



           

       
//===============================video endof


            base.Draw(gameTime);
        }
        
        #endregion

        #region Handle Input  /// Обрабока ввода с клавиатуры

        private void HandleInput()
        {
            currentKeyboard = Keyboard.GetState();


            // Set culling mode 
            if (currentKeyboard.IsKeyDown(Keys.F1))
            {
                if (!previousKeyboard.IsKeyDown(Keys.F1))
                {
                    cullingModeOn = !cullingModeOn;
                }
            }
            
            // Check for exit.
            if (currentKeyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Fullscreen on/off toggle
            if (currentKeyboard.IsKeyDown(Keys.F11))
            {
                // If not down last update, key has just been pressed.
                if (!previousKeyboard.IsKeyDown(Keys.F11))
                {
                    fullscreenMode = !fullscreenMode;
                    SetScreenMode();
                }
            }


            // Seated on/off toggle
            if (currentKeyboard.IsKeyDown(Keys.V))
            {
                if (!previousKeyboard.IsKeyDown(Keys.V))
                {
                    chooser.SeatedMode = !chooser.SeatedMode;
                    skeletonDetected = false;
                }
            }

            // Near mode on/off toggle
            if (currentKeyboard.IsKeyDown(Keys.N))
            {
                if (!previousKeyboard.IsKeyDown(Keys.N))
                {
                    chooser.NearMode = !chooser.NearMode;
                    skeletonDetected = false;
  
                }
            }
            
            // Reset the avatar filters (also resets camera)
            if (currentKeyboard.IsKeyDown(Keys.R))
            {
                if (!previousKeyboard.IsKeyDown(Keys.R))
                {

                }
            }

            // Рисовать bounding boxes вкл/выкл
            if (currentKeyboard.IsKeyDown(Keys.B))
            {
                if (!previousKeyboard.IsKeyDown(Keys.B))
                {
                    drawBoundingBoxesOn = !drawBoundingBoxesOn;
                }
            }
        }

        
        /// Toggle between fullscreen and windowed mode
        
        private void SetScreenMode()
        {
            // This sets the display resolution or window size to the desired size
            // If windowed, it also forces a 4:3 ratio for height and adds 110 for header/footer
            if (fullscreenMode)
            {
                foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check our requested FullScreenWidth and Height against each supported display mode and set if valid
                    if ((mode.Width == FullScreenWidth) && (mode.Height == FullScreenHeight))
                    {
                        graphics.PreferredBackBufferWidth = FullScreenWidth;
                        graphics.PreferredBackBufferHeight = FullScreenHeight;
                        graphics.IsFullScreen = true;
                        graphics.ApplyChanges();
                    }
                }
            }
            else
            {
                if (WindowedWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                {
                    graphics.PreferredBackBufferWidth = WindowedWidth;
                    graphics.PreferredBackBufferHeight = ((WindowedWidth / 16) * 9);
                    graphics.IsFullScreen = false;
                    graphics.ApplyChanges();
                }
            }

            UpdateStreamSizeAndLocation();
        }

        
        /// Update the depth and skeleton stream rendering position and size based on the backbuffer resolution.
        
        private void UpdateStreamSizeAndLocation()
        {
            int depthStreamWidth = graphics.PreferredBackBufferWidth / 8;
            Vector2 size = new Vector2(depthStreamWidth, (depthStreamWidth / 4) * 3);
            Vector3 pos = new Vector3(graphics.PreferredBackBufferWidth - depthStreamWidth , 0, 10);

            if (null != depthStream)
            {
                depthStream.Size = size;
                depthStream.Position = pos;
            }

            if (null != skeletonStream)
            {
                skeletonStream.Size = size;
                skeletonStream.Position = pos;
            }
        }

        
        /// Handles camera input.
        
        /// <param name="gameTime">The gametime.</param>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboard.IsKeyDown(Keys.Up) ||
                currentKeyboard.IsKeyDown(Keys.W))
            {
                cameraArc += time * CameraArcIncrement;
            }
            
            if (currentKeyboard.IsKeyDown(Keys.Down) ||
                currentKeyboard.IsKeyDown(Keys.S))
            {
                cameraArc -= time * CameraArcIncrement;
            }

            // Limit the arc movement.
            if (cameraArc > CameraArcAngleLimit)
            {
                cameraArc = CameraArcAngleLimit;
            }
            else if (cameraArc < -CameraArcAngleLimit)
            {
                cameraArc = -CameraArcAngleLimit;
            }

            // Check for input to rotate the camera around the model.
            if (currentKeyboard.IsKeyDown(Keys.Right) ||
                currentKeyboard.IsKeyDown(Keys.D))
            {
                cameraRotation += time * CameraArcIncrement;
            }

            if (currentKeyboard.IsKeyDown(Keys.Left) ||
                currentKeyboard.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * CameraArcIncrement;
            }

            // Check for input to zoom camera in and out.
            if (currentKeyboard.IsKeyDown(Keys.Z))
            {
                cameraDistance += time * CameraZoomIncrement;
            }

            if (currentKeyboard.IsKeyDown(Keys.X))
            {
                cameraDistance -= time * CameraZoomIncrement;
            }

            // Limit the camera distance from the origin.
            if (cameraDistance > CameraMaxDistance)
            {
                cameraDistance = CameraMaxDistance;
            }
            else if (cameraDistance < CameraMinDistance)
            {
                cameraDistance = CameraMinDistance;
            }

            if (currentKeyboard.IsKeyDown(Keys.R))
            {
                cameraArc = 0;
                cameraRotation = 0;
                cameraDistance = CameraStartingTranslation;
            }

            // Check for input to Camera height
            if (currentKeyboard.IsKeyDown(Keys.Up))
            {
                CameraHeight += time * 0.01f;
            }

            if (currentKeyboard.IsKeyDown(Keys.Down))
            {
                CameraHeight -= time * 0.01f;
            }

            // Check for input to Camera FOV
            if (currentKeyboard.IsKeyDown(Keys.O))
            {
                CameraFOV -= time * CameraFOVIncrement;
            }

            if (currentKeyboard.IsKeyDown(Keys.P))
            {
                CameraFOV += time * CameraFOVIncrement;
            }
        }

        #endregion

        #region Helpers

        
        /// This method ensures that we can render to the back buffer without
        /// losing the data we already had in our previous back buffer.  This
        /// is necessary for the SkeletonStreamRenderer.
        
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        
        /// This method maps a SkeletonPoint to the depth frame.
        
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the depth frame.</returns>
        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            // This is used to map a skeleton point to the depth image location
            if (null == chooser || null == Chooser.Sensor || true != Chooser.Sensor.IsRunning || Chooser.Sensor.Status != KinectStatus.Connected)
            {
                return Vector2.Zero;
            }

            var depthPt = chooser.Sensor.MapSkeletonPointToDepth(point, chooser.Sensor.DepthStream.Format);

            // scale to current depth image display size and add any position offset
            float x = (depthPt.X * skeletonStream.Size.X) / chooser.Sensor.DepthStream.FrameWidth;
            float y = (depthPt.Y * skeletonStream.Size.Y) / chooser.Sensor.DepthStream.FrameHeight;

            return new Vector2(x + skeletonStream.Position.X, y + skeletonStream.Position.Y);
        }

        #endregion

        #region Обновление позиции курсора

        private void UpdateCursorAction (Vector3 right_hand_position, Vector3 left_hand_position)
        {

            scaled_right_hand_position.X = right_hand_position.X * 40f;
            scaled_right_hand_position.Y = right_hand_position.Y * 40f + 5.4f ;
            scaled_right_hand_position.Z =  right_hand_position.Z * 50f - 60f;

            scaled_left_hand_position.X = left_hand_position.X * 40f;
            scaled_left_hand_position.Y = left_hand_position.Y * 40f + 5.4f;
            scaled_left_hand_position.Z = left_hand_position.Z * 50f - 60f;

            CursorLeft.Position = new Vector3(scaled_left_hand_position.X, scaled_left_hand_position.Y, scaled_left_hand_position.Z);
            CursorRight.Position = new Vector3(scaled_right_hand_position.X, scaled_right_hand_position.Y, scaled_right_hand_position.Z);

            cursorVectorDirection = previousCursorPosition - CursorRight.Position;
            leftCursorVectorDirection = previousLeftCursorPosition - CursorLeft.Position;
            
          /*  if (keepCursorCoordinateZ)
            {
                CursorRight.Position = new Vector3(scaled_right_hand_position.X, scaled_right_hand_position.Y, focusedObject.Position.Z);
            }  */

           
            previousCursorPosition = CursorRight.Position;
            previousLeftCursorPosition = CursorLeft.Position;
           // Window.Title = "Ускорение X:  " + Math.Round(cursorVectorDirection.X, 3).ToString() 
           //     + "     Ускорение Y:  " + Math.Round(cursorVectorDirection.Y, 3).ToString() 
           //     + "     Ускорение Z:  " + Math.Round(cursorVectorDirection.Z, 3).ToString();
        }

        float ScaleVector(int length, float position)
        {
            float value = ((length / 2.0f) * position) + (length / 2.0f);
            if (value > length)
            {
                return length;
            }
            if (value < 0f)
            {
                return 0f;
            }
            return value;
        }


        #endregion

        /// <summary>
        /// Check each pair of objects for collision/containment and store the results for
        /// coloring them at render time.
        /// </summary>
        private void Collide()
                {
                   

                   

                    



                }




    }
}