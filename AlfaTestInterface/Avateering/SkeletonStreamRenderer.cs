//------------------------------------------------------------------------------
// <copyright file="SkeletonStreamRenderer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Avateering
{
    using System;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A delegate method explaining how to map a SkeletonPoint from one space to another.
    /// </summary>
    /// <param name="point">The SkeletonPoint to map.</param>
    /// <returns>The Vector2 representing the target location.</returns>
    public delegate Vector2 SkeletonPointMap(SkeletonPoint point);

    /// <summary>
    /// This class is responsible for rendering a skeleton stream.
    /// </summary>
    public class SkeletonStreamRenderer : Object2D
    {
        /// <summary>
        /// The last frames skeleton data.
        /// </summary>
        private static Skeleton[] skeletonData;

        /// <summary>
        /// This flag ensures only request a frame once per update call
        /// across the entire application.
        /// </summary>
        private static bool skeletonDrawn = true;

        /// <summary>
        /// This is the map method called when mapping from
        /// skeleton space to the target space.
        /// </summary>
        private readonly SkeletonPointMap mapMethod;

        /// <summary>
        /// The SpriteBatch RasterizerState used for rendering.
        /// </summary>
        private RasterizerState rasterizerState;  

        /// <summary>
        /// The origin (center) location of the joint texture.
        /// </summary>
        private Vector2 jointOrigin;

        /// <summary>
        /// The joint texture.
        /// </summary>
        private Texture2D jointTexture;

        /// <summary>
        /// The origin (center) location of the bone texture.
        /// </summary>
        private Vector2 boneOrigin;
        
        /// <summary>
        /// The bone texture.
        /// </summary>
        private Texture2D boneTexture;

        /// <summary>
        /// Whether the rendering has been initialized.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// Initializes a new instance of the SkeletonStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="map">The method used to map the SkeletonPoint to the target space.</param>
        public SkeletonStreamRenderer(Game game, SkeletonPointMap map)
            : base(game)
        {
            mapMethod = map;
            initialized = false;
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
        /// This method initializes necessary values.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            rasterizerState = new RasterizerState();
            if (null != rasterizerState)
            {
                rasterizerState.ScissorTestEnable = true;
            }

            initialized = true;
        }

        /// <summary>
        /// This method retrieves a new skeleton frame if necessary.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        /// <param name="skeletonFrameData">The skeleton data for the current frame.</param>
        public void Update(GameTime gameTime, Skeleton[] skeletonFrameData)
        {
            base.Update(gameTime);

            if (null == skeletonFrameData)
            {
                // If the sensor is not found, not running, or not connected, stop now
                if (null == Chooser.Sensor || false == Chooser.Sensor.IsRunning
                    || Chooser.Sensor.Status != KinectStatus.Connected)
                {
                    return;
                }

                // If we have already drawn this skeleton, then we should retrieve a new frame
                // This prevents us from calling the next frame more than once per update
                if (skeletonDrawn)
                {
                    using (var skeletonFrame = Chooser.Sensor.SkeletonStream.OpenNextFrame(0))
                    {
                        // Sometimes we get a null frame back if no data is ready
                        if (null == skeletonFrame)
                        {
                            return;
                        }

                        // Reallocate if necessary
                        if (null == skeletonData || skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                        {
                            skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        }

                        skeletonFrame.CopySkeletonDataTo(skeletonData);
                        skeletonDrawn = false;
                    }
                }
            }
            else
            {
                skeletonData = skeletonFrameData;
                skeletonDrawn = false;
            }
        }

        /// <summary>
        /// This method draws the skeleton frame data.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            // If the joint texture isn't loaded, load it now
            if (null == jointTexture)
            {
                LoadContent();
            }

            // If the sensor is not found, not running, or not connected, or if we don't have data, lets return.
            if (null == Chooser.Sensor || false == Chooser.Sensor.IsRunning || Chooser.Sensor.Status != KinectStatus.Connected || null == skeletonData || null == mapMethod || null == SharedSpriteBatch)
            {
                return;
            }

            if (false == initialized)
            {
                Initialize();
            }

            // Set a scissor region so our skeleton does not draw outside the depth image
            Rectangle oldScissorRectangle = GraphicsDevice.ScissorRectangle;
            GraphicsDevice.ScissorRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            SharedSpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, rasterizerState);

            foreach (var skeleton in skeletonData)
            {
                switch (skeleton.TrackingState)
                {
                    case SkeletonTrackingState.Tracked:
                        // Draw Bones
                        DrawBone(skeleton.Joints, JointType.Head, JointType.ShoulderCenter);
                        DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderLeft);
                        DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);

                        DrawBone(skeleton.Joints, JointType.ShoulderLeft, JointType.ElbowLeft);
                        DrawBone(skeleton.Joints, JointType.ElbowLeft, JointType.WristLeft);
                        DrawBone(skeleton.Joints, JointType.WristLeft, JointType.HandLeft);

                        DrawBone(skeleton.Joints, JointType.ShoulderRight, JointType.ElbowRight);
                        DrawBone(skeleton.Joints, JointType.ElbowRight, JointType.WristRight);
                        DrawBone(skeleton.Joints, JointType.WristRight, JointType.HandRight);

                        if (Chooser.SeatedMode == false)
                        {
                            DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.Spine);
                            DrawBone(skeleton.Joints, JointType.Spine, JointType.HipCenter);
                            DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipLeft);
                            DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                            DrawBone(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                            DrawBone(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                            DrawBone(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                            DrawBone(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                            DrawBone(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                            DrawBone(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);
                        }

                        // Now draw the joints
                        foreach (Joint j in skeleton.Joints)
                        {
                            Color jointColor = Color.Green;
                            if (j.TrackingState != JointTrackingState.Tracked)
                            {
                                jointColor = Color.Yellow;
                            }

                            if (null != Chooser && (false == Chooser.SeatedMode || (true == Chooser.SeatedMode && j.JointType >= JointType.ShoulderCenter && j.JointType < JointType.HipLeft)))
                            {
                                SharedSpriteBatch.Draw(
                                    jointTexture,
                                    mapMethod(j.Position),
                                    null,
                                    jointColor,
                                    0.0f,
                                    jointOrigin,
                                    0.7f,   // <- масштаб отрисовки джойнта
                                    SpriteEffects.None,
                                    0.0f);
                            }
                        }

                        break;

                    case SkeletonTrackingState.PositionOnly:
                        // If we are only tracking position, draw a blue dot
                        SharedSpriteBatch.Draw(
                                jointTexture,
                                mapMethod(skeleton.Position),
                                null,
                                Color.Blue,
                                0.0f,
                                jointOrigin,
                                1.0f,
                                SpriteEffects.None,
                                0.0f);
                        break;
                }
            }

            SharedSpriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.ScissorRectangle = oldScissorRectangle;

            skeletonDrawn = true;

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the textures and sets the origin values.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            jointTexture = Game.Content.Load<Texture2D>("Joint");
            jointOrigin = new Vector2(jointTexture.Width / 2, jointTexture.Height / 2);
            
            boneTexture = Game.Content.Load<Texture2D>("Bone");
            boneOrigin = new Vector2(0.5f, 0.0f);
        }

        /// <summary>
        /// This method draws a bone.
        /// </summary>
        /// <param name="joints">The joint data.</param>
        /// <param name="startJoint">The starting joint.</param>
        /// <param name="endJoint">The ending joint.</param>
        private void DrawBone(JointCollection joints, JointType startJoint, JointType endJoint)
        {
            Vector2 start = mapMethod(joints[startJoint].Position);
            Vector2 end = mapMethod(joints[endJoint].Position);
            Vector2 diff = end - start;
            Vector2 scale = new Vector2(1.0f, diff.Length() / boneTexture.Height);

            float angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;

            Color color = Color.LightGreen;
            if (joints[startJoint].TrackingState != JointTrackingState.Tracked ||
                joints[endJoint].TrackingState != JointTrackingState.Tracked)
            {
                color = Color.Gray;
            }

            SharedSpriteBatch.Draw(boneTexture, start, null, color, angle, boneOrigin, scale, SpriteEffects.None, 1.0f);
        }
    }
}
