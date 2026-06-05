#region File Description
//-----------------------------------------------------------------------------
// AnimatedSprite.cs
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
    public class AnimatedSprite
    {
        #region Fields
        private Texture texture;
        private Vector2 positionValue;
        private Vector2 originValue;
        private float rotationValue;
        private Color colorValue = Color.White;
        #endregion

        #region Public Properties
        public Texture Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }
        public Vector2 Position
        {
            set
            {
                positionValue = value;
            }
            get
            {
                return positionValue;
            }
        }

        public Vector2 Origin
        {
            set
            {
                originValue = value;
            }
            get
            {
                return originValue;
            }
        }

        public float Rotation
        {
            set
            {
                rotationValue = value;
            }
            get
            {
                return rotationValue;
            }
        }
        public Color Color
        {
            set
            {
                colorValue = value;
            }
            get
            {
                return colorValue;
            }
        }
        #endregion

        #region Contructors
        public AnimatedSprite(Texture texture)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }

            this.texture = texture;
        }
        public AnimatedSprite() //Empty constructor for projectiles
        {
        }
        #endregion

        #region Draw

        public virtual void Draw(Rectangle ?sourceRect)
        {
            texture.RenderSpriteOnScreen(positionValue, sourceRect, colorValue, MathHelper.ToRadians(rotationValue), originValue);
        }
        #endregion
    }
}
