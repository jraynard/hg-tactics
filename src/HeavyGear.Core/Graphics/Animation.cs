#region File Description
//-----------------------------------------------------------------------------
// Animation.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace HeavyGear.Graphics
{
    /// <summary>
    /// Animation class holds an animation in the form of multiple textures and a set rate to cycle
    /// through them
    /// </summary>
    public class Animation
    {
        #region Fields
        private Rectangle gfxRect;
        private int frameIndex;
        private int lastFrame;
        private float frameTime;
        private float currentTime;
        private int totalFrames;
        private bool done = false;
        #endregion

        #region Properties
        public Rectangle AnimationRect
        {
            get
            {
                return gfxRect;
            }
        }
        public int FrameIndex
        {
            get
            {
                return frameIndex;
            }
        }
        public bool Done
        {
            get
            {
                return done;
            }
        }
        #endregion

        public void NextFrame()
        {
            frameIndex = (frameIndex + 1) % 4;
        }

        public void Start()
        {
            frameIndex = 0;
            currentTime = 0;
            totalFrames = 0;
            done = false;
        }

        /// <summary>
        /// Returns true if animation still running, false if not
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            if (done)
                return false;

            if (totalFrames == lastFrame)
            {
                done = true;
                totalFrames = 0;
                currentTime = 0;
                return false;
            }

            if (currentTime > frameTime)
            {
                NextFrame();
                currentTime = 0;
                totalFrames++;
            }
            else
                currentTime += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;

            return true;
        }

        #region Contructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frames">The textures to be used in this animation, in order from first to last</param>
        /// <param name="animationTime">The total time this animation should play in milliseconds</param>
        /// <param name="frameRate">The total time each frame should play in milliseconds</param>
        public Animation(Rectangle gfxRect, int lastFrame, float frameTime)
        {
            this.gfxRect = gfxRect;
            this.lastFrame = lastFrame;
            this.frameTime = frameTime;
            this.currentTime = 0;
            this.totalFrames = 0;
            this.done = false;
        }
        #endregion

    }
}
