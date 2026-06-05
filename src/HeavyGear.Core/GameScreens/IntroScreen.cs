#region File Description
//-----------------------------------------------------------------------------
// IntroScreen.cs
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
    class IntroScreen : IGameScreen
    {
        #region RenderSplashScreen
        /// <summary>
        /// Render splash screen
        /// </summary>
        public bool Render()
        {
            //ShadowMapShader.PrepareGameShadows();

            // Render background and black bar
            //BaseGame.UI.RenderMenuBackground();
            //BaseGame.UI.RenderBlackBar(518, 61);

            // Clicking or pressing start will go to the menu
            return Input.GamePadStartPressed || Input.GamePadStartPressed ||
                Input.KeyboardSpaceJustPressed || Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
        }
        #endregion
    }
}
