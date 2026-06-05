#region File Description
//-----------------------------------------------------------------------------
// NetworkScreen.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using HeavyGear.Net;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using HeavyGear.GameLogic;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
using Texture = HeavyGear.Graphics.Texture;
using HeavyGear.Properties;
#endregion

namespace HeavyGear.GameScreens
{
    /// <summary>
    /// Army selection
    /// </summary>
    /// <returns>IGame screen</returns>
    public class NetworkScreen : IGameScreen
    {
        #region Constants

        static string[]UnitNames;
        static int[][] ThreatValues;
        static Rectangle[][] PortraitRect;

        #endregion

        #region Variables

        // Network objects.
        // LiteNetLib session discovery replaces AvailableNetworkSessionCollection (Phase 6)
        static System.Collections.Generic.List<string> availableSessions = new();

        static Texture[] mapPreview;
        
        int startIndex = 0;
        int selectedButton = 0;

        bool ignoreMouse = true;

        bool waitingForPlayers = false;
        bool mapSelect = false;
        bool armySelect = false;
        bool unitSelect = false;
        bool startPacketSent = false;

        bool armyDone = false;

        static bool gamesRefreshed = true;

        static bool resourcesLoaded = false;
        static int numFilesLoaded = 0;

        private ArmyType armyType;
        private int threatValue = 0;
        private int[] unitCount = new int[] { 0, 0, 0, 0, 0};

        #endregion

        #region LoadResources
        ThreadStart LoadResources = delegate
        {
            //Construct portrait rects
            PortraitRect = new Rectangle[2][];
            PortraitRect[0] = new Rectangle[]
            {
                UIRenderer.PortraitCheetahGfxRect,
                UIRenderer.PortraitHunterGfxRect,
                UIRenderer.PortraitJaguarGfxRect,
                UIRenderer.PortraitGrizzlyGfxRect,
                UIRenderer.PortraitMammothGfxRect,
                UIRenderer.PortraitScorpionGfxRect,
                UIRenderer.PortraitAllerGfxRect,
                UIRenderer.PortraitInfantryGfxRect
            };
            PortraitRect[1] = new Rectangle[]
            {
                UIRenderer.PortraitIguanaGfxRect,
                UIRenderer.PortraitJagerGfxRect,
                UIRenderer.PortraitBlackMambaGfxRect,
                UIRenderer.PortraitSpittingCobraGfxRect,
                UIRenderer.PortraitNagaGfxRect,
                UIRenderer.PortraitTitanGfxRect,
                UIRenderer.PortraitStonewallGfxRect,
                UIRenderer.PortraitInfantryGfxRect
            };

            UnitNames = new string[] { "Standard Gear Squad", "Support Gear Squad", "Special Ops Gear Squad", "Strider Squad", "Infantry Platoon" };

            ThreatValues = new int[2][];
            ThreatValues[0] = new int[] { Squads.NorthStandardGear(0, 0).ThreatValue, Squads.NorthSupportGear(0,0).ThreatValue, Squads.NorthSpecialOpsGear(0,0).ThreatValue,
             Squads.NorthStrider(0, 0).ThreatValue, Squads.NorthInfantry(0,0).ThreatValue};
            ThreatValues[1] = new int[] { Squads.SouthStandardGear(0, 0).ThreatValue, Squads.SouthSupportGear(0,0).ThreatValue, Squads.SouthSpecialOpsGear(0,0).ThreatValue,
             Squads.SouthStrider(0, 0).ThreatValue, Squads.SouthInfantry(0,0).ThreatValue};


            
            mapPreview = new Texture[HeavyGearManager.Maps.Length];
            for (int i = 0; i < HeavyGearManager.Maps.Length; i++)
            {
                string previewName = Path.GetFileNameWithoutExtension(
                    HeavyGearManager.MapFileNames[i]) + "Preview";
                string fullFilename = "Maps/" + previewName;

                Texture2D texture = BaseGame.Content.Load<Texture2D>(fullFilename);
                mapPreview[i] = new Texture(texture);
                numFilesLoaded++;
            }
            resourcesLoaded = true;
        };

        #endregion

