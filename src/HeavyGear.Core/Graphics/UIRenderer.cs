#region File Description
//-----------------------------------------------------------------------------
// UIRenderer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified By
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HeavyGear.GameLogic;
using HeavyGear.Helpers;
using HeavyGear.GameScreens;
using HeavyGear.Shaders;
#endregion

namespace HeavyGear.Graphics
{
    /// <summary>
    /// Helper class to render all UI 2D stuff. Rendering textures is very easy
    /// with XNA using the Sprite class, but we still have to create
    /// SpriteBatches. This class helps and handles everything for us.
    /// </summary>
    public class UIRenderer : IDisposable
    {
        #region Constants
        /// <summary>
        /// Graphic rectangles for displaying UI stuff.
        /// </summary>
        public readonly static Rectangle
            UnitIdleGfxRect = new Rectangle(0, 0, 275, 275),
            UnitDestroyedGfxRect = new Rectangle(275, 0, 275, 275),
            UnitIconGfxRect = new Rectangle(550, 0, 275, 275),
            UnitMoveGfxRect = new Rectangle(0, 275, 550, 550),
            UnitHitGfxRect = new Rectangle(550, 275, 550, 550),
            UnitDestroyGfxRect = new Rectangle(0, 825, 550, 550),
            UnitFireGfxRect = new Rectangle(550, 825, 550, 550),
            HexTileGfxRect = new Rectangle(0, 0, 148, 128),
            HexTileHighlightGfxRect = new Rectangle(148, 0, 148, 128),
            HexTileCursorGfxRect = new Rectangle(0, 128, 296, 256),
            PortraitCheetahGfxRect = new Rectangle(0, 0, 275, 275),
            PortraitHunterGfxRect = new Rectangle(275, 0, 275, 275),
            PortraitJaguarGfxRect = new Rectangle(550, 0, 275, 275),
            PortraitGrizzlyGfxRect = new Rectangle(825, 0, 275, 275),
            PortraitMammothGfxRect = new Rectangle(0, 275, 275, 275),
            PortraitScorpionGfxRect = new Rectangle(275, 275, 275, 275),
            PortraitAllerGfxRect = new Rectangle(550, 275, 275, 275),
            PortraitInfantryGfxRect = new Rectangle(825, 275, 275, 275),
            PortraitIguanaGfxRect = new Rectangle(0, 550, 275, 275),
            PortraitJagerGfxRect = new Rectangle(275, 550, 275, 275),
            PortraitBlackMambaGfxRect = new Rectangle(550, 550, 275, 275),
            PortraitSpittingCobraGfxRect = new Rectangle(825, 550, 275, 275),
            PortraitNagaGfxRect = new Rectangle(0, 825, 275, 275),
            PortraitTitanGfxRect = new Rectangle(275, 825, 275, 275),
            PortraitStonewallGfxRect = new Rectangle(550, 825, 275, 275),
            MenuBackgroundGfxRect = new Rectangle(0, 0, 1024, 640),
            MessageBoxGfxRect = new Rectangle(1024, 0, 456, 182),
            MenuItemGfxRect = new Rectangle(1024, 182, 206, 56),
            MenuItemSelectGfxRect = new Rectangle(1230, 182, 206, 56),
            UnitInfoBarGfxRect = new Rectangle(0, 640, 800, 95),
            WeaponInfoBarGfxRect = new Rectangle(270, 640, 530, 95),
            SelectionGfxRect = new Rectangle(800, 640, 32, 32),
            ArrowGfxRect = new Rectangle(833, 640, 30, 32),
            MenuColorGfxRect = new Rectangle(864, 640, 18, 18);
        

        /// <summary>
        /// In game gfx rects. Tacho is our speedometer we see on the screen.
        /// </summary>
        //public static readonly Rectangle
        //    PlayerGfxRect = new Rectangle(381, 132, 222, 160),
        //    UnitGfxRect = new Rectangle(726, 2, 282, 62);
        #endregion

