//------------------------------------------------------------------------------
// <copyright file="CoordinateCross.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// CoordinateCross Class - draws a CoordinateCross in the current coordinate system
    /// XNA uses a right hand coordinate system
    /// +X (right) is Red 
    /// +Y (up) is Green 
    /// +Z (forward) is Blue 
    /// </summary>
    public class CoordinateCross : DrawableGameComponent
    {
        /// <summary>
        /// The number of line vertices to draw.
        /// </summary>
        private const int NumberVertices = 6;

        /// <summary>
        /// This is the array of 3D vertices with associated colors.
        /// </summary>
        private VertexPositionColor[] localAxesVertices;

        /// <summary>
        /// This is the XNA BasicEffect we use to draw.
        /// </summary>
        private BasicEffect effect;

        /// <summary>
        /// Initializes a new instance of the CoordinateCross class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="axisLength">The length of the axis in 3D units.</param>
        public CoordinateCross(Game game, float axisLength)
            : base(game)
        {
            CreateCoordinateCross(axisLength);
        }

        /// <summary>
        /// Initializes a new instance of the Coordinate Cross class.
        /// </summary>
        /// <param name="axisLength">The length of the axis in 3D units.</param>
        public void CreateCoordinateCross(float axisLength)
        {
            if (0.0f == axisLength)
            {
                return;
            }

            localAxesVertices = new VertexPositionColor[NumberVertices];

            // Create Coordinate axes
            // X is red
            localAxesVertices[0] = new VertexPositionColor(Vector3.Zero, Color.Red);
            localAxesVertices[1] = new VertexPositionColor(new Vector3(axisLength, 0, 0), Color.Red);    // right

            // Y is green
            localAxesVertices[2] = new VertexPositionColor(Vector3.Zero, Color.Green);
            localAxesVertices[3] = new VertexPositionColor(new Vector3(0, axisLength, 0), Color.Green);  // up

            // Z is blue
            localAxesVertices[4] = new VertexPositionColor(Vector3.Zero, Color.Blue);
            localAxesVertices[5] = new VertexPositionColor(new Vector3(0, 0, axisLength), Color.Blue);   // forward
        }

        /// <summary>
        /// This method renders the current state.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        public void Draw(GameTime gameTime, Matrix world, Matrix view, Matrix projection)
        {
            if (null == localAxesVertices || 0 == NumberVertices)
            {
                return;
            }

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw grid vertices as line list
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, localAxesVertices, 0, NumberVertices / 2);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the basic effect used for drawing.
        /// </summary>
        protected override void LoadContent()
        {
            effect = new BasicEffect(Game.GraphicsDevice);
            if (null == effect)
            {
                throw new InvalidOperationException("Error creating Basic Effect shader.");
            }

            effect.VertexColorEnabled = true;

            base.LoadContent();
        }
    }
}
