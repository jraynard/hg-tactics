#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
using HeavyGear.GameScreens;
#endregion

namespace HeavyGear.GameLogic
{
    public enum ProjectileType
    {
        Bullet,
        Rocket,
        Missile,
        Grenade
    }

    public class Projectile
    {
        #region Constants
        private const float MoveRate = 1000f;
        #endregion

        #region Variables
        private Vector2 origin;
        private Vector2 target;
        private Vector2 currentPosition;
        private float xSpeed = 0f;
        private float ySpeed = 0f;
        /// <summary>
        /// the animation for this projectile
        /// </summary>
        private int frame = 0;
        /// <summary>
        /// The time that has elapsed since the last frame
        /// </summary>
        private float frameTime = 0.0f;
        private float frameRate = 100.0f;
        /// <summary>
        /// The rate at which the projetile's position is changed
        /// </summary>
        
        /// <summary>
        /// Type of Projectile this is
        /// </summary>
        private ProjectileType projectileType;
        /// <summary>
        /// Whether projectile has reached target
        /// </summary>
        private bool done = false;
        private float rotation = 0.0f;
        
        #endregion

        #region Properties
       
        public ProjectileType ProjectileType
        {
            get
            {
                return this.projectileType;
            }
        }

        public bool Done
        {
            get
            {
                return done;
            }
        }

        public Vector2 CurrentPosition
        {
            get
            {
                return currentPosition;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create car physics controller
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        public Projectile(Vector2 origin, Vector2 target, ProjectileType projectileType)
        {
            this.origin = origin;
            this.target = target;
            this.currentPosition = origin;
            this.projectileType = projectileType;
            float distance = (float)Math.Sqrt(Math.Pow(target.X - origin.X, 2) + Math.Pow(target.Y - origin.Y, 2));
            xSpeed = (target.X - origin.X) / distance;
            ySpeed = (target.Y - origin.Y) / distance;

        }
        #endregion

        #region Draw
        public void Draw()
        {
            //get the camera position relative to the projectile's map position
            Vector2 position;
            Vector2 camera = HeavyGearManager.Map.Origin;
            Vector2.Subtract(ref currentPosition, ref camera, out position);
            float zoomValue = HeavyGearManager.Map.ZoomValue;

            Vector2 unitOffset = new Vector2();
            unitOffset.X = BaseGame.UnitOffset.X / zoomValue;
            unitOffset.Y = BaseGame.UnitOffset.Y / zoomValue;

            //center the projectile
            Vector2.Add(ref position, ref unitOffset, out position);

            Rectangle renderRect = new Rectangle(
                (int)position.X, 
                (int)position.Y, 
                (int)(BaseGame.TileHeight / zoomValue), 
                (int)(BaseGame.TileHeight / zoomValue)
                );

            Projectiles.Draw(projectileType, renderRect, rotation, frame);
        }
        #endregion

        #region Update
        /// <summary>
        /// Update game logic for our projectile.
        /// </summary>
        public virtual void Update()
        {
            if (!done)
            {
                if (Math.Abs(currentPosition.X - target.X) < BaseGame.TileWidth / 2 && Math.Abs(currentPosition.Y - target.Y) < BaseGame.TileHeight / 2)
                    done = true;
                //Update the projectile's position and frame every 50 ms
                if (frameTime >= frameRate)
                {
                    frame = (frame + 1) % 4;
                    frameTime = 0;
                }
                else
                    frameTime += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;

                currentPosition.X = currentPosition.X + BaseGame.MoveFactorPerSecond * MoveRate * xSpeed;
                currentPosition.Y = currentPosition.Y + BaseGame.MoveFactorPerSecond * MoveRate * ySpeed;
            }
        }
        #endregion

        public static Projectile Bullet(Vector2 origin, Vector2 target)
        {
            return new Projectile(origin, target, ProjectileType.Bullet);
        }

        public static Projectile Rocket(Vector2 origin, Vector2 target)
        {
            return new Projectile(origin, target, ProjectileType.Rocket);
        }

        public static Projectile Missile(Vector2 origin, Vector2 target)
        {
            return new Projectile(origin, target, ProjectileType.Missile);
        }

        public static Projectile Grenade(Vector2 origin, Vector2 target)
        {
            return new Projectile(origin, target, ProjectileType.Grenade);
        }

    }
}