        #region Variables
        Texture[][] unit;
        Texture projectile;
        /// <summary>
        /// Menu Background
        /// </summary>
        Texture mainMenu;
        /// <summary>
        /// Credits screen
        /// </summary>
        Texture creditsScreen;
        /// <summary>
        /// Title screen
        /// </summary>
        Texture titleScreen;
        /// <summary>
        /// Unit ranking emblems
        /// </summary>
        Texture ranking;
        /// <summary>
        /// All hexagon graphics including cursor
        /// </summary>
        Texture hex;
        /// <summary>
        /// Unit portrait for menu
        /// </summary>
        Texture portrait;
        /// <summary>
        /// Textures for game menus
        /// </summary>
        Texture menuUI;
        /// <summary>
        /// Mouse cursor
        /// </summary>
        Texture mouseCursor;
        /// <summary>
        /// Font
        /// </summary>
        TextureFont font;

        #endregion

        #region Properties
        public Texture CreditsScreen
        {
            get
            {
                return creditsScreen;
            }
        }
        /// <summary>
        /// Main menu screen
        /// </summary>
        public Texture MainMenu
        {
            get
            {
                return mainMenu;
            }
        }
        /// <summary>
        /// Game menu graphics
        /// </summary>
        public Texture MenuUI
        {
            get
            {
                return menuUI;
            }
        }
        /// <summary>
        /// Menu portrait graphics
        /// </summary>
        public Texture Portrait
        {
            get
            {
                return portrait;
            }
        }
        /// <summary>
        /// Title Screen background
        /// </summary>
        public Texture TitleScreen
        {
            get
            {
                return titleScreen;
            }
        }

        //These textures are loaded by gamescreen
        public Texture[][] Unit
        {
            get
            {
                return unit;
            }
            set
            {
                unit = value;
            }
        }

        public Texture Projectile
        {
            get
            {
                return projectile;
            }
            set
            {
                projectile = value;
            }
        }
        public Texture Ranking
        {
            get
            {
                return ranking;
            }
            set
            {
                ranking = value;
            }
        }
        /// <summary>
        /// A transparent hex tile, used to lay out the hex grid on screen
        /// </summary>
        public Texture Hex
        {
            get
            {
                return hex;
            }
            set
            {
                hex = value;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create user interface renderer
        /// </summary>
        public UIRenderer()
        {
            menuUI = new Texture("MenuUI.png");
            mouseCursor = new Texture("MouseCursor.png");
            font = new TextureFont();

            creditsScreen = new Texture("credits.png");
            mainMenu = new Texture("MainMenu.png");
            titleScreen = new Texture("TitleScreen.png");

            portrait = new Texture("Portraits.png");
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (unit != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            unit[i][j].Dispose();
                        }
                        unit[i] = null;
                    }
                        
                }
                if (projectile != null)
                    projectile.Dispose();
                if (creditsScreen != null)
                    creditsScreen.Dispose();
                if (ranking != null)
                    ranking.Dispose();
                if (hex != null)
                    hex.Dispose();
                if (mainMenu != null)
                    mainMenu.Dispose();
                if (mouseCursor != null)
                    mouseCursor.Dispose();
                if (font != null)
                    font.Dispose();
                if (menuUI != null)
                    menuUI.Dispose();
                if (titleScreen != null)
                    titleScreen.Dispose();
                if (portrait != null)
                    portrait.Dispose();
            }
        }
        #endregion

        #region Add time fadeup effect
        /// <summary>
        /// Time fadeup modes
        /// </summary>
        public enum TimeFadeupMode
        {
            Plus,
            Minus,
            Normal,
        }

        class TimeFadeupText
        {
            public string text;
            public Color color;
            public float showTimeMs;
            public const float MaxShowTimeMs = 2250;

            public TimeFadeupText(string setText, Color setColor)
            {
                text = setText;
                color = setColor;
                showTimeMs = MaxShowTimeMs;
            }
        }
        List<TimeFadeupText> fadeupTexts = new List<TimeFadeupText>();

