#region File Description
//-----------------------------------------------------------------------------
// MainMenu.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HeavyGear.GameLogic;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameScreens
{
    /// <summary>
    /// Main menu
    /// </summary>
    class MainMenu : IGameScreen
    {
        #region Constants
        static readonly Rectangle[] ButtonRects = new Rectangle[]
            {
                new Rectangle(80, 410, 215, 30),
                new Rectangle(80, 455, 150, 30),
                new Rectangle(80, 500, 150, 30),
                new Rectangle(80, 545, 160, 30),
                new Rectangle(80, 590, 160, 30)
            };

        const int NumberOfButtons = 5;

        /// <summary>
        /// The amount of time idle at the menu before returning to the splash screen
        /// </summary>
        const float TimeOutMenu = 60000.0f;
        
        #endregion

        #region Variables
        float selectionTimeOut = 0;
        /// <summary>
        /// Start with button 0 being selected (play game)
        /// </summary>
        int selectedButton = 0;

        /// <summary>
        /// Ignore the mouse unless it moves;
        /// this is so the mouse does not disrupt game pads and keyboard
        /// </summary>
        bool ignoreMouse = true;

        #endregion

        #region Render
        /// <summary>
        /// Interpolate rectangle
        /// </summary>
        /// <param name="rect1">Rectangle 1</param>
        /// <param name="rect2">Rectangle 2</param>
        /// <param name="interpolation">Interpolation</param>
        /// <returns>Rectangle</returns>
        internal static Rectangle InterpolateRect(
            Rectangle rect1, Rectangle rect2,
            float interpolation)
        {
            return new Rectangle(
                (int)Math.Round(
                rect1.X * interpolation + rect2.X * (1 - interpolation)),
                (int)Math.Round(
                rect1.Y * interpolation + rect2.Y * (1 - interpolation)),
                (int)Math.Round(
                rect1.Width * interpolation + rect2.Width * (1 - interpolation)),
                (int)Math.Round(
                rect1.Height * interpolation + rect2.Height * (1 - interpolation)));
        }

        //float pressedLeftMs = 0;
        //float pressedRightMs = 0;
        /// <summary>
        /// Render
        /// </summary>
        /// <returns>Bool</returns>
        public bool Render()
        {
            // This starts both menu and in game post screen shader!
            //BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.MainMenu.RenderOnScreen(BaseGame.ResolutionRect);
            //BaseGame.UI.RenderBlackBar(80, BaseGame.YToRes(640) - 80);

            // Show logos
            // Little helper to keep track if mouse is actually over a button.
            // Required because buttons are selected even when not hovering over
            // them for GamePad support, but we still want the mouse only to
            // be apply when we are actually over the button.
            int mouseIsOverButton = -1;

            // If the user manipulated the mouse, stop ignoring the mouse
            // This allows the mouse to override the game pad or keyboard selection
            if (Input.HasMouseMoved || Input.MouseLeftButtonJustPressed)
                ignoreMouse = false;

            // Show buttons
            // Part 1: Calculate global variables for our buttons

            for (int num = 0; num < NumberOfButtons; num++)
            {
                // Is this button currently selected?
                bool selected = num == selectedButton;

                Rectangle renderRect = new Rectangle( 
                    BaseGame.ResolutionRect.X + BaseGame.XToRes(49),
                    BaseGame.YToRes(ButtonRects[num].Y + 10),
                    BaseGame.XToRes(32), 
                    BaseGame.YToRes(32));
                Rectangle mouseRect = new Rectangle(renderRect.X, BaseGame.YToRes(ButtonRects[num].Y), BaseGame.XToRes(300), BaseGame.YToRes(32));

                if (Input.MouseInBox(mouseRect))
                    mouseIsOverButton = num;

                // Add selection arrow if selected
                if (selected)
                    BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.ArrowGfxRect, Color.Black, MathHelper.ToRadians(90.0f));
                    //BaseGame.UI.Selection.RenderOnScreen(renderRect);
            }

            // Draw label for Map Editor button (not baked into menu texture)
            TextureFont.WriteText(
                BaseGame.ResolutionRect.X + BaseGame.XToRes(75),
                BaseGame.YToRes(ButtonRects[4].Y + 6),
                "Map Editor");

            if (!ignoreMouse && mouseIsOverButton >= 0)
                selectedButton = mouseIsOverButton;
            else if (selectionTimeOut > 200)
            {
                // Handle GamePad input, and also allow keyboard input
                if (Input.GamePadUpPressed || Input.KeyboardUpJustPressed)
                {
                    //Sound.Play(Sound.Sounds);
                    selectedButton =
                        (selectedButton + NumberOfButtons - 1) % NumberOfButtons;
                    ignoreMouse = true;
                    selectionTimeOut = 0;
                }
                else if (Input.GamePadDownPressed || Input.KeyboardDownJustPressed)
                {
                    //Sound.Play(Sound.Sounds.Highlight);
                    selectedButton = (selectedButton + 1) % NumberOfButtons;
                    ignoreMouse = true;
                    selectionTimeOut = 0;
                }
            }
            else
                selectionTimeOut += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;

            // If user presses the mouse button or the game pad A or Space,
            // start the game screen for the currently selected game part.
            if ((Input.MouseLeftButtonJustPressed  && mouseIsOverButton >= 0)|| Input.GamePadAJustPressed || 
                Input.KeyboardSpaceJustPressed || 
                Input.KeyboardKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                // Start game screen
                switch (selectedButton)
                {
                    case 0:
                        //Network - disabled until framework 3.0
#if DEBUG
                        HeavyGearManager.AddGameScreen(new NetworkScreen());
#endif
                        break;
                    case 1:
                        //Local
                        HeavyGearManager.AddGameScreen(new LocalScreen());
                        break;
                    case 2:
                        //Credits
                        HeavyGearManager.AddGameScreen(new CreditsScreen());
                        break;
                    case 3:
                        //Exit
                        HeavyGearManager.ExitGame();
                        break;
                    case 4:
                        //Map Editor
                        HeavyGearManager.AddGameScreen(new EditorScreen());
                        break;
                }
            }

            if (Input.GamePadBackJustPressed || Input.KeyboardEscapeJustPressed)
                return true;

            return false;
        }
        #endregion
    }
}