        #region RefreshGames
        ThreadStart RefreshGames = delegate
        {
            var found = new System.Collections.Generic.List<(System.Net.IPEndPoint ep, string name)>();
            LiteNetLibTransport.SessionDiscovered += (ep, name) => { lock (found) found.Add((ep, name)); };
            LiteNetLibTransport.DiscoverLAN();
            availableSessions.Clear();
            lock (found)
                foreach (var (ep, name) in found)
                    availableSessions.Add(ep.Address.ToString());
            if (availableSessions.Count == 0)
                HeavyGearManager.ErrorMessage = "No network sessions found.";
            gamesRefreshed = true;
        };

        #endregion

        #region Constructor
        public NetworkScreen()
        {
            Thread loadThread = new Thread(LoadResources);
            loadThread.Start();
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        /// <returns>Bool</returns>
        public bool Render()
        {
            int mouseIsOverButton = -1;

            // If the user manipulated the mouse, stop ignoring the mouse
            // This allows the mouse to override the game pad or keyboard selection
            if (Input.HasMouseMoved || Input.MouseLeftButtonJustPressed)
                ignoreMouse = false;

            // Immediately paint here, else post screen UI will
            // be drawn over!
            Texture.additiveSprite.End();
            Texture.alphaSprite.End();
            //SpriteHelper.DrawAllSprites();

            // Restart the sprites after the paint
            Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //No menu background here

            #region LoadScreen
            if (!resourcesLoaded)
            {
                Rectangle messageRect = new Rectangle();
                messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                    "Loading Maps");
                TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(80),
                    numFilesLoaded + " / " + HeavyGearManager.Maps.Length);

                return false;
            }
            #endregion

            #region RefreshScreen
            if (!gamesRefreshed)
            {
                Rectangle messageRect = new Rectangle();
                messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                    "Refreshing Network Games...");

                return false;
            }
            #endregion

            #region WaitingForPlayers
            if (waitingForPlayers)
            {
                if (HeavyGearManager.Transport.IsConnected)
                {
                    waitingForPlayers = false;
                    mapSelect = true;
                    HeavyGearManager.PlayerReady = new bool[] { false, false, false, false };
                }
                else
                {
                    int playersReady = 0;
                    for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                    {
                        if (HeavyGearManager.PlayerReady[i])
                            playersReady++;
                    }
                    
                    if (playersReady == HeavyGearManager.NumberOfPlayers && 
                        HeavyGearManager.Transport.IsHost)
                        HeavyGearManager.Transport.StartGame();

                    if (Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        if (!HeavyGearManager.PlayerReady[HeavyGearManager.LocalPlayer.PlayerIndex])
                            HeavyGearManager.SendMenuPacket(HeavyGearManager.LocalPlayer.PlayerIndex);
                    }
                    else if (Input.GamePadBJustPressed || Input.KeyboardEscapeJustPressed)
                    {
                        if (HeavyGearManager.PlayerReady[HeavyGearManager.LocalPlayer.PlayerIndex])
                            HeavyGearManager.SendMenuPacket(HeavyGearManager.LocalPlayer.PlayerIndex);
                    }

                    Rectangle messageRect = new Rectangle();
                    messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                    messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                    messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                    messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                    BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                        "Waiting for all players to be ready");
                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(80),
                        playersReady + " / " + HeavyGearManager.NumberOfPlayers + " players ready");
                }

                return false;
            }
            #endregion

            #region StartGame

            if (HeavyGearManager.StartGame)
            {
                Rectangle messageRect = new Rectangle();
                messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(80),
                    "Starting Game");

                HeavyGearManager.AddGameScreen(new GameScreen());

                return false;
            }

            #endregion

            //Display menu background for the rest of the menus

            #region Menu Background
            //render menu background
            Rectangle menuBackground = BaseGame.ResolutionRect;
#if XBOX360
            menuBackground.Y += BaseGame.YToRes(20);
            menuBackground.Height -= BaseGame.YToRes(50);