        /// <summary>
        /// Add time fadeup effect. Used in the game to show times, plus (green)
        /// means we took longer for this checkpoint, minus (green) means we did
        /// good.
        /// </summary>
        /// <param name="timeMilisec">Time</param>
        /// <param name="mode">Mode</param>
        public void AddTimeFadeupEffect(int timeMilliseconds, TimeFadeupMode mode)
        {
            string text =
                //min
                ((timeMilliseconds / 1000) / 60) + ":" +
                //sec
                ((timeMilliseconds / 1000) % 60).ToString("00") + "." +
                //ms
                ((timeMilliseconds / 10) % 100).ToString("00");
            Color col = Color.White;
            if (mode == TimeFadeupMode.Plus)
            {
                text = "+ " + text;
                col = Color.Red;
            }
            else if (mode == TimeFadeupMode.Minus)
            {
                text = "- " + text;
                col = Color.Green;
            }

            fadeupTexts.Add(new TimeFadeupText(text, col));
        }

        /// <summary>
        /// Render all time fadeup effects, move them up and fade them out.
        /// </summary>
        public void RenderTimeFadeupEffects()
        {
            for (int num = 0; num < fadeupTexts.Count; num++)
            {
                TimeFadeupText fadeupText = fadeupTexts[num];
                fadeupText.showTimeMs -= HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;
                if (fadeupText.showTimeMs < 0)
                {
                    fadeupTexts.Remove(fadeupText);
                    num--;
                }
                else
                {
                    // Fade out
                    float alpha = 1.0f;
                    if (fadeupText.showTimeMs < 1500)
                        alpha = fadeupText.showTimeMs / 1500.0f;
                    // Move up
                    float moveUpAmount =
                        (TimeFadeupText.MaxShowTimeMs - fadeupText.showTimeMs) /
                        TimeFadeupText.MaxShowTimeMs;

                    // Calculate screen position
                    TextureFont.WriteTextCentered(BaseGame.Width / 2,
                        BaseGame.Height / 3 - (int)(moveUpAmount * BaseGame.Height / 3),
                        fadeupText.text,
                        ColorHelper.ApplyAlphaToColor(fadeupText.color, alpha),
                        2.25f);
                }
            }
        }
        #endregion

        #region Menu UI
        public void RenderCursorHighlight(Vector2 position, int state)
        {
            if (state < 0 || state > 3)
                return;

            Rectangle sourceRect;
            switch (state)
            {
                case 0:
                    sourceRect = new Rectangle(0, 0, 148, 128);
                    break;
                case 1:
                    sourceRect = new Rectangle(148, 0, 148, 128);
                    break;
                case 2:
                    sourceRect = new Rectangle(0, 128, 148, 128);
                    break;
                case 3:
                    sourceRect = new Rectangle(148, 128, 148, 128);
                    break;
                default:
                    return;
            }

            sourceRect.X += HexTileCursorGfxRect.X;
            sourceRect.Y += HexTileCursorGfxRect.Y;

            float zoomValue = HeavyGearManager.Map.ZoomValue;

            Rectangle renderRect = new Rectangle((int)position.X, (int)position.Y,
                (int)(BaseGame.TileWidth / zoomValue), (int)(BaseGame.TileHeight / zoomValue));

            hex.RenderOnScreen(renderRect, sourceRect);
        }
        
        #endregion
        
