using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Samples.Kinect.Avateering
{
    public class Timer
    {
        #region Fields

        public bool IsActive
        {
            get { return CurrentDuration < TotalDuration; }
        }

        public float PercentComplete
        {
            get { return (float) (CurrentDuration/TotalDuration); }
        }

        public float CurrentDuration;
        public float TotalDuration;

        #endregion

        #region Initialization

        /// <summary>
        /// Start the timer
        /// </summary>
        /// <param name="seconds"></param>
        public void Start(float milliseconds)
        {
            CurrentDuration = 0.0f;
            TotalDuration = milliseconds;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Update the timer
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            CurrentDuration += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentDuration > TotalDuration)
            {
                CurrentDuration = TotalDuration;
            }
        }

        #endregion
    }
}