#endif
            BaseGame.UI.MenuUI.RenderOnScreen(menuBackground, UIRenderer.MenuBackgroundGfxRect);

            //(200, 122), (824, 122), (200, 526), (824, 526) Selection Box
            BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(122), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Top segment
            BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Left Segment
            BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(526), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Bottom segment
            BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(824), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Right Segment

            string name = "";
            if (HeavyGearManager.LocalPlayer != null)
                name = HeavyGearManager.LocalPlayer.Name;
            TextureFont.WriteText(BaseGame.XToRes(210), menuBackground.Y + BaseGame.YToRes(65), name);
            #endregion

            #region NetworkLobby
            if (!HeavyGearManager.Transport.IsConnected)
            {
                #region Display

                // Desktop: no sign-in required
                TextureFont.WriteText(BaseGame.XToRes(103), menuBackground.Y + BaseGame.YToRes(16), "Select Network Game");//115, 23 for menu title

                //BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(301), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line

                BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(223), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(324), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(425), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line

                //Display the list of available games
                int rowX = BaseGame.XToRes(202);
                int rowY = BaseGame.YToRes(124);

                if (availableSessions == null)
                {
                    gamesRefreshed = false;
                    Thread refreshThread = new Thread(RefreshGames);
                    refreshThread.Start();

                    return false;
                }

                if (availableSessions.Count > 4)
                {
                    if (startIndex < 0)
                        startIndex = 0;
                    if (startIndex > availableSessions.Count - 4)
                        startIndex = availableSessions.Count - 4;
                    if (selectedButton < startIndex)
                        startIndex = selectedButton;
                    if (selectedButton > startIndex + 3)
                        startIndex = selectedButton - 3;
                }
                else
                {
                    startIndex = 0;
                }

                for (int i = startIndex; i < startIndex + 4 && i < availableSessions.Count; i++)
                {
                    Rectangle selectionRect = new Rectangle(
                            rowX,
                            rowY + BaseGame.YToRes(101 * (i - startIndex)),
                            BaseGame.XToRes(624),
                            BaseGame.YToRes(101));

                    if (i == selectedButton)
                        BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);

                    if (Input.MouseInBox(selectionRect))
                        mouseIsOverButton = i;

                    // TODO-MG Phase 6: display session info from LiteNetLib
                    // TextureFont.WriteText(x, y, session.HostGamertag);
                    // TextureFont.WriteText(x, y, session.QualityOfService);
                    // TextureFont.WriteText(x, y, session.CurrentGamerCount.ToString());
                }

                if (!ignoreMouse && mouseIsOverButton >= 0)
                    selectedButton = mouseIsOverButton;

                #region ControllerText
                TextureFont.WriteText(BaseGame.XToRes(120), menuBackground.Y + BaseGame.YToRes(590), "JOIN : A ");
                TextureFont.WriteText(BaseGame.XToRes(250), menuBackground.Y + BaseGame.YToRes(590), "CREATE : X ");
                #endregion

                #endregion

                if (Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                {
                    //Join existing
                    if (availableSessions.Count > 0)
                    {
                        string hostIp = availableSessions[startIndex < availableSessions.Count ? startIndex : 0];
                        HeavyGearManager.JoinSession(LiteNetLibTransport.Connect(hostIp));
                        waitingForPlayers = true;
                    }
                }
                else if (Input.GamePadXJustPressed || Input.Keyboard.IsKeyDown(Keys.Enter))
                {
                    // Create a new session
                    HeavyGearManager.CreateSession(LiteNetLibTransport.CreateHost());
                    waitingForPlayers = true;
                }

                if (Input.GamePadYJustPressed || Input.Keyboard.IsKeyDown(Keys.Tab))
                {
                    gamesRefreshed = false;
                    Thread refreshThread = new Thread(RefreshGames);
                    refreshThread.Start();

                    return false;
                }

                return false;
            }
            #endregion

            #region MapSelect
            if (mapSelect)
            {
                if (HeavyGearManager.Transport.IsHost)
                {

                    #region Display

                    TextureFont.WriteText(BaseGame.XToRes(103), menuBackground.Y + BaseGame.YToRes(16), "Select Map");//115, 23 for menu title

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(301), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(223), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(324), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(425), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line

                    //Display the list of maps
                    int rowX = BaseGame.XToRes(202);
                    int rowY = BaseGame.YToRes(124);

                    if (HeavyGearManager.Maps.Length > 4)
                    {
                        if (startIndex < 0)
                            startIndex = 0;
                        if (startIndex > HeavyGearManager.Maps.Length - 4)
                            startIndex = HeavyGearManager.Maps.Length - 4;
                        if (selectedButton < startIndex)
                            startIndex = selectedButton;
                        if (selectedButton > startIndex + 3)
                            startIndex = selectedButton - 3;
                    }
                    else
                    {
                        startIndex = 0;
                    }

                    for (int i = startIndex; i < startIndex + 4 && i < HeavyGearManager.Maps.Length; i++)
                    {
                        Rectangle selectionRect = new Rectangle(
                                rowX,
                                rowY + BaseGame.YToRes(101 * (i - startIndex)),
                                BaseGame.XToRes(624),
                                BaseGame.YToRes(101));

                        if (i == selectedButton)
                            BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);

                        if (Input.MouseInBox(selectionRect))
                            mouseIsOverButton = i;

                        Rectangle prevRect = new Rectangle(rowX + BaseGame.XToRes(5), rowY + BaseGame.YToRes(5 + 101 * (i - startIndex)), BaseGame.XToRes(91), BaseGame.YToRes(91));

                        mapPreview[i].RenderOnScreen(prevRect);

                        TextureFont.WriteText(
                            BaseGame.XToRes(350),
                            rowY + BaseGame.YToRes(10 + (101 * (i - startIndex))),
                            HeavyGearManager.Maps[i].Name);
                        TextureFont.WriteText(
                            BaseGame.XToRes(350),
                            rowY + BaseGame.YToRes(50 + (101 * (i - startIndex))),
                            HeavyGearManager.Maps[i].Description);
                        TextureFont.WriteText(
                            BaseGame.XToRes(700),
                            rowY + BaseGame.YToRes(10 + (101 * (i - startIndex))),
                            HeavyGearManager.Maps[i].ThreatValue.ToString());


                    }

                    if (!ignoreMouse && mouseIsOverButton >= 0)
                        selectedButton = mouseIsOverButton;

                    //Controller Text
                    //TODO: Show controller button
                    //TextureFont.WriteText(BaseGame.XToRes(182), menuBackground.Bottom - BaseGame.YToRes(40), "DONE");

                    #endregion

                    #region Input

                    // Handle GamePad input, and also allow keyboard input
                    if (Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        //Sound.Play(Sound.Sounds.ButtonClick);
                        if (selectedButton > 0)
                            selectedButton--;
                        //ignoreMouse = true;
                    }
                    else if (Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        //Sound.Play(Sound.Sounds.ButtonClick);
                        if (selectedButton < HeavyGearManager.Maps.Length - 1)
                            selectedButton++;
                        //ignoreMouse = true;
                    }

                    // If user presses the mouse button or the game pad A or Space,
                    // select that map.
                    if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0) || Input.GamePadAJustPressed ||
                        Input.KeyboardSpaceJustPressed)
                    {
                        HeavyGearManager.Map = new Map(HeavyGearManager.MapFileNames[selectedButton]);
                        HeavyGearManager.SendMenuPacket(-1);
                        mapSelect = false;
                        armySelect = true;
                        startIndex = 0;
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
                        "Host is choosing the map");

                    if (HeavyGearManager.Map != null)
                    {
                        mapSelect = false;
                        armySelect = true;
                        startIndex = 0;
                    }
                }

                return false;
            }
            #endregion

            #region ArmySelect

            if (armySelect)
            {
                #region Display Army Info

                BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(512), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line

                TextureFont.WriteText(BaseGame.XToRes(103), menuBackground.Y + BaseGame.YToRes(16), "Select Army");//115, 23 for menu title

                Rectangle renderRect;
                Rectangle NLCSRect = new Rectangle(
                        BaseGame.XToRes(200),
                        BaseGame.YToRes(122),
                        BaseGame.XToRes(312),
                        BaseGame.YToRes(404)
                        );
                Rectangle ASTRect = new Rectangle(
                        BaseGame.XToRes(512),
                        BaseGame.YToRes(122),
                        BaseGame.XToRes(312),
                        BaseGame.YToRes(404)
                        );
                if (HeavyGearManager.LocalPlayer.ArmyIndex == (int)ArmyType.NLCS)
                {
                    renderRect = NLCSRect;
                }
                else
                {
                    renderRect = ASTRect;
                }

                BaseGame.UI.MenuUI.RenderOnScreen(renderRect, UIRenderer.SelectionGfxRect);

                if (Input.MouseInBox(NLCSRect))
                    mouseIsOverButton = 0;
                else if (Input.MouseInBox(ASTRect))
                    mouseIsOverButton = 1;

                if (!ignoreMouse && mouseIsOverButton >= 0)
                    HeavyGearManager.LocalPlayer.ArmyIndex = mouseIsOverButton;

                TextureFont.WriteText(
                        BaseGame.XToRes(202), menuBackground.Y + BaseGame.YToRes(130),
                        "Northern Lights Confederacy");

                TextureFont.WriteText(
                        BaseGame.XToRes(518), menuBackground.Y + BaseGame.YToRes(130),
                        "Allied Southern Territories");

                Rectangle NLCSPortraitRect = new Rectangle(NLCSRect.X, NLCSRect.Bottom - BaseGame.YToRes(312), BaseGame.XToRes(312), BaseGame.YToRes(312));
                Rectangle ASTPortraitRect = new Rectangle(ASTRect.X, ASTRect.Bottom - BaseGame.YToRes(312), BaseGame.XToRes(312), BaseGame.YToRes(312));

                BaseGame.UI.Portrait.RenderOnScreen(NLCSPortraitRect, UIRenderer.PortraitHunterGfxRect);
                BaseGame.UI.Portrait.RenderOnScreen(ASTPortraitRect, UIRenderer.PortraitJagerGfxRect);
                #endregion

                #region Handle Player Input

                if (Input.GamePadLeftJustPressed || Input.KeyboardLeftJustPressed)
                {
                    //Sound.Play(Sound.Sounds.Highlight);
                    HeavyGearManager.LocalPlayer.ArmyIndex =
                        (HeavyGearManager.LocalPlayer.ArmyIndex + 1) % 2;
                }
                else if (Input.GamePadRightJustPressed || Input.KeyboardRightJustPressed)
                {
                    //Sound.Play(Sound.Sounds.Highlight);
                    HeavyGearManager.LocalPlayer.ArmyIndex =
                        (HeavyGearManager.LocalPlayer.ArmyIndex + 3) % 2;
                }

                if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0) ||
                    Input.GamePadAJustPressed || Input.KeyboardSpaceJustPressed)
                {
                    armySelect = false;
                    unitSelect = true;
                }

                #endregion

                return false;
            }

            #endregion

            #region UnitSelect
            if (unitSelect)
            {
                if (armyDone)
                {
                    if (!HeavyGearManager.PlayerReady[HeavyGearManager.LocalPlayer.PlayerIndex])
                        HeavyGearManager.SendMenuPacket(HeavyGearManager.LocalPlayer.PlayerIndex);
                    int playersReady = 0;
                    for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                    {
                        if (HeavyGearManager.PlayerReady[i])
                            playersReady++;
                    }

                    if (playersReady == HeavyGearManager.NumberOfPlayers &&
                        HeavyGearManager.Transport.IsHost &&
                        !startPacketSent)
                    {
                        startPacketSent = true;
                        HeavyGearManager.SendStartGamePacket();
                    }

                    Rectangle messageRect = new Rectangle();
                    messageRect.X = BaseGame.XToRes(512) - BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width) / 2;
                    messageRect.Y = BaseGame.YToRes(320) - BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height) / 2;
                    messageRect.Width = BaseGame.XToRes(UIRenderer.MessageBoxGfxRect.Width);
                    messageRect.Height = BaseGame.YToRes(UIRenderer.MessageBoxGfxRect.Height);

                    BaseGame.UI.MenuUI.RenderOnScreen(messageRect, UIRenderer.MessageBoxGfxRect);

                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(40),
                        "Waiting for all players to be ready");
                    TextureFont.WriteTextCentered(messageRect.X + messageRect.Width / 2, messageRect.Y + BaseGame.YToRes(80),
                        playersReady + " / " + HeavyGearManager.NumberOfPlayers + " players ready");
                }
                else
                {

                    #region Display

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(301), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(723), BaseGame.YToRes(122), BaseGame.XToRes(2), BaseGame.YToRes(404)), UIRenderer.MenuColorGfxRect); //Divider Line

                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(223), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(324), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line
                    BaseGame.UI.MenuUI.RenderOnScreen(new Rectangle(BaseGame.XToRes(200), BaseGame.YToRes(425), BaseGame.XToRes(624), BaseGame.YToRes(2)), UIRenderer.MenuColorGfxRect); //Divider Line

                    TextureFont.WriteText(BaseGame.XToRes(103), menuBackground.Y + BaseGame.YToRes(16), "Select Squads");//115, 23 for menu title

                    #endregion

                    #region Get Player Info

                    armyType = (ArmyType)HeavyGearManager.LocalPlayer.ArmyIndex;

                    threatValue = 0;

                    int rowX = BaseGame.XToRes(202);
                    int rowY = BaseGame.YToRes(124);
                    int rowWidth = BaseGame.XToRes(624);
                    int rowHeight = BaseGame.YToRes(101);

                    if (startIndex > UnitNames.Length - 4)
                        startIndex = UnitNames.Length - 4;
                    if (startIndex < 0)
                        startIndex = 0;
                    if (selectedButton < startIndex)
                        startIndex = selectedButton;
                    if (selectedButton > startIndex + 3)
                        startIndex = selectedButton - 3;

                    //display up or down arrows and check for mouse over
                    if (startIndex > 0)
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

                    if (startIndex < UnitNames.Length - 3)
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

                    //calculate player's total threat value
                    for (int i = 0; i < UnitNames.Length; i++)
                    {
                        if (unitCount[i] > 0)
                        {

                            threatValue += unitCount[i] * ThreatValues[HeavyGearManager.LocalPlayer.ArmyIndex][i];
                        }

                    }


                    //Show 4 units in the list, starting with startIndex
                    for (int j = startIndex; j < startIndex + 4 && j < UnitNames.Length - 3; j++)
                    {

                        Rectangle selectionRect = new Rectangle(rowX, rowY + rowHeight * (j - startIndex), rowWidth, rowHeight);

                        if (Input.MouseInBox(selectionRect))
                            mouseIsOverButton = j;

#if XBOX360
                        if (selectedButton == j)
                            BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);