        #region Unit info
        /// <summary>
        /// Render unit info bar
        /// </summary>
        /// <param name="playerIndex">Index of Player whose turn it currently is</param>
        /// <param name="unitName">Name of the unit who is currently active</param>
        /// <param name="movementPointsLeft">How many movement points the current unit has left</param>
        public void RenderUnitInfo(Player player, Unit unit)
        {
            //render unit info background
            int barX = BaseGame.XToRes(112);
            int barY = BaseGame.YToRes(545);
            int barWidth = BaseGame.XToRes(800);
            int barHeight = BaseGame.YToRes(95);

#if XBOX360
            barY -= BaseGame.XToRes(40);
#endif
            menuUI.RenderOnScreen(new Rectangle(barX, barY, barWidth, barHeight), UnitInfoBarGfxRect);

            int movementPointsLeft, actionsLeft;
            movementPointsLeft = unit.MP;
            if (unit.UnitType == UnitType.Infantry)
                actionsLeft = 1 - unit.ActionsUsed;
            else
                actionsLeft = ((Vehicle)unit).Crew.Count - unit.ActionsUsed;

            if (unit.PlayerIndex != player.PlayerIndex)
            {
                //Blank out MP and actions info for enemy unit
                movementPointsLeft = 0;
                actionsLeft = 0;
            }

            // More distance to the screen borders on the Xbox 360 to fit better into
            // the save region. Calculate all rectangles for each platform,
            // then they will be used the same way on both platforms.

            // Ok, now add the player text

            //TODO: Unit Icon
            this.unit[player.ArmyIndex][unit.UnitIndex].RenderOnScreen(
                new Rectangle(
                barX + BaseGame.XToRes(15), 
                barY + BaseGame.YToRes(25),
                BaseGame.XToRes(70),
                BaseGame.YToRes(60)), 
                UnitIconGfxRect);

            //Unit Name
            TextureFont.WriteText(
                    barX + BaseGame.XToRes(100),
                    barY + BaseGame.YToRes(15),
                    unit.Name);

            //MP:
            TextureFont.WriteText(
                    barX + BaseGame.XToRes(100),
                    barY + BaseGame.YToRes(35),
                    "MP: " + movementPointsLeft);

            //Speed:
            if (unit.UnitType != UnitType.Infantry)
            {
                if (((Vehicle)unit).Speed == SpeedType.CombatSpeed)
                {
                    TextureFont.WriteText(
                        barX + BaseGame.XToRes(100),
                        barY + BaseGame.YToRes(55),
                        "Speed: Combat");
                }
                else
                {
                    TextureFont.WriteText(
                        barX + BaseGame.XToRes(100),
                        barY + BaseGame.YToRes(55),
                        "Speed: Top");
                }
            }

            //Actions:
            TextureFont.WriteText(
                barX + BaseGame.XToRes(100),
                barY + BaseGame.YToRes(75),
                "Actions: " + actionsLeft);
            
            //TODO: Crew/Squad Experience Icon
            Experience experience;
            Rectangle expRect = new Rectangle(barX + BaseGame.XToRes(295), barY + BaseGame.YToRes(25), BaseGame.XToRes(100), BaseGame.YToRes(60));

            if (unit.UnitType == UnitType.Infantry)
                experience = (Experience)((InfantrySquad)unit).Skill;
            else
                experience = (Experience)((Vehicle)unit).Crew.Skill;

            string expString = "";
            Rectangle sourceRect =  new Rectangle(0, 0, 100, 80);
            switch (experience)
            {
                case Experience.Rookie:
                    expString = "Rookie";
                    break;
                case Experience.Qualified:
                    expString = "Qualified";
                    sourceRect.X = 100;
                    break;
                case Experience.Veteran:
                    expString = "Veteran";
                    sourceRect.Y = 80;
                    break;
                case Experience.Elite:
                    expString = "Elite";
                    sourceRect.X = 100;
                    sourceRect.Y = 80;
                    break;
                case Experience.Legendary:
                    expString = "Legendary";
                    sourceRect.Y = 160;
                    break;
            }

            ranking.RenderOnScreen(expRect, sourceRect);
            TextureFont.WriteText(
                barX + BaseGame.XToRes(410),
                barY + BaseGame.YToRes(20),
                "Experience: " + expString);

            TextureFont.WriteText(
                barX + BaseGame.XToRes(410),
                barY + BaseGame.YToRes(40),
                "Squad: " + (unit.SquadIndex + 1));

            TextureFont.WriteText(
                barX + BaseGame.XToRes(410),
                barY + BaseGame.YToRes(60),
                "Morale: " + (-player.Squads[unit.SquadIndex].MoralePenalty));
                    
            //Status
            TextureFont.WriteText(barX + BaseGame.XToRes(650), barY + BaseGame.YToRes(10), "STATUS");

            if (unit.UnitType == UnitType.Infantry)
            {
            }
            else
            {
                Vehicle vehicle = (Vehicle)unit;

                TextureFont.WriteText(barX + BaseGame.XToRes(650), barY + BaseGame.YToRes(30), "Sensors : ");
                TextureFont.WriteText(barX + BaseGame.XToRes(650), barY + BaseGame.YToRes(50), "Comms  : ");
                TextureFont.WriteText(barX + BaseGame.XToRes(650), barY + BaseGame.YToRes(70), "FC         : " + (-vehicle.FireControl));
                
                if (vehicle.CommsDestroyed)
                    TextureFont.WriteText(barX + BaseGame.XToRes(750), barY + BaseGame.YToRes(50), "DS", Color.Red);
                else
                    TextureFont.WriteText(barX + BaseGame.XToRes(750), barY + BaseGame.YToRes(50), "OK", Color.Green);
                if (vehicle.SensorsDestroyed)
                    TextureFont.WriteText(barX + BaseGame.XToRes(750), barY + BaseGame.YToRes(30), "DS", Color.Red);
                else
                    TextureFont.WriteText(barX + BaseGame.XToRes(750), barY + BaseGame.YToRes(30), "OK", Color.Green);
            }
        }
        #endregion

