using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Samples.Kinect.Avateering
{
    public class WindowMedia : Environment
    {
        public string MediaType { get; set; }

        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        protected static Timer timer;
        protected static bool GestureAnyIsActive;
        public bool OnWindowPicked;

        public bool WindowIsActive;
        public bool WindowIsOnFront;

        protected float gestureBringToBackAccelFactor = 1.3f;
        protected float gestureScaleAccelerationFactor = 1.0f;
        protected float gestureCloseRightHandAccelFactor = 0.6f;
        protected float gestureCloseLeftHandAccelFactor = -0.3f;



        public WindowMedia(Game game)
            : base(game)
        {
            MediaType = null;
        }


        protected override void LoadContent()
        {
            base.LoadContent();            
            BoundingBox = new BoundingBox();
            debugDraw = new DebugDraw(Game.GraphicsDevice);
        }


        public override void Update(GameTime gameTime)
        {
            CollideWindow();
            DetectDragGesture();
            DetectWindowOnFront();
            base.Update(gameTime);

            
        }

        public void StepBack()
        {
            AvateeringXNA.TextInputEnabled = false;
            for (int i = 0; i < AvateeringXNA.AllMediaWindows.Count; i++)
            {
                AvateeringXNA.AllMediaWindows[i].Position.Z -=  3f;
                //AvateeringXNA.AllMediaWindows[i].WindowIsActive = false;
            }
        }
        
        public void StepForward()
        {
            if (AvateeringXNA.AllMediaWindows.Count > 0)
            {
                if (AvateeringXNA.AllMediaWindows.Max(z => z.Position.Z) < AvateeringXNA.FrontPositionZ + 5f)
                {
                    for (int i = 0; i < AvateeringXNA.AllMediaWindows.Count; i++)
                    {
                        AvateeringXNA.AllMediaWindows[i].Position.Z += 3f;
                        //AvateeringXNA.AllMediaWindows[i].WindowIsActive = false;
                    }
                }
            }
        }


        public void DetectWindowOnFront()
        {
            if (AvateeringXNA.AllMediaWindows.Count > 0)
            {
                float max = AvateeringXNA.AllMediaWindows.Max(z => z.Position.Z);
               // Game.Window.Title = max.ToString(); 

            /*    if (Position.Z >= max)
                {
                    WindowIsActive = true;
                    WindowIsOnFront = true;
                }
                else
                {
                    WindowIsActive = false;
                    WindowIsOnFront = false;
                }
            */    
                
                for (int i = 0; i < AvateeringXNA.AllMediaWindows.Count; i++)
                {
                    if (AvateeringXNA.AllMediaWindows[i].Position.Z >= max)
                    {
                        AvateeringXNA.AllMediaWindows[i].WindowIsActive = true;
                        AvateeringXNA.AllMediaWindows[i].WindowIsOnFront = true;
                    }
                    else
                    {
                        AvateeringXNA.AllMediaWindows[i].WindowIsActive = false;
                        AvateeringXNA.AllMediaWindows[i].WindowIsOnFront = false;
                    }
                }
         
            
            }
           
            

        }



        public void DetectDragGesture()
        {

                if (AvateeringXNA.head.Y < AvateeringXNA.left_hand_joint.Y)
                {
                    //if (Math.Abs(Position.Z - AvateeringXNA.CursorRight.Position.Z) < 0.5f)
                   // {
                        if (OnWindowPicked)
                        {
                            DragWindow();
                        }
                   // }
                }
        }
 

        public void DragWindow()
        {   
            
            //Position.X += AvateeringXNA.CursorRightDelta.X;
            //Position.Y += AvateeringXNA.CursorRightDelta.Y;
            //Position.Z = AvateeringXNA.CursorRight.Position.Z;
            
            Vector3 centerOfWindow = AvateeringXNA.CursorLeft.boundingBox.Min + (AvateeringXNA.CursorLeft.boundingBox.Max - AvateeringXNA.CursorLeft.boundingBox.Min) / 2;
            Position = AvateeringXNA.CursorRight.Position;
        }

        public void CollideWindow()
        {
            collideResults3 = BoundingBox.Contains(AvateeringXNA.CursorRight.boundingBox);
            collideResults4 = BoundingBox.Contains(AvateeringXNA.CursorLeft.boundingBox);

            

            switch (collideResults3)
            {
                case ContainmentType.Contains:
                    break;
                case ContainmentType.Disjoint:
                    OnWindowPicked = false;
                    break;
                case ContainmentType.Intersects:
                    {
                        OnWindowPicked = true;
                        
                    }
                    break;
            }
            previousCollideResult3 = collideResults3;
        }
        


    }
}