#else
                        if (selectedButton == j && mouseIsOverButton >= 0 && mouseIsOverButton < UnitNames.Length)
                            BaseGame.UI.MenuUI.RenderOnScreen(selectionRect, UIRenderer.SelectionGfxRect);
#endif

                        BaseGame.UI.Portrait.RenderOnScreen(
                                new Rectangle(rowX, rowY + rowHeight * (j - startIndex), BaseGame.XToRes(101), rowHeight),
                                PortraitRect[HeavyGearManager.LocalPlayer.ArmyIndex][j]
                                );
                        TextureFont.WriteText(rowX + BaseGame.XToRes(126), rowY + rowHeight * (j - startIndex) + BaseGame.YToRes(40), UnitNames[j]);
                        TextureFont.WriteText(rowX + BaseGame.XToRes(326), rowY + rowHeight * (j - startIndex) + BaseGame.YToRes(40), ThreatValues[HeavyGearManager.LocalPlayer.ArmyIndex][j].ToString());
                        TextureFont.WriteText(rowX + rowWidth - BaseGame.XToRes(51), rowY + rowHeight * (j - startIndex) + BaseGame.YToRes(40), unitCount[j].ToString());

                    }

#if !XBOX360
                    Rectangle doneButton = new Rectangle(BaseGame.XToRes(750), BaseGame.YToRes(590), UIRenderer.MenuItemGfxRect.Width, UIRenderer.MenuItemGfxRect.Height);

                    if (Input.MouseInBox(doneButton))
                    {
                        mouseIsOverButton = 100;
                        BaseGame.UI.MenuUI.RenderOnScreen(doneButton, UIRenderer.MenuItemSelectGfxRect);
                    }
                    else
                        BaseGame.UI.MenuUI.RenderOnScreen(doneButton, UIRenderer.MenuItemGfxRect);

                    TextureFont.WriteTextCentered(doneButton.X + doneButton.Width / 2, doneButton.Y + doneButton.Height / 2, "DONE");
