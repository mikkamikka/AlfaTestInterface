//------------------------------------------------------------------------------
// <copyright file="KinectChooser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// This class will pick a Kinect sensor, if available.
    /// </summary>
    public class KinectChooser : DrawableGameComponent
    {
        /// <summary>
        /// The status to string mapping.
        /// </summary>
        private readonly Dictionary<KinectStatus, string> statusMap = new Dictionary<KinectStatus, string>();

        /// <summary>
        /// The requested color image format.
        /// </summary>
        private readonly ColorImageFormat colorImageFormat;

        /// <summary>
        /// The requested depth image format.
        /// </summary>
        private readonly DepthImageFormat depthImageFormat;

        /// <summary>
        /// The chooser background texture.
        /// </summary>
        private Texture2D chooserBackground;

        /// <summary>
        /// The font for rendering the state text.
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Gets or sets near mode.
        /// </summary>
        private bool nearMode;

        /// <summary>
        /// Gets or sets seated mode.
        /// </summary>
        private bool seatedMode;


        // Some smoothing with little latency (defaults).
        // Only filters out small jitters.
        // Good for gesture recognition in games.
        //const TransformSmoothParameters DefaultParams = {0.5f; 0.5f; 0.5f, 0.05f, 0.04f };

        // Smoothed with some latency.
        // Filters out medium jitters.
        // Good for a menu system that needs to be smooth but
        // doesn't need the reduced latency as much as gesture recognition does.
        //const NUI_TRANSFORM_SMOOTH_PARAMETERS SomewhatLatentParams = { 0.5f, 0.1f, 0.5f, 0.1f, 0.1f };

        // Very smooth, but with a lot of latency.
        // Filters out large jitters.
        // Good for situations where smooth data is absolutely required
        // and latency is not an issue.
        //const NUI_TRANSFORM_SMOOTH_PARAMETERS VerySmoothParams = { 0.7f, 0.3f, 1.0f, 1.0f, 1.0f };




        /// <summary>
        /// Initializes a new instance of the KinectChooser class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="colorFormat">The desired color image format.</param>
        /// <param name="depthFormat">The desired depth image format.</param>
        public KinectChooser(Game game, ColorImageFormat colorFormat, DepthImageFormat depthFormat)
            : base(game)
        {
            colorImageFormat = colorFormat;
            depthImageFormat = depthFormat;

            nearMode = true;
            seatedMode = false;

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            DiscoverSensor();

            statusMap.Add(KinectStatus.Undefined, "Required");
            statusMap.Add(KinectStatus.Connected, string.Empty);
            statusMap.Add(KinectStatus.DeviceNotGenuine, "Device Not Genuine");
            statusMap.Add(KinectStatus.DeviceNotSupported, "Device Not Supported");
            statusMap.Add(KinectStatus.Disconnected, "Required");
            statusMap.Add(KinectStatus.Error, "Error");
            statusMap.Add(KinectStatus.Initializing, "Initializing...");
            statusMap.Add(KinectStatus.InsufficientBandwidth, "Insufficient Bandwidth");
            statusMap.Add(KinectStatus.NotPowered, "Not Powered");
            statusMap.Add(KinectStatus.NotReady, "Not Ready");
        }

        /// <summary>
        /// Gets the SpriteBatch from the services.
        /// </summary>
        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Gets the selected KinectSensor.
        /// </summary>
        public KinectSensor Sensor { get; private set; }

        /// <summary>
        /// Gets the last known status of the KinectSensor.
        /// </summary>
        public KinectStatus LastStatus { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether near mode is enabled.
        /// Near mode enables depth between 0.4 to 3m, default is between 0.8 to 4m.
        /// </summary>
        public bool NearMode
        {
            get
            {
                return nearMode;
            }

            set
            {
                if (null != Sensor && null != Sensor.DepthStream)
                {
                    try
                    {
                        Sensor.DepthStream.Range = value ? DepthRange.Near : DepthRange.Default;   // set near or default mode
                        nearMode = value;
                    }
                    catch (InvalidOperationException)
                    {
                        // not valid for this camera
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether seated mode is enabled for skeletal tracking.
        /// Seated mode tracks only the upper body skeleton,
        /// returning the 10 joints of the arms, shoulders and head.
        /// </summary>
        public bool SeatedMode
        {
            get
            {
                return seatedMode;
            }

            set
            {
                if (null != Sensor && null != Sensor.SkeletonStream)
                {
                    try
                    {
                        Sensor.SkeletonStream.TrackingMode = value ? SkeletonTrackingMode.Seated : SkeletonTrackingMode.Default; // Set seated or default mode
                        seatedMode = value;
                    }
                    catch (InvalidOperationException)
                    {
                        // not valid for this camera
                    }
                }
            }
        }

        /// <summary>
        /// This method renders the current state of the KinectChooser.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            // If the background is not loaded, load it now
            if (null == chooserBackground)
            {
                LoadContent();
            }

            if (null == SharedSpriteBatch)
            {
                return;
            }

            // If we don't have a sensor, or the sensor we have is not connected
            // then we will display the information text
            if (null == Sensor || LastStatus != KinectStatus.Connected)
            {
                SharedSpriteBatch.Begin();

                // Render the background
                SharedSpriteBatch.Draw(
                    chooserBackground,
                    new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2),
                    null,
                    Color.White,
                    0,
                    new Vector2(chooserBackground.Width / 2, chooserBackground.Height / 2),
                    1,
                    SpriteEffects.None,
                    0);

                // Determine the text
                string txt = statusMap[KinectStatus.Undefined];
                if (Sensor != null)
                {
                    txt = statusMap[LastStatus];
                }

                // Render the text
                Vector2 size = font.MeasureString(txt);
                SharedSpriteBatch.DrawString(
                    font,
                    txt,
                    new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, (Game.GraphicsDevice.Viewport.Height / 2) + size.Y),
                    Color.White);
                SharedSpriteBatch.End();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the textures and fonts.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            chooserBackground = Game.Content.Load<Texture2D>("ChooserBackground");
            font = Game.Content.Load<SpriteFont>("Segoe16");
        }

        /// <summary>
        /// This method ensures that the KinectSensor is stopped before exiting.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // Always stop the sensor when closing down
            if (null != Sensor)
            {
                Sensor.Stop();
            }
        }

        /// <summary>
        /// This method will use basic logic to try to grab a sensor.
        /// Once a sensor is found, it will start the sensor with the
        /// requested options.
        /// </summary>
        private void DiscoverSensor()
        {
            // Grab any available sensor
            Sensor = KinectSensor.KinectSensors.FirstOrDefault();

            if (null != Sensor)
            {
                LastStatus = Sensor.Status;

                // If this sensor is connected, then enable it
                if (LastStatus == KinectStatus.Connected)
                {
                    // For many applications we would enable the
                    // automatic joint smoothing, however, in this
                    // Avateering sample, we perform skeleton joint
                    // position corrections, so we will manually
                    // filter when these are complete.

                    // Typical smoothing parameters for the joints:
                     var parameters = new TransformSmoothParameters
                     {
                     Smoothing = 0.7f,
                     Correction = 0.3f,
                     Prediction = 0.9f,
                     JitterRadius = 0.9f,
                     MaxDeviationRadius = 0.6f 
                     };
                    Sensor.SkeletonStream.Enable(parameters);     // <-- задаем параметры сглаживания
                    Sensor.ColorStream.Enable(colorImageFormat);
                    Sensor.DepthStream.Enable(depthImageFormat);
                    Sensor.SkeletonStream.EnableTrackingInNearRange = true;  // Enable skeleton tracking in near mode

                    try
                    {
                        Sensor.Start();
                    }
                    catch (InvalidOperationException)
                    {
                        // sensor is in use by another application
                        // will treat as disconnected for display purposes
                        Sensor = null;
                    }
                }
            }
            else
            {
                LastStatus = KinectStatus.Disconnected;
            }
        }

        /// <summary>
        /// This wires up the status changed event to monitor for 
        /// Kinect state changes.  It automatically stops the sensor
        /// if the device is no longer available.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // If the status is not connected, try to stop it
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }

            LastStatus = e.Status;
            DiscoverSensor();
        }
    }
}
