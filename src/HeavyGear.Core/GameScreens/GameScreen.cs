#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.GameLogic;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Texture = HeavyGear.Graphics.Texture;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameScreens
{
    /// <summary>
    /// GameScreen, handles all in game logic and player input
    /// </summary>
    class GameScreen : IGameScreen
    {
        #region Constants
        const float effectiveSensitivity = 0.01f;
        static readonly string[] PauseActions = new string[]
        {
            "Resume",
            "Main Menu",
            "Exit Game"
        };
        static readonly string[] InfantryActions = new string[]
        {
            "Move",
            "Attack",
            "Observe",
            "Rally",
            "End Turn"
        };

        static readonly string[] VehicleActions = new string[]
        {
            "Move",
            "Attack",
            "Observe",
            "Acquire",
            "Shift Speed",
            "Sensors",
            "Rally",
            "End Turn"
        };

        static readonly string[] SideActions = new string[]
        {
            "End Turn"
        };

        static readonly string[] InfantryActionsInfo = new string[]
        {
            "Move the unit",
            "Attack a target unit within line of sight",
            "Observes a target within LOS for indirect or guided weapon fire",
            "End your side's turn"
        };

        static readonly string[] VehicleActionsInfo = new string[]
        {
            "Move the unit",
            "Attack a target unit within line of sight",
            "Observes a target within LOS for indirect or guided weapon fire",
            "Acquire a target using active sensors",
            "Shift from Combat Speed to Top Speed or vice versa",
            "Activate/Deactivate active sensors for this unit",
            "End your side's turn"
        };

        static readonly string[] SideActionsInfo = new string[]
        {
            "End your side's turn"
        };

        const int DeploymentAreaRange = 3;

        #endregion

        #region Variables
        int[] playerSides = new int[] { 0, 2, 1, 3 };
        /// <summary>
        /// the percentage of textures that have been loaded
        /// </summary>
        static int loadProgress = 0;
        /// <summary>
        /// Set to true once all textures are loaded by resource thread
        /// </summary>
        static bool resourcesLoaded = false;
        /// <summary>
        /// Used for calibrating camera movement based on fps
        /// </summary>
        float moveAmount = -1;
        /// <summary>
        /// List of actions that can be performed by a given unit
        /// </summary>
        string[] Actions;
        /// <summary>
        /// Help text displayed for the user indicating effects of a given action
        /// </summary>
        string[] ActionsInfo;
        /// <summary>
        /// Currently selected action in a menu
        /// </summary>
        int selectedButton = 0;
        /// <summary>
        /// The current starting point of the deploy menu
        /// </summary>
        int deployMenuIndex = 0;
        /// <summary>
        /// Prevents input being registered for a time to prevent double presses
        /// </summary>
        bool inputTimeOut = false;
        /// <summary>
        /// If true will display a list of actions for the selected unit
        /// </summary>
        bool showActionMenu = false;
        /// <summary>
        /// Shows a list of weapons for a selected unit
        /// </summary>
        bool showWeaponMenu = false;
        /// <summary>
        /// Shows the side action menu, when user presses a on a hex with no unit
        /// </summary>
        bool showSideActionMenu = false;
        /// <summary>
        /// Lets user select a tile to move the unit to
        /// </summary>
        bool moveMode = false;
        /// <summary>
        /// Lets user select facing for a unit
        /// </summary>
        bool facingMode = false;
        /// <summary>
        /// Lets the user select a target for attack
        /// </summary>
        bool attackMode = false;
        /// <summary>
        /// Lets the user select a target to attempt to acquire on sensors
        /// </summary>
        bool acquireMode = false;
        /// <summary>
        /// Lets the user observer a target for artillery
        /// </summary>
        bool observeMode = false;
        /// <summary>
        /// Show the menu for placing units in deploy mode
        /// </summary>
        bool deployMenu = true;
        /// <summary>
        /// If the game is paused
        /// </summary>
        bool pause = false;
        /// <summary>
        /// References the Unit that is currently active
        /// </summary>
        Unit activeUnit = null;
        /// <summary>
        /// References the unit currently selected by the cursor
        /// </summary>
        Unit selectedUnit = null;
        /// <summary>
        /// Used for unit facing
        /// </summary>
        int rotation = 0;
        /// <summary>
        /// Cursors current position in map coord
        /// </summary>
        Point cursorPosition = new Point();
        /// <summary>
        /// Whether the cursor has changed so we can check if a new unit is under the cursor
        /// </summary>
        bool cursorChanged = true;
        /// <summary>
        /// Last time cursor input was processed to prevent it from going too fast
        /// </summary>
        float lastCursorMoveTime = 0.0f;
        /// <summary>
        /// Last time the higlighted cursor was updated
        /// </summary>
        float lastCursorUpdate = 0.0f;
        /// <summary>
        /// The current state of the highlighted cursor
        /// </summary>
        int cursorState = 0;
        /// <summary>
        /// Last time between inputs
        /// </summary>
        float lastInputTime = 0.0f;
        /// <summary>
        /// Time allowed in between cursor movements
        /// </summary>
        float cursorTimeOut = 150.0f;
        /// <summary>
        /// Set to true when the mouse has not recieved input this frame
        /// </summary>
        bool ignoreMouse = true;
        /// <summary>
        /// used for LOS highlighting (Debug)
        /// </summary>
        List<HexTile> LOSArea = null;
        /// <summary>
        /// Used to highlight the tiles within a given weapon's range
        /// </summary>
        List<HexTile> highlightedTiles;
        /// <summary>
        /// Highlights the deployment area in deploy mode
        /// </summary>
        List<HexTile> deploymentArea;
        /// <summary>
        /// The start point to set the cursor to
        /// </summary>
        //Point startPoint;
        #endregion

        #region LoadResources
        ThreadStart LoadResources = delegate
        {
            BaseGame.UI.Unit = new Texture[2][];
            BaseGame.UI.Unit[0] = new Texture[]
            {
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Hunter.png"),
                new Texture("Infantry.png")
            };
            loadProgress = 20;
            BaseGame.UI.Unit[1] = new Texture[]
            {
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Jager.png"),
                new Texture("Infantry.png")
            };
            loadProgress = 40;
            BaseGame.UI.Projectile = new Texture("Projectiles.png");
            loadProgress = 50;
            BaseGame.UI.Hex = new Texture("Hexes.png");
            loadProgress = 60;
            BaseGame.UI.Ranking = new Texture("Ranking.png");
            loadProgress = 70;

            HeavyGearManager.Map.LoadBackground();
            loadProgress = 100;

            resourcesLoaded = true;
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Create game screen
        /// </summary>
        public GameScreen()
        {
            //Init everything for deployment
            cursorPosition = new Point(HeavyGearManager.Map.Width / 2, HeavyGearManager.Map.Height / 2);
            HeavyGearManager.Camera.MapPosition = cursorPosition;
            cursorChanged = true;

            //Load resources
            Thread loadThread = new Thread(LoadResources);
            loadThread.Start();
        }
        #endregion

        #region Render
        /// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        public bool Render()
        {
            int mouseIsOverButton = -1;

            // If the user manipulated the mouse, stop ignoring the mouse
            // This allows the mouse to override the game pad or keyboard selection
            if (Input.HasMouseMoved || Input.MouseLeftButtonJustPressed)
                ignoreMouse = false;

            //Menu background variables
            Rectangle menuBackground = BaseGame.ResolutionRect;

            //Check for input time out
            if (inputTimeOut)
            {
                if (lastInputTime > 200)
                {
                    //end input time out
                    lastInputTime = 0;
                    inputTimeOut = false;
                }
                else
                {
                    lastInputTime += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;
                }
            }

            #region LoadScreen
            if (!resourcesLoaded)
            {
                Rectangle messageRect = new Rectangle();
                messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                Point startPoint = new Point(messageRect.X, messageRect.Y + BaseGame.YToRes(80));
                int height = BaseGame.YToRes(20);
                int width = 10;

                if (loadProgress > 0)
                    width = (int)(((float)loadProgress / 100) * messageRect.Width);

                TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                    "Loading...");
                
                Rectangle loadBarRect = new Rectangle(startPoint.X, startPoint.Y, width, height);

                BaseGame.UI.MenuUI.RenderOnScreen(loadBarRect, UIRenderer.MenuColorGfxRect);

                return false;
            }
            #endregion

            #region Draw Units/Map
            // Render map and all units in each player's army. also checks for input timeout here so we don't need to go through
            // all the players again
            HeavyGearManager.Map.Draw(false);
            for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
            {
                Player player = HeavyGearManager.Player(i);
                if (!inputTimeOut)
                {
                    foreach (Unit unit in player.Units)
                    {
                        if (unit.IsMoving || unit.IsShooting || unit.InAnimation || unit.Projectiles.Count > 0)
                        {
                            inputTimeOut = true;
                            break;
                        }
                    }
                }
                player.Draw();
            }
            #endregion

            #region Message Log Box
            //Render the message log box
            int messageXPos = BaseGame.XToRes(80);
            int messageYPos = BaseGame.YToRes(50);
#if XBOX360
            //messageYPos += 30;
#endif

            //Rectangle messageLogRect = new Rectangle(messageXPos, messageYPos, BaseGame.XToRes1600(770), BaseGame.YToRes1200(130));
            //BaseGame.UI.MessageLog.RenderOnScreen(messageLogRect, UIRenderer.MessageLogGfxRect);

            //Fill the log with the last 4 items
            if (HeavyGearManager.MessageLog.Length > 0)
            {
                int textXPos, textYPos;

                for (int i = 0; i < 4; i++)
                {
                    textXPos = messageXPos + BaseGame.XToRes(20);
                    textYPos = messageYPos + BaseGame.YToRes(20 + 20 * i);
                    TextureFont.WriteText(textXPos, textYPos, HeavyGearManager.MessageLog[i]);
                }
            }
            #endregion

            //This area is for modes with no camera or cursor movement, ie menus

            #region NextPlayer
            if (HeavyGearManager.NextPlayer)
            {
                if (!inputTimeOut)
                {
                    Rectangle messageRect = new Rectangle();
                    messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                    messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                    messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                    messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                    BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                        HeavyGearManager.ActivePlayer.Name);
                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(80),
                        "Turn " + HeavyGearManager.CurrentTurn + " Start");

                    if (Input.GamePadAJustPressed || Input.GamePadBackJustPressed || Input.GamePadBJustPressed || Input.GamePadStartPressed ||
                        Input.KeyboardSpaceJustPressed || Input.MouseLeftButtonJustPressed)
                    {
                        HeavyGearManager.NextPlayer = false;
                        inputTimeOut = true;
                    }
                }

                return false;
            }
            #endregion

            #region DeployMenu
            if (deployMenu)
            {
                if (!HeavyGearManager.DeployMode)
                {
                    deployMenu = false;
                    return false;
                }
                if (!HeavyGearManager.Transport.IsConnected ||
                    HeavyGearManager.LocalPlayer.PlayerIndex == HeavyGearManager.ActivePlayer.PlayerIndex)
                {
                    #region Display Menu
                    //render menu background
                    BaseGame.UI.MenuUI.RenderOnScreen(menuBackground, UIRenderer.MenuBackgroundGfxRect);
                    TextureFont.WriteText(BaseGame.XToRes(103), menuBackground.Y + BaseGame.YToRes(16), "Deploy Units");//115, 23 for menu title
                    //(200, 122), (824, 122), (200, 526), (824, 526) Selection Box
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(122), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Top segment
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Left Segment
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(526), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Bottom segment
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(824), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Right Segment

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(301), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(223), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(324), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(425), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line

                    int rowX = BaseGame.XToRes(200);
                    int rowY = BaseGame.YToRes(122);
                    int rowWidth = BaseGame.XToRes(624);
                    int rowHeight = BaseGame.YToRes(101);

                    if (HeavyGearManager.ActivePlayer.Units.Count > 4)
                    {
                        if (deployMenuIndex < 0)
                            deployMenuIndex = 0;
                        if (deployMenuIndex > HeavyGearManager.ActivePlayer.Units.Count - 4)
                            deployMenuIndex = HeavyGearManager.ActivePlayer.Units.Count - 4;
                        if (selectedButton < deployMenuIndex)
                            deployMenuIndex = selectedButton;
                        if (selectedButton > deployMenuIndex + 3)
                            deployMenuIndex = selectedButton - 3;

                        //display up or down arrows and check for mouse over
                        if (deployMenuIndex > 0)
                        {
                            //up arrow
                            Rectangle upRect = new Rectangle(BaseGame.XToRes(500), BaseGame.YToRes(80), BaseGame.XToRes(32), BaseGame.YToRes(32));
                            Rectangle upMouseRect = new Rectangle(BaseGame.XToRes(484), BaseGame.YToRes(64), BaseGame.XToRes(32), BaseGame.YToRes(32));

                            Color arrowColor = Color.White;

                            if (Input.MouseInBox(upMouseRect))
                            {
                                mouseIsOverButton = 200;
                                arrowColor = Color.Black;
                            }

                            BaseGame.UI.MenuUI.RenderSpriteOnScreen(upRect, UIRenderer.ArrowGfxRect, arrowColor, 0);
                        }

                        if (deployMenuIndex < HeavyGearManager.ActivePlayer.Units.Count - 4)
                        {
                            //down arrow
                            Rectangle downRect = new Rectangle(BaseGame.XToRes(500), BaseGame.YToRes(550), BaseGame.XToRes(32), BaseGame.YToRes(32));
                            Rectangle downMouseRect = new Rectangle(BaseGame.XToRes(484), BaseGame.YToRes(534), BaseGame.XToRes(32), BaseGame.YToRes(32));

                            Color arrowColor = Color.White;

                            if (Input.MouseInBox(downMouseRect))
                            {
                                mouseIsOverButton = 300;
                                arrowColor = Color.Black;
                            }

                            BaseGame.UI.MenuUI.RenderSpriteOnScreen(downRect, UIRenderer.ArrowGfxRect, arrowColor, MathHelper.ToRadians(180.0f));
                        }
                    }
                    else
                    {
                        deployMenuIndex = 0;
                    }

                    //Show 4 units in the list, starting with startIndex
                    for (int j = deployMenuIndex; j < deployMenuIndex + 4 && j < HeavyGearManager.ActivePlayer.Units.Count; j++)
                    {
                        Rectangle selectionRect = new Rectangle(rowX, rowY + rowHeight * (j - deployMenuIndex), rowWidth, rowHeight);

                        if (Input.MouseInBox(selectionRect))
                            mouseIsOverButton = j;

#if XBOX360
                    if (selectedButton == j)
                        BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);
#else
                        if (selectedButton == j && mouseIsOverButton >= 0 && mouseIsOverButton < HeavyGearManager.ActivePlayer.Units.Count)
                            BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);
#endif

                        BaseGame.UI.Unit[HeavyGearManager.ActivePlayer.ArmyIndex][HeavyGearManager.ActivePlayer.Units[j].UnitIndex].RenderOnScreen(
                                new Rectangle(rowX, rowY + rowHeight * (j - deployMenuIndex), BaseGame.XToRes(101), rowHeight),
                                UIRenderer.UnitIconGfxRect);
                        int squadIndex = HeavyGearManager.ActivePlayer.Units[j].SquadIndex + 1;
                        TextureFont.WriteText(rowX + BaseGame.XToRes(126), rowY + rowHeight * (j - deployMenuIndex) + BaseGame.YToRes(30), HeavyGearManager.ActivePlayer.Units[j].Name);
                        TextureFont.WriteText(rowX + BaseGame.XToRes(426), rowY + rowHeight * (j - deployMenuIndex) + BaseGame.YToRes(30), squadIndex.ToString());
                        //TextureFont.WriteText(rowX + BaseGame.XToRes(126), rowY + rowHeight * currentRow + BaseGame.YToRes(60), HeavyGearManager.ActivePlayer.Army[j].);
                        if (HeavyGearManager.ActivePlayer.Units[j].MapPosition.X >= 0 && HeavyGearManager.ActivePlayer.Units[j].MapPosition.Y >= 0)
                            TextureFont.WriteText(rowX + BaseGame.XToRes(126), rowY + rowHeight * (j - deployMenuIndex) + BaseGame.YToRes(60), "PLACED", Color.Red);
                    }

#if !XBOX360
                    Rectangle doneButton = new Rectangle(BaseGame.XToRes(750), BaseGame.YToRes(590), UIRenderer.MenuItemGfxRect.Width, UIRenderer.MenuItemGfxRect.Height);

                    if (Input.MouseInBox(doneButton))
                    {
                        mouseIsOverButton = 100;
                        BaseGame.UI.MenuUI.RenderOnScreen(doneButton, UIRenderer.MenuItemSelectGfxRect);
                    }
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(doneButton, UIRenderer.MenuItemSelectGfxRect);

                    TextureFont.WriteTextCentered(doneButton.X + doneButton.Width / 2, doneButton.Y + doneButton.Height / 2, "DONE");
#endif

                    if (deploymentArea == null)
                    {
                        deploymentArea = HeavyGearManager.Map.GetDeploymentArea(playerSides[HeavyGearManager.ActivePlayer.PlayerIndex], DeploymentAreaRange);
                        foreach (HexTile tile in deploymentArea)
                            tile.Highlight = true;
                    }
                    #endregion

                    #region Headers
                    TextureFont.WriteText(BaseGame.XToRes(210), menuBackground.Y + BaseGame.YToRes(65), "Player " + (HeavyGearManager.ActivePlayer.PlayerIndex + 1));
                    TextureFont.WriteText(BaseGame.XToRes(460), menuBackground.Y + BaseGame.YToRes(105), "Unit");
                    #endregion

                    #region Player Input
                    if (!ignoreMouse && mouseIsOverButton >= 0 && mouseIsOverButton < HeavyGearManager.ActivePlayer.Units.Count)
                        selectedButton = mouseIsOverButton;

                    // handle xbox controller input for first player
                    if (!inputTimeOut)
                    {
                        if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 100) ||
                            Input.GamePadXJustPressed || Input.Keyboard.IsKeyDown(Keys.Enter))
                        {
                            bool unitNeedsPlace = false;
                            inputTimeOut = true;
                            foreach (Unit unit in HeavyGearManager.ActivePlayer.Units)
                            {
                                if (unit.MapPosition.X < 0 || unit.MapPosition.Y < 0)
                                {
                                    unitNeedsPlace = true;
                                    break;
                                }
                            }

                            if (unitNeedsPlace)
                                return false;

                            foreach (HexTile tile in deploymentArea)
                                tile.Highlight = false;
                            deploymentArea = null;
                            deployMenuIndex = 0;
                            selectedButton = 0;
                            cursorChanged = true;
                            HeavyGearManager.DeployEndTurn();
                            Point point = Point.Zero;
                            if (HeavyGearManager.ActivePlayer.PlayerIndex == 1)
                                point = new Point(HeavyGearManager.Map.Width / 2, HeavyGearManager.Map.Height - 1);
                            else if (HeavyGearManager.ActivePlayer.PlayerIndex == 2)
                                point = new Point(1, HeavyGearManager.Map.Height / 2);
                            else
                                point = new Point(HeavyGearManager.Map.Width - 1, HeavyGearManager.Map.Height / 2);

                            HeavyGearManager.Camera.MapPosition = point;
                            cursorPosition = point;

                            return false;
                        }
                        else if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0 && mouseIsOverButton < HeavyGearManager.ActivePlayer.Units.Count) ||
                            Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                        {
                            activeUnit = HeavyGearManager.ActivePlayer.Units[selectedButton];
                            deployMenu = false;
                            inputTimeOut = true;
                            return false;
                        }

                        if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 200) ||
                            Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                        {
                            if (selectedButton > 0)
                                selectedButton--;
                        }
                        else if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 300) ||
                            Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                        {
                            if (selectedButton < HeavyGearManager.ActivePlayer.Units.Count - 1)
                                selectedButton++;
                        }

                        else if (Input.GamePadBJustPressed || Input.KeyboardEscapeJustPressed ||
                       (Input.MouseRightButtonJustPressed && mouseIsOverButton >= 0 && mouseIsOverButton < HeavyGearManager.ActivePlayer.Units.Count))
                        {
                            inputTimeOut = true;
                            HeavyGearManager.ActivePlayer.Units[selectedButton].MapPosition = new Point(-1, -1);
                        }

                    }
                    #endregion
                }
                else
                {
                    Rectangle messageRect = new Rectangle();
                    messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                    messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                    messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                    messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                    BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                        "Waiting for turn...");
                }

                return false;
            }
            #endregion

            #region Pause Mode
            if (pause)
            {
                #region Draw
                //Show the pause menu
                TextureFont.WriteText(BaseGame.XToRes(460), BaseGame.YToRes(180), "P A U S E", Color.Orange);


                int xPos = BaseGame.XToRes(410);
                int yPos = BaseGame.YToRes(210);
                int buttonWidth = BaseGame.XToRes(206);
                int buttonHeight = BaseGame.YToRes(56);

                if (selectedButton >= PauseActions.Length)
                    selectedButton = 0;

                for (int num = 0; num < PauseActions.Length; num++)
                {
                    // Is this button currently selected?
                    bool selected = num == selectedButton;

                    Rectangle renderRect = new Rectangle(xPos, yPos + buttonHeight * num, buttonWidth, buttonHeight);
                    //if so render selected button
                    if (selected)
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemSelectGfxRect);
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemGfxRect);

                    if (Input.MouseInBox(renderRect))
                        mouseIsOverButton = num;

                    TextureFont.WriteTextCentered(xPos + BaseGame.XToRes(100), yPos + buttonHeight * num + BaseGame.YToRes(30), PauseActions[num]);
                }
                #endregion

                #region Pause Menu Input

                if (mouseIsOverButton >= 0)
                    selectedButton = mouseIsOverButton;

                if (!inputTimeOut)
                {
                    if (Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton =
                            (selectedButton + PauseActions.Length - 1) % PauseActions.Length;
                    }
                    else if (Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton = (selectedButton + 1) % PauseActions.Length;
                    }

                    // bool aButtonPressed = BaseGame.UI.RenderControllerButtons(false);
                    // If user presses the mouse button or the game pad A or Space,
                    // start the game screen for the currently selected game part.
                    if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0) ||
                        Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        inputTimeOut = true;
                        switch (selectedButton)
                        {
                            case 0:
                                pause = false;
                                return false;
                            case 1:
                                HeavyGearManager.ReturnToMenu();
                                return false;
                            case 2:
                                HeavyGearManager.ExitGame();
                                return true;
                        }
                        Sound.Play(Sound.Sounds.ButtonClick);
                    }

                    if (Input.GamePadBJustPressed ||
                        Input.GamePadBackJustPressed || Input.KeyboardEscapeJustPressed)
                    {
                        inputTimeOut = true;
                        pause = false;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            #region Game Over
            if (HeavyGearManager.GameOver)
            {
                //display game over screen

                TextureFont.WriteText(BaseGame.XToRes(500), BaseGame.YToRes(310), "Player " + (HeavyGearManager.PlayerWon + 1) + " Wins!!");

                if (!inputTimeOut)
                {
                    if (Input.GamePadBackJustPressed || Input.KeyboardEscapeJustPressed ||
                            Input.GamePadAJustPressed ||
                            Input.GamePadBJustPressed ||
                            Input.GamePadXJustPressed ||
                            Input.GamePadXJustPressed ||
                            Input.KeyboardSpaceJustPressed ||
                            Input.MouseLeftButtonJustPressed ||
                            Input.MouseRightButtonJustPressed
                        )
                    {
                        // Stop sounds
                        //Sound.StopGearSound()

                        // Play menu music again
                        //Sound.Play(Sound.Sounds.MenuMusic);

                        // Return to menu
                        HeavyGearManager.ReturnToMenu();
                    }
                    return false;
                }
            }
            #endregion

            #region ActionMenu
            //render the action menu if appropriate
            if (showActionMenu)
            {
                #region Display
                //set button variables based on unit type
                int NumberOfButtons = 0;
                if (activeUnit.UnitType == UnitType.Infantry)
                {
                    Actions = InfantryActions;
                    ActionsInfo = InfantryActionsInfo;
                    NumberOfButtons = InfantryActions.Length;
                }
                else
                {
                    Actions = VehicleActions;
                    ActionsInfo = VehicleActionsInfo;
                    NumberOfButtons = VehicleActions.Length;
                }

                int xPos = BaseGame.XToRes(410);
                int yPos = BaseGame.YToRes(210);
                int buttonWidth = BaseGame.XToRes(206);
                int buttonHeight = BaseGame.YToRes(56);

                //makes sure the selected button isn't out of range when going from vehicle to infantry
                if (selectedButton >= NumberOfButtons)
                    selectedButton = 0;

                //Render Message Box
                Rectangle messageBox = new Rectangle(
                    BaseGame.XToRes(556),
                    BaseGame.YToRes(10),
                    BaseGame.XToRes(456),
                    BaseGame.YToRes(182));
#if XBOX360
                messageBox.Y += BaseGame.YToRes(20);
#endif
                BaseGame.UI.MenuUI.RenderOnScreen(messageBox, UIRenderer.MessageBoxGfxRect);

                for (int num = 0; num < NumberOfButtons; num++)
                {
                    // Is this button currently selected?
                    bool selected = num == selectedButton;

                    Rectangle renderRect = new Rectangle(xPos, yPos + buttonHeight * num, buttonWidth, buttonHeight);
                    //if so render selected button
                    if (selected)
                    {
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemSelectGfxRect);
                        TextureFont.WriteText(BaseGame.XToRes(576), BaseGame.YToRes(30), ActionsInfo[num]);
                    }
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemGfxRect);

                    if (Input.MouseInBox(renderRect))
                        mouseIsOverButton = num;

                    TextureFont.WriteTextCentered(xPos + BaseGame.XToRes(100), yPos + buttonHeight * num + BaseGame.YToRes(30), Actions[num]);

                }
                #endregion

                #region Input
                if (!ignoreMouse && mouseIsOverButton >= 0)
                    selectedButton = mouseIsOverButton;
                if (!inputTimeOut)
                {
                    if (Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton =
                            (selectedButton + NumberOfButtons - 1) % NumberOfButtons;
                    }
                    else if (Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton = (selectedButton + 1) % NumberOfButtons;
                    }

                    if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0) ||
                        Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        showActionMenu = false;
                        inputTimeOut = true;
                        switch (selectedButton)
                        {
                            case 0:
                                moveMode = true;
                                break;
                            case 1:
                                attackMode = true;
                                break;
                            case 2:
                                observeMode = true;
                                break;
                            case 3:
                                if (activeUnit.UnitType == UnitType.Infantry)
                                {
                                    if (activeUnit.IsSquadLeader)
                                        HeavyGearManager.ActivePlayer.Squads[activeUnit.SquadIndex].Rally();
                                    else
                                        HeavyGearManager.MessageLogAdd(activeUnit.Name + " is not the squad leader.");
                                }
                                else
                                    acquireMode = true;
                                break;
                            case 4:
                                if (activeUnit.UnitType == UnitType.Infantry)
                                    activeUnit.IsTurn = false;
                                else
                                    ((Vehicle)activeUnit).ChangeSpeed();
                                break;
                            case 5:
                                ((Vehicle)activeUnit).ToggleSensors();
                                break;
                            case 6:
                                if (activeUnit.IsSquadLeader)
                                    HeavyGearManager.ActivePlayer.Squads[activeUnit.SquadIndex].Rally();
                                else
                                    HeavyGearManager.MessageLogAdd(activeUnit.Name + " is not the squad leader.");
                                break;
                            case 7:
                                HeavyGearManager.EndTurn();
                                break;
                        }
                        Sound.Play(Sound.Sounds.ButtonClick);
                        return false;

                    }

                    if (Input.GamePadBJustPressed || Input.GamePadBackJustPressed ||
                        Input.KeyboardEscapeJustPressed || Input.MouseRightButtonJustPressed)
                    {
                        inputTimeOut = true;
                        showActionMenu = false;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            #region SideActionMenu
            if (showSideActionMenu)
            {
                #region Display
                //set button variables based on unit type
                int NumberOfButtons = 0;
                Actions = SideActions;
                ActionsInfo = SideActionsInfo;
                NumberOfButtons = SideActions.Length;

                int xPos = BaseGame.XToRes(410);
                int yPos = BaseGame.YToRes(210);
                int buttonWidth = BaseGame.XToRes(206);
                int buttonHeight = BaseGame.YToRes(56);

                //makes sure the selected button isn't out of range when going from vehicle to infantry
                if (selectedButton >= NumberOfButtons)
                    selectedButton = 0;

                //Render Message Box
                Rectangle messageBox = new Rectangle(
                    BaseGame.XToRes(556),
                    BaseGame.YToRes(10),
                    BaseGame.XToRes(456),
                    BaseGame.YToRes(182));
#if XBOX360
                messageBox.Y += BaseGame.YToRes(20);
#endif
                BaseGame.UI.MenuUI.RenderOnScreen(messageBox, UIRenderer.MessageBoxGfxRect);

                for (int num = 0; num < NumberOfButtons; num++)
                {
                    // Is this button currently selected?
                    bool selected = num == selectedButton;

                    Rectangle renderRect = new Rectangle(xPos, yPos + buttonHeight * num, buttonWidth, buttonHeight);
                    //if so render selected button
                    if (selected)
                    {
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemSelectGfxRect);
                        TextureFont.WriteText(BaseGame.XToRes(576), BaseGame.YToRes(30), ActionsInfo[num]);
                    }
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemGfxRect);

                    if (Input.MouseInBox(renderRect))
                        mouseIsOverButton = num;

                    TextureFont.WriteTextCentered(xPos + BaseGame.XToRes(100), yPos + buttonHeight * num + BaseGame.YToRes(30), Actions[num]);

                }
                #endregion

                #region Input
                if (!ignoreMouse && mouseIsOverButton >= 0)
                    selectedButton = mouseIsOverButton;
                if (!inputTimeOut)
                {
                    if (Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton =
                            (selectedButton + NumberOfButtons - 1) % NumberOfButtons;
                    }
                    else if (Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);
                        selectedButton = (selectedButton + 1) % NumberOfButtons;
                    }

                    if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0) ||
                        Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        showSideActionMenu = false;
                        inputTimeOut = true;
                        switch (selectedButton)
                        {
                            case 0:
                                HeavyGearManager.EndTurn();
                                break;
                        }
                        Sound.Play(Sound.Sounds.ButtonClick);
                        return false;

                    }

                    if (Input.GamePadBJustPressed || Input.GamePadBackJustPressed ||
                        Input.KeyboardEscapeJustPressed || Input.MouseRightButtonJustPressed)
                    {
                        inputTimeOut = true;
                        showSideActionMenu = false;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            #region WeaponMenu
            if (showWeaponMenu)
            {
                if (activeUnit.Weapons.Count == 0)
                {
                    showWeaponMenu = false;
                    return true;
                }

                #region Display
                //set button variables based on unit type
                int NumberOfWeapons = activeUnit.Weapons.Count;
                int xPos = BaseGame.XToRes(410);
                int yPos = BaseGame.YToRes(210);
                int buttonWidth = BaseGame.XToRes(206);
                int buttonHeight = BaseGame.YToRes(56);

                for (int i = 0; i < NumberOfWeapons; i++)
                {
                    bool selected = false;
                    if (i == activeUnit.Weapon)
                        selected = true;

                    Rectangle renderRect = new Rectangle(xPos, yPos + buttonHeight * i, buttonWidth, buttonHeight);
                    //if so render selected button
                    if (selected)
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemSelectGfxRect);
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.MenuItemGfxRect);

                    if (Input.MouseInBox(renderRect))
                        mouseIsOverButton = i;

                    TextureFont.WriteTextCentered(xPos + BaseGame.XToRes(100), yPos + buttonHeight * i + BaseGame.YToRes(30), activeUnit.Weapons[i].Name);
                }

                //Weapon Info display
                BaseGame.UI.RenderWeaponInfo(activeUnit.Weapons[activeUnit.Weapon]);
                #endregion

                #region Input
                if (!ignoreMouse && mouseIsOverButton >= 0)
                {
                    activeUnit.Weapon = mouseIsOverButton;
                }
                if (!inputTimeOut)
                {
                    //bool selectNextWeapon = false;
                    if (Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);

                        if (activeUnit.Weapon < NumberOfWeapons - 1)
                            activeUnit.Weapon++;
                        else
                            activeUnit.Weapon = 0;
                    }
                    else if (Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        Sound.Play(Sound.Sounds.ButtonClick);

                        if (activeUnit.Weapon > 0)
                            activeUnit.Weapon--;
                        else
                            activeUnit.Weapon = NumberOfWeapons - 1;
                    }

                    if (Input.GamePadRightJustPressed || Input.KeyboardRightJustPressed)
                    {
                        Weapon weapon = activeUnit.Weapons[activeUnit.Weapon];
                        if (weapon.RateOfFire < weapon.MaxRateOfFire)
                            weapon.RateOfFire++;
                    }

                    if (Input.GamePadLeftJustPressed || Input.KeyboardLeftJustPressed)
                    {
                        Weapon weapon = activeUnit.Weapons[activeUnit.Weapon];
                        if (weapon.RateOfFire > 0)
                            weapon.RateOfFire--;
                    }

                    //Pressing space, X, A, B to end menu
                    if (Input.GamePadXPressed || Input.GamePadAJustPressed || Input.GamePadBJustPressed || Input.KeyboardSpaceJustPressed
                        || Input.MouseLeftButtonJustPressed)
                    {
                        showWeaponMenu = false;
                        activeUnit = null;
                        inputTimeOut = true;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            //This area is for modes where camera movement is allowed but no cursor is needed

            #region Camera Movement
#if !XBOX360
            if (moveAmount < 0)
                moveAmount = BaseGame.MoveFactorPerSecond * BaseGame.MouseCameraMoveRate;

            if (Input.MousePos.X > BaseGame.Width - 3)
                HeavyGearManager.Camera.MoveRight(moveAmount);
            else if (Input.MousePos.X < 3)
                HeavyGearManager.Camera.MoveLeft(moveAmount);

            if (Input.MousePos.Y > BaseGame.Height - 6)
                HeavyGearManager.Camera.MoveDown(moveAmount);
            else if (Input.MousePos.Y < 3)
                HeavyGearManager.Camera.MoveUp(moveAmount);

            if (!inputTimeOut)
            {
                if (Input.MouseWheelDelta != 0)
                {
                    float zoomAmount = -(6 / (float)Input.MouseWheelDelta);
                    BaseGame.Zoom(zoomAmount);
                }

                if (Input.MouseMiddleButtonPressed)
                    BaseGame.ZoomReset();
            }
#endif
            if (moveAmount < 0)
                moveAmount = BaseGame.MoveFactorPerSecond * BaseGame.CameraMoveRate;
            if (Input.GamePad.ThumbSticks.Right.X < -effectiveSensitivity)
                HeavyGearManager.Camera.MoveLeft(Math.Abs(Input.GamePad.ThumbSticks.Right.X * moveAmount));
            else if (Input.GamePad.ThumbSticks.Right.X > effectiveSensitivity)
                HeavyGearManager.Camera.MoveRight(Input.GamePad.ThumbSticks.Right.X * moveAmount);

            if (Input.GamePad.ThumbSticks.Right.Y > effectiveSensitivity)
                HeavyGearManager.Camera.MoveUp(Input.GamePad.ThumbSticks.Right.Y * moveAmount);
            else if (Input.GamePad.ThumbSticks.Right.Y < -effectiveSensitivity)
                HeavyGearManager.Camera.MoveDown(Math.Abs(Input.GamePad.ThumbSticks.Right.Y * moveAmount));

            if (Input.Keyboard.IsKeyDown(Keys.W))
                HeavyGearManager.Camera.MoveUp(0.75f * moveAmount);
            else if (Input.Keyboard.IsKeyDown(Keys.S))
                HeavyGearManager.Camera.MoveDown(0.75f * moveAmount);

            if (Input.Keyboard.IsKeyDown(Keys.A))
                HeavyGearManager.Camera.MoveLeft(0.75f * moveAmount);
            else if (Input.Keyboard.IsKeyDown(Keys.D))
                HeavyGearManager.Camera.MoveRight(0.75f * moveAmount);

            if (!inputTimeOut)
            {
                if (Input.GamePad.Triggers.Left > 0)
                    BaseGame.Zoom(Input.GamePad.Triggers.Left / 1000);
                else if (Input.GamePad.Triggers.Right > 0)
                    BaseGame.Zoom(-(Input.GamePad.Triggers.Right / 1000));
            }

            #endregion

            #region Facing Mode
            if (facingMode)
            {
                TextureFont.WriteTextCentered(BaseGame.XToRes(512), BaseGame.YToRes(50), "Select Facing");

                //if (HeavyGearManager.Camera.MapPosition != activeUnit.MapPosition)
                //    HeavyGearManager.Camera.MapPosition = activeUnit.MapPosition;

                #region Input

                if (!inputTimeOut)
                {
                    if (!ignoreMouse)
                    {
                        HexTile tile = HeavyGearManager.Map.GetTile(activeUnit.MapPosition.X, activeUnit.MapPosition.Y);
                        Vector2 hexCenter, mapOrigin;
                        float zoomValue = HeavyGearManager.Map.ZoomValue;
                        BaseGame.ConvertMapToPixel(tile.MapPosition.X, tile.MapPosition.Y, out hexCenter);
                        hexCenter.X += (BaseGame.TileWidth / zoomValue) / 2;
                        hexCenter.Y += (BaseGame.TileHeight / zoomValue) / 2;
                        mapOrigin = HeavyGearManager.Map.Origin;
                        Vector2.Subtract(ref hexCenter, ref mapOrigin, out hexCenter);

                        if (Input.MousePos.X < (int)(hexCenter.X - (BaseGame.TileWidth / zoomValue) / 2))
                        {
                            if (Input.MousePos.Y < hexCenter.Y)
                                activeUnit.Rotation = Facing.NorthWest;
                            else
                                activeUnit.Rotation = Facing.SouthWest;
                        }
                        else if (Input.MousePos.X > (int)(hexCenter.X + (BaseGame.TileWidth / zoomValue) / 2))
                        {
                            if (Input.MousePos.Y < hexCenter.Y)
                                activeUnit.Rotation = Facing.NorthEast;
                            else
                                activeUnit.Rotation = Facing.SouthEast;
                        }
                        else if (Input.MousePos.Y > (int)hexCenter.Y)
                            activeUnit.Rotation = Facing.South;
                        else if (Input.MousePos.Y < (int)hexCenter.Y)
                            activeUnit.Rotation = Facing.North;
                    }
                    else
                    {
                        rotation = (int)Math.Round(MathHelper.ToDegrees(activeUnit.Rotation));
                        if (Input.GamePadLeftJustPressed || Input.KeyboardLeftJustPressed)
                        {
                            switch (rotation)
                            {
                                case FacingInt.North:
                                    activeUnit.Rotation = Facing.NorthWest;
                                    break;
                                case FacingInt.NorthEast:
                                    activeUnit.Rotation = Facing.North;
                                    break;
                                case FacingInt.SouthEast:
                                    activeUnit.Rotation = Facing.NorthEast;
                                    break;
                                case FacingInt.South:
                                    activeUnit.Rotation = Facing.SouthEast;
                                    break;
                                case FacingInt.SouthWest:
                                    activeUnit.Rotation = Facing.South;
                                    break;
                                case FacingInt.NorthWest:
                                    activeUnit.Rotation = Facing.SouthWest;
                                    break;
                            }
                        }
                        else if (Input.GamePadRightJustPressed || Input.KeyboardRightJustPressed)
                        {
                            switch (rotation)
                            {
                                case FacingInt.North:
                                    activeUnit.Rotation = Facing.NorthEast;
                                    break;
                                case FacingInt.NorthEast:
                                    activeUnit.Rotation = Facing.SouthEast;
                                    break;
                                case FacingInt.SouthEast:
                                    activeUnit.Rotation = Facing.South;
                                    break;
                                case FacingInt.South:
                                    activeUnit.Rotation = Facing.SouthWest;
                                    break;
                                case FacingInt.SouthWest:
                                    activeUnit.Rotation = Facing.NorthWest;
                                    break;
                                case FacingInt.NorthWest:
                                    activeUnit.Rotation = Facing.North;
                                    break;
                            }
                        }
                    }

                    if (Input.MouseLeftButtonJustPressed || Input.GamePadAJustPressed ||
                        Input.GamePadBJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        facingMode = false;
                        activeUnit = null;
                        cursorChanged = true;
                        inputTimeOut = true;
                        if (HeavyGearManager.DeployMode)
                            deployMenu = true;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            //This area is for modes using both camera and cursor controls

            #region Cursor Movement
            bool cursorMoved = false;
            //Mouse selection
            if (!ignoreMouse)
            {
                Point mousePosition;
                Vector2 mouseScreenPosition = new Vector2();
                mouseScreenPosition.X = Input.MousePos.X + HeavyGearManager.Map.Origin.X;
                mouseScreenPosition.Y = Input.MousePos.Y + HeavyGearManager.Map.Origin.Y;

                BaseGame.ConvertPixelToMap(ref mouseScreenPosition, out mousePosition);
                if (mousePosition != cursorPosition)
                {
                    cursorPosition = mousePosition;
                    cursorChanged = true;
                }
            }
            //Gamepad movement
            if (lastCursorMoveTime > cursorTimeOut)
            {
                if (Input.GamePad.ThumbSticks.Left.X < -effectiveSensitivity)
                {
                    if (Input.GamePad.ThumbSticks.Left.Y < -effectiveSensitivity)
                    {
                        if (cursorPosition.Y % 2 == 0)
                            cursorPosition.X -= 1;

                        cursorPosition.Y += 1;
                        cursorMoved = true;
                    }
                    else if (Input.GamePad.ThumbSticks.Left.Y > effectiveSensitivity)
                    {
                        if (cursorPosition.Y % 2 == 0)
                            cursorPosition.X -= 1;

                        cursorPosition.Y -= 1;
                        cursorMoved = true;
                    }
                }
                else if (Input.GamePad.ThumbSticks.Left.X > effectiveSensitivity)
                {
                    if (Input.GamePad.ThumbSticks.Left.Y < -effectiveSensitivity)
                    {
                        if (cursorPosition.Y % 2 > 0)
                            cursorPosition.X += 1;

                        cursorPosition.Y += 1;
                        cursorMoved = true;
                    }
                    else if (Input.GamePad.ThumbSticks.Left.Y > effectiveSensitivity)
                    {
                        if (cursorPosition.Y % 2 > 0)
                            cursorPosition.X += 1;

                        cursorPosition.Y -= 1;
                        cursorMoved = true;
                    }
                }
                else if (Input.GamePad.ThumbSticks.Left.Y > effectiveSensitivity)
                {
                    cursorPosition.Y -= 2;
                    cursorMoved = true;
                }
                else if (Input.GamePad.ThumbSticks.Left.Y < -effectiveSensitivity)
                {
                    cursorPosition.Y += 2;
                    cursorMoved = true;
                }

                //keyboard
                if (Input.KeyboardKeyJustPressed(Keys.NumPad8))
                {
                    cursorPosition.Y -= 2;
                    cursorMoved = true;
                }
                if (Input.KeyboardKeyJustPressed(Keys.NumPad9))
                {
                    if (cursorPosition.Y % 2 > 0)
                        cursorPosition.X += 1;

                    cursorPosition.Y -= 1;
                    cursorMoved = true;
                }
                if (Input.KeyboardKeyJustPressed(Keys.NumPad7))
                {
                    if (cursorPosition.Y % 2 == 0)
                        cursorPosition.X -= 1;

                    cursorPosition.Y -= 1;
                    cursorMoved = true;
                }
                if (Input.KeyboardKeyJustPressed(Keys.NumPad1))
                {
                    if (cursorPosition.Y % 2 == 0)
                        cursorPosition.X -= 1;

                    cursorPosition.Y += 1;
                    cursorMoved = true;
                }
                if (Input.KeyboardKeyJustPressed(Keys.NumPad2))
                {
                    cursorPosition.Y += 2;
                    cursorMoved = true;
                }
                if (Input.KeyboardKeyJustPressed(Keys.NumPad3))
                {
                    if (cursorPosition.Y % 2 > 0)
                        cursorPosition.X += 1;

                    cursorPosition.Y += 1;
                    cursorMoved = true;
                }

                if ((int)cursorPosition.X < 0)
                    cursorPosition.X = 0;

                if ((int)cursorPosition.Y < 0)
                    cursorPosition.Y = 0;

                if ((int)cursorPosition.X > (HeavyGearManager.Map.Width - 1))
                    cursorPosition.X = HeavyGearManager.Map.Width - 1;

                if ((int)cursorPosition.Y > (HeavyGearManager.Map.Height - 1))
                    cursorPosition.Y = HeavyGearManager.Map.Height - 1;
            }

            if (cursorMoved)
            {
                lastCursorMoveTime = 0;
                cursorChanged = true;
            }
            else
            {
                lastCursorMoveTime += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;
            }

            if (!inputTimeOut)
            {
                //Center cursor position to camera position on left stick press
                if (Input.GamePad.Buttons.LeftStick == ButtonState.Pressed)
                {
                    cursorPosition = HeavyGearManager.Camera.MapPosition;
                    cursorChanged = true;
                }
                //Center camera pos to cursor pos on right stick press
                if (Input.GamePad.Buttons.RightStick == ButtonState.Pressed)
                {
                    HeavyGearManager.Camera.MapPosition = cursorPosition;
                }
            }
            #endregion

            #region UnitInfo
            if (cursorChanged)
            {
                bool unitFound = false;
                for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                {
                    Player player = HeavyGearManager.Player(i);
                    foreach (Unit unit in player.Units)
                    {
                        if (unit.MapPosition == cursorPosition && unit.IsAlive)
                        {
                            unitFound = true;
                            selectedUnit = unit;
                            break;
                        }
                    }
                    if (unitFound)
                        break;
                }
                if (!unitFound)
                    selectedUnit = null;

                cursorChanged = false;
            }
            //Only show the unit info bar if a unit is selected and it is the current player's
            if (selectedUnit != null && selectedUnit.PlayerIndex == HeavyGearManager.ActivePlayer.PlayerIndex)
                BaseGame.UI.RenderUnitInfo(HeavyGearManager.ActivePlayer, selectedUnit);

            #endregion

            #region Cursor Position
            Vector2 position;
            Vector2 cameraPosition = HeavyGearManager.Camera.Position;
            Vector2 origin = Vector2.Zero;
            origin.X = cameraPosition.X - BaseGame.Width / 2;
            origin.Y = cameraPosition.Y - BaseGame.Height / 2;

            BaseGame.ConvertMapToPixel(ref cursorPosition, out position);

            Vector2.Subtract(ref position, ref origin, out position);
            #endregion

            #region Draw Cursor
            BaseGame.UI.RenderCursorHighlight(position, cursorState);

            if (lastCursorUpdate > 250)
            {
                cursorState = (cursorState + 1) % 4;
                lastCursorUpdate = 0;
            }
            else
                lastCursorUpdate += HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;

            #endregion

            #region Deploy Mode
            //check for deploy mode
            if (HeavyGearManager.DeployMode)
            {
                #region Input

                if (!inputTimeOut)
                {
                    if (Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed || Input.MouseLeftButtonJustPressed)
                    {
                        inputTimeOut = true;
                        if (activeUnit.MapPosition == cursorPosition)
                            facingMode = true;
                        else if (deploymentArea.Contains(HeavyGearManager.Map.GetTile(cursorPosition)))
                        {
                            foreach (Unit unit in HeavyGearManager.ActivePlayer.Units)
                            {
                                if (unit.MapPosition == cursorPosition)
                                    return false;
                            }

                            activeUnit.MapPosition = cursorPosition;
                            facingMode = true;
                        }
                    }
                }

                #endregion

                return false;
            }
            #endregion

            #region Move Mode
            if (moveMode)
            {
                TextureFont.WriteText(BaseGame.XToRes1600(700), BaseGame.YToRes1200(50), "Movement");

                #region Build highlighted tiles
                if (highlightedTiles == null)
                {
                    highlightedTiles = HeavyGearManager.Map.GetMoveArea(activeUnit.MapPosition, activeUnit.MP, activeUnit.UnitType);
                    foreach (HexTile tile in highlightedTiles)
                        tile.Highlight = true;
                }
                #endregion

                #region Handle Input
                if (!inputTimeOut)
                {
                    if (Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed || Input.MouseLeftButtonJustPressed)
                    {
                        if (highlightedTiles.Contains(HeavyGearManager.Map.GetTile(cursorPosition.X, cursorPosition.Y)))
                        {

                            activeUnit.MoveUnit(HeavyGearManager.Map.GetMovePath(activeUnit.MapPosition, cursorPosition));
                            foreach (HexTile tile in highlightedTiles)
                                tile.Highlight = false;
                            highlightedTiles = null;
                            moveMode = false;
                            facingMode = true;
                            //unitMoving = true;
                            inputTimeOut = true;
                            return false;
                        }
                    }
                    if (Input.GamePadBJustPressed || Input.KeyboardEscapeJustPressed || Input.MouseRightButtonJustPressed)
                    {
                        foreach (HexTile tile in highlightedTiles)
                            tile.Highlight = false;
                        highlightedTiles = null;
                        moveMode = false;
                        inputTimeOut = true;
                        return false;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            #region Attack/Observe/Acquire Mode
            if (attackMode || observeMode || acquireMode)
            {
                TextureFont.WriteText(BaseGame.XToRes1600(700), BaseGame.YToRes1200(50), "Select Target");

                Weapon weaponToUse = activeUnit.Weapons[activeUnit.Weapon];

                #region Build list of tiles in range
                List<Player> opponents = new List<Player>();

                for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                {
                    Player player = HeavyGearManager.Player(i);
                    if (player.PlayerIndex != HeavyGearManager.ActivePlayer.PlayerIndex)
                        opponents.Add(player);
                }

                if (LOSArea == null)
                    HeavyGearManager.Map.GetLOSArea(activeUnit, out LOSArea);

                //foreach (HexTile tile in LOSArea)
                //    tile.Highlight = true;

                if (highlightedTiles == null)
                {
                    List<HexTile> tilesInRange = new List<HexTile>();

                    if (attackMode)
                        tilesInRange = HeavyGearManager.Map.GetArea(activeUnit.MapPosition, weaponToUse.Range * 8);
                    else if (acquireMode)
                    {
                        int sightRange = 0;
                        if (activeUnit.UnitType != UnitType.Infantry)
                        {
                            if (!((Vehicle)activeUnit).SensorsDestroyed)
                                sightRange = ((Vehicle)activeUnit).SensorRange;
                            else
                                sightRange = 20;
                        }
                        tilesInRange = HeavyGearManager.Map.GetArea(activeUnit.MapPosition, sightRange);
                    }
                    else
                    {
                        tilesInRange = HeavyGearManager.Map.GetArea(activeUnit.MapPosition, 20);
                    }

                    if (acquireMode)
                        highlightedTiles = tilesInRange;
                    else
                    {
                        highlightedTiles = new List<HexTile>();
                        foreach (HexTile tile in tilesInRange)
                        {
                            if (LOSArea.Contains(tile))
                                highlightedTiles.Add(tile);
                        }
                        //Add in observed units for guided and indirect weapons
                        if (attackMode && (weaponToUse.Guided || weaponToUse.IndirectFire))
                        {
                            foreach (Unit unit in HeavyGearManager.ActivePlayer.Units)
                            {
                                if (weaponToUse.Guided)
                                {
                                    if (unit.TaggedUnit != null)
                                    {
                                        HexTile tileToAdd = HeavyGearManager.Map.GetTile(
                                        unit.TaggedUnit.MapPosition.X,
                                        unit.TaggedUnit.MapPosition.Y);
                                        if (!highlightedTiles.Contains(tileToAdd))
                                            highlightedTiles.Add(tileToAdd);
                                    }
                                }
                                else
                                {
                                    if (unit.ObservedUnit != null)
                                    {
                                        HexTile tileToAdd = HeavyGearManager.Map.GetTile(
                                            unit.ObservedUnit.MapPosition.X,
                                            unit.ObservedUnit.MapPosition.Y);
                                        if (!highlightedTiles.Contains(tileToAdd))
                                            highlightedTiles.Add(tileToAdd);
                                    }
                                }
                            }
                        }
                    }

                    foreach (HexTile tile in highlightedTiles)
                    {
                        tile.Highlight = true;
                    }
                }

                #endregion

                #region Input
                if (!inputTimeOut)
                {
                    if (Input.GamePadAJustPressed || Input.KeyboardKeyJustPressed(Keys.Enter) || Input.MouseLeftButtonJustPressed)
                    {
                        inputTimeOut = true;
                        if (selectedUnit != null && selectedUnit.PlayerIndex != HeavyGearManager.ActivePlayer.PlayerIndex &&
                            selectedUnit.IsAlive)
                        {
                            if (attackMode)
                            {
                                foreach (HexTile tile in highlightedTiles)
                                {
                                    if (tile.MapPosition == selectedUnit.MapPosition)
                                    {
                                        activeUnit.Attack(selectedUnit);
                                        attackMode = false;
                                    }
                                }
                            }
                            else if (acquireMode)
                            {
                                foreach (HexTile tile in highlightedTiles)
                                {
                                    if (tile.MapPosition == selectedUnit.MapPosition)
                                    {
                                        ((Vehicle)activeUnit).AcquireUnit(selectedUnit);
                                        acquireMode = false;
                                    }
                                }

                            }
                            else if (observeMode)
                            {
                                foreach (HexTile tile in highlightedTiles)
                                {
                                    if (tile.MapPosition == selectedUnit.MapPosition)
                                    {
                                        activeUnit.ObserveUnit(selectedUnit);
                                        observeMode = false;
                                    }
                                }

                            }
                        }
                        if (!attackMode && !observeMode && !acquireMode)
                        {
                            foreach (HexTile tile in highlightedTiles)
                                tile.Highlight = false;
                            highlightedTiles = null;
                            activeUnit = null;
                            LOSArea = null;
                            return false;
                        }
                    }

                    if (Input.GamePadBJustPressed || Input.KeyboardEscapeJustPressed || Input.MouseRightButtonJustPressed)
                    {
                        foreach (HexTile tile in highlightedTiles)
                            tile.Highlight = false;
                        attackMode = observeMode = acquireMode = false;
                        activeUnit = null;
                        LOSArea = null;
                        inputTimeOut = true;
                        highlightedTiles = null;
                        //selectedUnit = null;
                    }
                }
                #endregion

                return false;
            }
            #endregion

            #region Normal Mode
            
            #region Handle actions
            if (!inputTimeOut)
            {
                if (Input.GamePad.Buttons.LeftShoulder == ButtonState.Pressed || Input.KeyboardSpaceJustPressed)
                    HeavyGearManager.Camera.MapPosition = cursorPosition;

#if DEBUG
                if (Input.Keyboard.IsKeyDown(Keys.F11) ||
                        Input.GamePad.Buttons.LeftShoulder == ButtonState.Pressed)
                {
                    inputTimeOut = true;
                    HeavyGearManager.Map.ShowHexType = !HeavyGearManager.Map.ShowHexType;
                    HeavyGearManager.Map.ShowElevation = !HeavyGearManager.Map.ShowElevation;
                    HeavyGearManager.Map.ShowPosition = !HeavyGearManager.Map.ShowPosition;
                }
#endif

                //open Action menu if a unit is under the cursor
                if (selectedUnit != null)
                {
                    if (!selectedUnit.HasUsedActions && selectedUnit.PlayerIndex == HeavyGearManager.ActivePlayer.PlayerIndex &&
                            selectedUnit.IsAlive && 
                            (!HeavyGearManager.Transport.IsConnected || 
                            HeavyGearManager.LocalPlayer.PlayerIndex == HeavyGearManager.ActivePlayer.PlayerIndex)
                        )
                    {
                        if (Input.GamePad.Buttons.A == ButtonState.Pressed || Input.KeyboardKeyJustPressed(Keys.Enter) ||
                            Input.MouseLeftButtonJustPressed)
                        {
                            showActionMenu = true;
                            selectedButton = 0;
                            inputTimeOut = true;
                            activeUnit = selectedUnit;
                        }
                        if (Input.GamePad.Buttons.X == ButtonState.Pressed || Input.Keyboard.IsKeyDown(Keys.Tab) ||
                            Input.MouseRightButtonJustPressed)
                        {
                            showWeaponMenu = true;
                            inputTimeOut = true;
                            selectedButton = selectedUnit.Weapon;
                            activeUnit = selectedUnit;
                        }
                    }


                    //Toggles the selected units LOS
                    if (Input.Keyboard.IsKeyDown(Keys.F10) ||
                        Input.GamePad.Buttons.RightShoulder == ButtonState.Pressed)
                    {
                        inputTimeOut = true;
                        if (LOSArea == null)
                        {
                            HeavyGearManager.Map.GetLOSArea(selectedUnit, out LOSArea);
                            foreach (HexTile tile in LOSArea)
                                tile.Highlight = true;
                        }
                        else
                        {
                            foreach (HexTile tile in LOSArea)
                                tile.Highlight = false;
                            LOSArea = null;
                        }
                    }
                    
                }
                else
                {
                    //Show side actions menu
                    if (Input.GamePad.Buttons.A == ButtonState.Pressed || Input.KeyboardKeyJustPressed(Keys.Enter) ||
                            Input.MouseLeftButtonJustPressed)
                    {
                        showSideActionMenu = true;
                        selectedButton = 0;
                        inputTimeOut = true;
                    }
                }
                //Show pause menu
                if (Input.GamePad.Buttons.Start == ButtonState.Pressed || Input.KeyboardEscapeJustPressed)
                {
                    selectedButton = 0;
                    pause = true;
                    inputTimeOut = true;
                }
            }
            #endregion

            return false;
            #endregion

        }
        #endregion
    }
}