#endif
                    #endregion

                    #region Headers
                    TextureFont.WriteText(BaseGame.XToRes(610), menuBackground.Y + BaseGame.YToRes(65), "Threat Value: " + threatValue);
                    TextureFont.WriteText(BaseGame.XToRes(460), menuBackground.Y + BaseGame.YToRes(105), "Unit");
                    TextureFont.WriteText(BaseGame.XToRes(750), menuBackground.Y + BaseGame.YToRes(105), "Total");
                    TextureFont.WriteText(BaseGame.XToRes(585), menuBackground.Y + BaseGame.YToRes(550), "MAX Threat Value: " + HeavyGearManager.Map.ThreatValue);
                    #endregion

                    #region Player Input
                    if (!ignoreMouse && mouseIsOverButton >= 0 && mouseIsOverButton < UnitNames.Length)
                        selectedButton = mouseIsOverButton;

                    // handle xbox controller input for first player, NEEDS DONE BUTTON FOR MOUSE


                    if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 200) ||
                        Input.GamePadUpJustPressed || Input.KeyboardUpJustPressed)
                    {
                        if (selectedButton > 0)
                            selectedButton--;
                    }
                    else if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 300) ||
                        Input.GamePadDownJustPressed || Input.KeyboardDownJustPressed)
                    {
                        if (selectedButton < UnitNames.Length - 1)
                            selectedButton++;
                    }
                    else if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton == 100) ||
                        Input.GamePadXJustPressed || Input.KeyboardSpaceJustPressed)
                    {
                        if (threatValue <= HeavyGearManager.Map.ThreatValue && threatValue > 0)
                        {
                            BuildArmy(HeavyGearManager.LocalPlayer);
                            armyDone = true;
                        }
                    }
                    else
                    {
                        if ((Input.MouseLeftButtonJustPressed && mouseIsOverButton >= 0 && mouseIsOverButton < UnitNames.Length) ||
                            Input.GamePadRightJustPressed || Input.GamePadAJustPressed || Input.KeyboardRightJustPressed)
                        {
                            unitCount[selectedButton]++;
                        }
                        else if ((Input.MouseRightButtonJustPressed && mouseIsOverButton >= 0 && mouseIsOverButton < UnitNames.Length) ||
                            Input.GamePadLeftJustPressed || Input.GamePadBJustPressed || Input.KeyboardLeftJustPressed)
                        {
                            if (unitCount[selectedButton] > 0)
                                unitCount[selectedButton]--;
                        }
                    }

                    if (Input.GamePadYJustPressed || Input.KeyboardEscapeJustPressed)
                    {
                        armySelect = true;
                        unitSelect = false;
                        selectedButton = 0;
                        startIndex = 0;
                        for (int i = 0; i < UnitNames.Length; i++)
                            unitCount[i] = 0;
                    }
                    #endregion
                }
                return false;
            }
            #endregion

            return false;
        }
        #endregion

        #region Build Army

        private void BuildArmy(Player player)
        {
            List<Squad> squads = new List<Squad>();
            int squadIndex = 0;
            for (int unitType = UnitNames.Length; unitType >= 0; unitType--)
            {
                for (int j = 0; j < unitCount[unitType]; j++)
                {
                    switch (unitType)
                    {
                        case 0:
                            if (armyType == ArmyType.NLCS)
                                squads.Add(Squads.NorthStandardGear(player.PlayerIndex, squadIndex));
                            else
                                squads.Add(Squads.SouthStandardGear(player.PlayerIndex, squadIndex));
                            break;
                        case 1:
                            if (armyType == ArmyType.NLCS)
                                squads.Add(Squads.NorthSupportGear(player.PlayerIndex, squadIndex));
                            else
                                squads.Add(Squads.SouthSupportGear(player.PlayerIndex, squadIndex));
                            break;
                        case 2:
                            if (armyType == ArmyType.NLCS)
                                squads.Add(Squads.NorthSpecialOpsGear(player.PlayerIndex, squadIndex));
                            else
                                squads.Add(Squads.SouthSpecialOpsGear(player.PlayerIndex, squadIndex));
                            break;
                        case 3:
                            if (armyType == ArmyType.NLCS)
                                squads.Add(Squads.NorthStrider(player.PlayerIndex, squadIndex));
                            else
                                squads.Add(Squads.SouthStrider(player.PlayerIndex, squadIndex));
                            break;
                        case 4:
                            if (armyType == ArmyType.NLCS)
                                squads.Add(Squads.NorthInfantry(player.PlayerIndex, squadIndex));
                            else
                                squads.Add(Squads.SouthInfantry(player.PlayerIndex, squadIndex));
                            break;
                    }
                    squadIndex++;
                }
            }


            player.Squads = squads;
        }

        #endregion
    }
}
