#region File Description
//-----------------------------------------------------------------------------
// Camera2D.cs
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
    public class Camera2D
    {
        #region Fields
        private Vector2 positionValue;
        private Point mapPosition;
        private bool cameraChanged;
        
        #endregion

        #region Public Properties
        /// <summary>
        /// Get/Set the postion value of the camera
        /// </summary>
        public Vector2 Position
        {
            set
            {
                if (positionValue != value)
                {
                    cameraChanged = true;
                    positionValue = value;
                    BaseGame.ConvertPixelToMap(ref positionValue, out mapPosition);
                }
            }
            get { return positionValue; }
        }
        /// <summary>
        /// Gets the camera's position in map coordinates or center camera on a certain map position
        /// </summary>
        public Point MapPosition
        {
            get
            {
                return mapPosition;
            }
            set
            {
                cameraChanged = true;
                mapPosition = value;
                BaseGame.ConvertMapToPixel(ref mapPosition, out positionValue);
            }
        }

        public void SetMapPosition(Vector2 pixelPosition)
        {
        }

        /// <summary>
        /// Gets whether or not the camera has been changed since the last
        /// ResetChanged call
        /// </summary>
        public bool IsChanged
        {
            get { return cameraChanged; }
            set { cameraChanged = value; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create a new Camera2D
        /// </summary>
        public Camera2D()
        {
            positionValue = Vector2.Zero;
        }
        #endregion

        #region Movement Methods
        
        /// <summary>
        /// Used to inform the camera that new values are updated by the application.
        /// </summary>
        public void ResetChanged()
        {
            cameraChanged = false;
        }

        /// <summary>
        /// Pan in the right direction.
        /// </summary>
        public void MoveRight(float dV)
        {
            cameraChanged = true;
            float zoomValue = HeavyGearManager.Map.ZoomValue;
            positionValue.X+=dV;
            if (positionValue.X > HeavyGearManager.Map.Width * (BaseGame.TileScale.X / zoomValue))
                positionValue.X = HeavyGearManager.Map.Width * (BaseGame.TileScale.X / zoomValue);
        }
        /// <summary>
        /// Pan in the left direction.
        /// </summary>
        public void MoveLeft(float dV)
        {
            cameraChanged = true;
            positionValue.X-= dV;
            if (positionValue.X < -1)
                positionValue.X = -1;
        }
        /// <summary>
        /// Pan in the up direction.
        /// </summary>
        public void MoveUp(float dV)
        {
            cameraChanged = true;
            positionValue.Y-= dV;
            if (positionValue.Y < -1 )
                positionValue.Y = -1;
        }
        /// <summary>
        /// Pan in the down direction.
        /// </summary>
        public void MoveDown(float dV)
        {
            float zoomValue = HeavyGearManager.Map.ZoomValue;
            cameraChanged = true;
            positionValue.Y+=dV;
            if (positionValue.Y > HeavyGearManager.Map.Height * (BaseGame.TileScale.Y / zoomValue))
                positionValue.Y = HeavyGearManager.Map.Height * (BaseGame.TileScale.Y / zoomValue);
        }
        #endregion
    }
}