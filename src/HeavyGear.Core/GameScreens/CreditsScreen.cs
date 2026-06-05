#region File Description
//-----------------------------------------------------------------------------
// CreditsScreen.cs
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
    class CreditsScreen : IGameScreen
    {
        #region RenderSplashScreen
        /// <summary>
        /// Render splash screen
        /// </summary>
        public bool Render()
        {
            // Render background and black bar
            BaseGame.UI.CreditsScreen.RenderOnScreen(BaseGame.ResolutionRect);
            
            // Clicking or pressing start will go to the menu
            return Input.GamePadStartPressed || Input.GamePadAJustPressed || Input.GamePadBackJustPressed || Input.GamePadBJustPressed ||
                Input.MouseLeftButtonJustPressed || Input.MouseRightButtonJustPressed ||
                Input.KeyboardSpaceJustPressed || Input.KeyboardEscapeJustPressed ||
                Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
        }
        #endregion
    }
}
