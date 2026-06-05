#region File Description
//-----------------------------------------------------------------------------
// Projectiles.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameLogic
{
    public static class Projectiles
    {
        public static Rectangle[] animationRects = new Rectangle[]
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(0, 32, 32, 32),
                new Rectangle(32, 32, 32, 32)
            };

        public static Rectangle[] gfxRects = new Rectangle[]
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(0, 64, 64, 64),
                new Rectangle(64, 64, 64, 64)
            };
          
        #region Projectiles

        public static void Draw(ProjectileType type, Rectangle renderRect, float rotation, int frame)
        {
            Rectangle sourceRect = animationRects[frame];
            sourceRect.X += gfxRects[(int)type].X;
            sourceRect.Y += gfxRects[(int)type].Y;

            BaseGame.UI.Projectile.RenderOnScreen(renderRect, sourceRect, Color.White, rotation);
        }

        

        #endregion
    }
}
