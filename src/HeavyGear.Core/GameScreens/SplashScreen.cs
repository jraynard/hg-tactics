#region File Description
//-----------------------------------------------------------------------------
// SplashScreen.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using HeavyGear.Graphics;
using HeavyGear.GameLogic;
using Microsoft.Xna.Framework;
#endregion

namespace HeavyGear.GameScreens
{
    /// <summary>
    /// Splash screen
    /// </summary>
    class SplashScreen : IGameScreen
    {
        #region RenderSplashScreen
        /// <summary>
        /// Render splash screen
        /// </summary>
        public bool Render()
        {
            //ShadowMapShader.PrepareGameShadows();

            // Render background and black bar
            BaseGame.UI.TitleScreen.RenderOnScreen(BaseGame.ResolutionRect);
            //BaseGame.UI.RenderBlackBar(518, 61);

            // Show Press Start to continue. 
            /*Rectangle startRect =
                new Rectangle(
                    BaseGame.XToRes(512) - UIRenderer.PressStartGfxRect.Width / 2,
                    BaseGame.YToRes(620) - UIRenderer.PressStartGfxRect.Height,
                    UIRenderer.PressStartGfxRect.Width,
                    UIRenderer.PressStartGfxRect.Height);
            */
            //if ((int)(BaseGame.TotalTime / 0.375f) % 3 != 0)
                //BaseGame.UI.Headers.RenderOnScreen(startRect, UIRenderer.PressStartGfxRect);
                    //BaseGame.CalcRectangleCenteredWithGivenHeight(
                    //512, 518 + 61 / 2, 26, UIRenderer.PressStartGfxRect),
                    //UIRenderer.PressStartGfxRect);

            // Clicking or pressing any button or key will go to menu
            return Input.GamePadStartPressed || Input.GamePadStartPressed || Input.GamePadAJustPressed || Input.GamePadBJustPressed ||
                Input.GamePadXJustPressed || Input.GamePadYJustPressed ||
                Input.MouseLeftButtonJustPressed || Input.MouseRightButtonJustPressed ||
                Input.KeyboardSpaceJustPressed || Input.Keyboard.GetPressedKeys().Length > 0;
        }
        #endregion
    }
}