        #region WeaponInfo
        public void RenderWeaponInfo(Weapon weapon)
        {
            int barX = BaseGame.XToRes(112);
            int barY = BaseGame.YToRes(545);
            int barWidth = BaseGame.XToRes(530);
            int barHeight = BaseGame.YToRes(95);

            menuUI.RenderOnScreen(new Rectangle(barX, barY, barWidth, barHeight), WeaponInfoBarGfxRect);

            //Weapon Name
            TextureFont.WriteText(
                    barX + BaseGame.XToRes(150),
                    barY + BaseGame.YToRes(20),
                    weapon.Name);

            //Ammo:
            TextureFont.WriteText(
                    barX + BaseGame.XToRes(150),
                    barY + BaseGame.YToRes(40),
                    "Ammo: " + weapon.CurrentAmmo);

            //ROF:
            TextureFont.WriteText(
                    barX + BaseGame.XToRes(150),
                    barY + BaseGame.YToRes(60),
                    "Rate of Fire: " + weapon.RateOfFire);
        }
        #endregion

        #region Render
        bool showFps =
#if DEBUG
 true;//false;//true;
#else
 false;
#endif

        /// <summary>
        /// Render all UI elements at the end of the frame, will also
        /// render the mouse cursor if we got a mouse attached.
        /// 
        /// Render all ui elements that we collected this frame.
        /// Flush user interface graphics, this are mainly all
        /// Texture.RenderOnScreen calls we made this frame so far.
        /// Used in UIRenderer.RenderTextsAndMouseCursor, but can also be
        /// called to force rendering UI at a specific point in the code.
        /// </summary>
        public static void Render(LineManager2D lineManager2D)
        {
            if (lineManager2D == null)
                throw new ArgumentNullException("lineManager2D");

            // Disable depth buffer for UI
            BaseGame.Device.DepthStencilState = DepthStencilState.None;

            // Draw all sprites
            Texture.additiveSprite.End();
            Texture.alphaSprite.End();
            //SpriteHelper.DrawAllSprites();

            // Render all 2d lines
            lineManager2D.Render();
        }

        /// <summary>
        /// Render texts and mouse cursor, which is done at the very end
        /// of our render loop.
        /// </summary>
        public void RenderTextsAndMouseCursor()
        {
//#if DEBUG
            // Show fps
            if (Input.KeyboardF1JustPressed ||
                // Also allow toggeling with gamepad
                (Input.GamePad.Buttons.LeftShoulder == ButtonState.Pressed &&
                Input.GamePadYJustPressed))
                showFps = !showFps;
//#endif
            if (showFps)
                TextureFont.WriteText(
                    BaseGame.XToRes(200), BaseGame.YToRes(26),
                    "Fps: " + BaseGame.Fps + " " +
                    BaseGame.Width + "x" + BaseGame.Height);

            // Render font texts
            RenderTimeFadeupEffects();
            font.WriteAll();

            // Render mouse
            // For the xbox, there is no mouse support, don't show cursor!
            if (Input.MouseDetected &&
                // Also don't show cursor in game!
                HeavyGearManager.ShowMouseCursor)
            {
                Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // Use our SpriteHelper logic to render the mouse cursor now!
                mouseCursor.RenderOnScreen(Input.MousePos);

                Texture.additiveSprite.End();
                Texture.alphaSprite.End();

                //SpriteHelper.DrawAllSprites();
            }
        }
        #endregion
    }
}