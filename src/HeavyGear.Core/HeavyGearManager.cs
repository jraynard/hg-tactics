#region File Description
//-----------------------------------------------------------------------------
// HeavyGearManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified By
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using HeavyGear.GameLogic;
using HeavyGear.GameScreens;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Net;
using HeavyGear.Sounds;
using HeavyGear.Shaders;
using HeavyGear.Properties;
using Texture = HeavyGear.Graphics.Texture;
#endregion

namespace HeavyGear
{
    public enum PacketType
    {
        Normal,
        Deploy,
        Menu,
        Attack,
        Target,
        StartTurn,
        StartGame
    }

    /// <summary>
    /// This is the main entry class of game. Handles all game screens,
    /// which in turn handle all game logic.
    /// </summary>
    public class HeavyGearManager : BaseGame
    {
        #region Variables
        /// <summary>
        /// Game screens stack. We can easily add and remove game screens
        /// and they follow the game logic automatically. Very cool.
        /// </summary>
        private static Stack<IGameScreen> gameScreens = new Stack<IGameScreen>();

        //private static bool gameOver;
        private static Camera2D camera = new Camera2D();

        /// <summary>
        /// displays events to the user
        /// </summary>
        private static string[] messageLog = new string[]
            { 
                "",
                "",
                "",
                ""
            };

        /// <summary>
        /// Players for the game, stores unit lists and such
        /// </summary>
        //private static Player[] players; //= new Player[4]; //new Player[]{new Player(PlayerIndex.One), new Player(PlayerIndex.Two)};

        public static bool[] PlayerReady = new bool[4];
        private static Player activePlayer;
        private static Player localPlayer;
        private static Player[] players;

        private static int playerWon;
        private static bool gameOver;
        public static bool NextPlayer = false;

        private static bool deployMode = true;
        private static bool startGame = false;

        /// <summary>
        /// List of available maps
        /// </summary>
        private static Map[] maps;
        private static string[] mapFileNames;
        /// <summary>
        /// Current map we are playing on
        /// </summary>
        private static Map map;
        private static int currentTurn = 0;

        // Transport abstraction -- replaced by LiteNetLib implementation in Phase 6.
        private static ITransport transport = NullTransport.Instance;

        // Reusable packet buffers
        private static MemoryStream packetStream = new MemoryStream(1024);
        private static BinaryWriter packetWriter;
        private static BinaryReader packetReader;

        // Stored so packet handlers can reference current frame time
        private static GameTime lastGameTime = new GameTime();

        private static string errorMessage;

        // How often should we send network packets?
        private const int framesBetweenPackets = 6;

        // How recently did we send the last network packet?
        private static int framesSinceLastSend;
        
        #endregion

        #region Properties
        public static Player LocalPlayer
        {
            get
            {
                return localPlayer;
            }
        }
        public static bool GameOver
        {
            get
            {
                return gameOver;
            }
        }
        public static int PlayerWon
        {
            get
            {
                return playerWon;
            }
        }
        public static bool DeployMode
        {
            get
            {
                return deployMode;
            }
        }
        public static int CurrentTurn
        {
            get
            {
                return currentTurn;
            }
        }
        public static string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }
        public static ITransport Transport => transport;
        public static int NumberOfPlayers
        {
            get
            {
                if (players != null) return players.Length;
                return transport.PlayerCount;
            }
        }
        public static string[] MessageLog
        {
            get
            {
                return messageLog;
            }
        }
        public static Camera2D Camera
        {
            get
            {
                return camera;
            }
        }

        public static Player ActivePlayer
        {
            get
            {
                return activePlayer;
            }
        }
        public static bool StartGame
        {
            get
            {
                return startGame;
            }
        }
        
        /// <summary>
        /// In menu
        /// </summary>
        /// <returns>Bool</returns>
        public static bool InMenu
        {
            get
            {
                return gameScreens.Count > 0 &&
                    (gameScreens.Peek().GetType() != typeof(GameScreen));
            }
        }

        /// <summary>
        /// In game?
        /// </summary>
        public static bool InGame
        {
            get
            {
                return gameScreens.Count > 0 &&
                    (gameScreens.Peek().GetType() == typeof(GameScreen));
            }
        }

        /// <summary>
        /// ShowMouseCursor
        /// </summary>
        /// <returns>Bool</returns>
        public static bool ShowMouseCursor
        {
            get
            {
                // Only if not in Game, not in splash screen!
                return gameScreens.Count > 0 &&
                    //gameScreens.Peek().GetType() != typeof(GameScreen) &&
                    gameScreens.Peek().GetType() != typeof(SplashScreen);
            }
        }

        /// <summary>
        /// Players for the game
        /// </summary>
        /// <returns>Player</returns>
        public static Player Player(int index) => players[index];

        /// <summary>
        /// Gets the current Map
        /// </summary>
        /// <returns>Map</returns>
        public static Map Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
            }
        }

        public static Map[] Maps
        {
            get
            {
                return maps;
            }
        }
        public static string[] MapFileNames
        {
            get
            {
                return mapFileNames;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create game
        /// </summary>
        public HeavyGearManager()
            : base("HeavyGear")
        {
            packetWriter = new BinaryWriter(packetStream);
            packetReader = new BinaryReader(packetStream);
            //TODO: Start playing the menu music

            // Create main menu at our main entry point
            gameScreens.Push(new MainMenu());

            //But start with splash screen
            gameScreens.Push(new SplashScreen());

            //Init any static screens here

            //Init map list
            mapFileNames = Directory.GetFiles(Path.Combine(Directories.ContentDirectory, "Maps"), "*.xml");
            maps = new Map[mapFileNames.Length];

            for (int i = 0; i < mapFileNames.Length; i++)
                maps[i] = new Map(mapFileNames[i]);
        }

        /// <summary>
        /// Create game for unit tests, not used for anything else.
        /// </summary>
        public HeavyGearManager(string unitTestName)
            : base(unitTestName)
        {
            // Don't add game screens here
        }

        /// <summary>
        /// Load stuff
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

        }
        #endregion

        #region Camera
        /// <summary>
        /// Reset the camera to the center of the tile grid
        /// and reset the position of the animted sprite
        /// </summary>
        private void ResetToInitialPositions()
        {
            //set the initial position to the center of the
            //tile field
            camera.Position = new Vector2(Map.Width/2, Map.Height/2);

            CameraChanged();
        }

        /// <summary>
        /// This function is called when the camera's values have changed
        /// and is used to update the properties of the tiles and animated sprite
        /// </summary>
        public void CameraChanged()
        {
            map.VisibilityChanged = true;
            //changes have been accounted for, reset the changed value so that this
            //function is not called unnecessarily
            camera.ResetChanged();
        }
        #endregion

        #region Add game screen
        /// <summary>
        /// Add game screen
        /// </summary>
        /// <param name="gameScreen">Game screen</param>
        public static void AddGameScreen(IGameScreen gameScreen)
        {
            // Play sound for screen click
            Sound.Play(Sound.Sounds.ScreenClick);

            // Add the game screen
            gameScreens.Push(gameScreen);
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);

            lastGameTime = gameTime;
            if (transport.IsConnected)
            {
                UpdateNetworkSession(gameTime);
            }

            if (InGame)
            {
                if (!transport.IsConnected)
                {
                    foreach (Player player in players)
                        player.UpdateLocal();
                }

                if (camera.IsChanged)
                    CameraChanged();

                //check to see if one player has won the game
                int playerCount = 0;
                for (int i = 0; i < NumberOfPlayers; i++)
                {
                    Player player = Player(i);
                    foreach (Unit unit in player.Units)
                    {
                        if (unit.IsAlive || unit.InAnimation)
                        {
                            playerCount++;
                            playerWon = player.PlayerIndex;
                            break;
                        }
                    }
                }

                if (playerCount < 2)
                    gameOver = true;
            }
        }
        #endregion

        #region Network Methods

        private static void UpdateNetworkSession(GameTime gameTime)
        {
            transport.Update();
        }

        public static void SendMenuPacket(int playerIndex)
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Menu);
            string mapName = map?.Name ?? "";
            packetWriter.Write(mapName);
            packetWriter.Write(playerIndex);
            transport.SendToAll(packetStream.ToArray());
        }

        private static void SendStartTurnPacket(int playerIndex)
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.StartTurn);
            packetWriter.Write(playerIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendStartGamePacket()
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.StartGame);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendAttackPacket(int attackingUnitIndex)
        {
            Player player = localPlayer;
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Attack);
            packetWriter.Write(messageLog[0]);
            packetWriter.Write(messageLog[1]);
            packetWriter.Write(messageLog[2]);
            packetWriter.Write(messageLog[3]);
            player.WriteAttackPacket(packetWriter, attackingUnitIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendTargetPacket(int targetedUnitIndex)
        {
            Player player = localPlayer;
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Target);
            packetWriter.Write(messageLog[0]);
            packetWriter.Write(messageLog[1]);
            packetWriter.Write(messageLog[2]);
            packetWriter.Write(messageLog[3]);
            player.WriteTargetPacket(packetWriter, targetedUnitIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        private static void OnPacketReceived(BinaryReader reader, int senderIndex)
        {
            PacketType packetType = (PacketType)reader.ReadByte();
            Player sender = players != null && senderIndex < players.Length ? players[senderIndex] : null;

            switch (packetType)
            {
                case PacketType.Menu:
                    string mapName = reader.ReadString();
                    int playerIndex = reader.ReadInt32();
                    if (playerIndex >= 0) PlayerReady[playerIndex] = !PlayerReady[playerIndex];
                    if (!string.IsNullOrEmpty(mapName))
                    {
                        for (int i = 0; i < maps.Length; i++)
                        {
                            if (maps[i].Name == mapName) { map = new Map(mapFileNames[i]); break; }
                        }
                    }
                    break;
                case PacketType.StartTurn:
                    int pi = reader.ReadInt32();
                    if (deployMode) deployMode = false;
                    activePlayer = players[pi];
                    if (localPlayer?.PlayerIndex == pi)
                    {
                        foreach (Unit u in activePlayer.Units) if (u.IsAlive) u.StartTurn();
                        NextPlayer = true;
                    }
                    break;
                case PacketType.StartGame:
                    startGame = true;
                    break;
                case PacketType.Normal:
                    sender?.ReadNormalPacket(reader, lastGameTime, TimeSpan.Zero);
                    break;
                case PacketType.Deploy:
                    sender?.ReadDeployPacket(reader, lastGameTime, TimeSpan.Zero);
                    break;
                case PacketType.Attack:
                    messageLog[0] = reader.ReadString();
                    messageLog[1] = reader.ReadString();
                    messageLog[2] = reader.ReadString();
                    messageLog[3] = reader.ReadString();
                    int attackIdx = reader.ReadByte();
                    UnitType tType = (UnitType)reader.ReadByte();
                    int tPlayer = reader.ReadByte();
                    int tUnit = reader.ReadByte();
                    int infDmg = 0;
                    DamageType dmg = DamageType.None;
                    if (tType == UnitType.Infantry) infDmg = reader.ReadByte();
                    else dmg = (DamageType)reader.ReadByte();
                    WeaponType wpn = (WeaponType)reader.ReadByte();
                    if (sender != null)
                    {
                        sender.Units[attackIdx].TargetedUnit = Player(tPlayer).Units[tUnit];
                        sender.Units[attackIdx].AddProjectiles(wpn, 0);
                    }
                    if (localPlayer?.PlayerIndex == tPlayer)
                    {
                        if (tType == UnitType.Infantry) Player(tPlayer).ApplyInfantryDamage(tUnit, infDmg);
                        else Player(tPlayer).ApplyDamage(tUnit, dmg);
                    }
                    else { Player(tPlayer).Units[tUnit].StartAnimation(AnimationType.Hit); }
                    break;
                case PacketType.Target:
                    messageLog[0] = reader.ReadString();
                    messageLog[1] = reader.ReadString();
                    messageLog[2] = reader.ReadString();
                    messageLog[3] = reader.ReadString();
                    int targetIdx = reader.ReadByte();
                    bool destroyed = reader.ReadBoolean();
                    if (destroyed && sender != null) sender.Units[targetIdx].Destroy();
                    break;
            }
        }

        public static void JoinSession(ITransport newTransport)
        {
            transport = newTransport;
            transport.PacketReceived += OnPacketReceived;
            transport.SessionEnded += () => { transport = NullTransport.Instance; };
        }

        public static void EndSession()
        {
            transport?.Dispose();
            transport = NullTransport.Instance;
        }

        public static void CreateSession(ITransport newTransport)
        {
            try { JoinSession(newTransport); }
            catch (Exception e) { errorMessage = e.Message; }
        }

        public static void HookSessionEvents() { /* handled by ITransport events */ }

        public static int GetBits(byte b, int offset, int count)
        {
            return (b >> offset) & ((1 << count) - 1);
        }
        #endregion


        

        public static void StartLocal(int numOfPlayers)
        {
            players = new Player[numOfPlayers];
            for (int i = 0; i < numOfPlayers; i++)
                players[i] = new Player(i, "Player " + (i + 1));
            activePlayer = Player(0);
        }

        /// <summary>
        /// Adds a new message to the end of the message log and moves other strings back one
        /// </summary>
        /// <param name="message"></param>
        public static void MessageLogAdd(string message)
        {
            for (int i = 1; i < 4; i++)
            {
                messageLog[i - 1] = messageLog[i];
            }
            messageLog[3] = message;
        }

        #region EndTurn

        public static void EndTurn()
        {
            if (gameOver)
                return;

            if (activePlayer.PlayerIndex == NumberOfPlayers - 1)
                currentTurn++;

            activePlayer = Player((activePlayer.PlayerIndex + 1) % NumberOfPlayers);

            if (activePlayer.Units.Count == 0)
                EndTurn();

            if (activePlayer.Units.Count > 0)
                    camera.MapPosition = activePlayer.Units[0].MapPosition;

            if (!transport.IsConnected)
            {
                foreach (Unit unit in activePlayer.Units)
                {
                    if (unit.IsAlive)
                        unit.StartTurn();
                }
                NextPlayer = true;
            }
            else
            {
                SendStartTurnPacket(activePlayer.PlayerIndex);
            }
        }
        

        public static void DeployEndTurn()
        {
            activePlayer = Player((activePlayer.PlayerIndex + 1) % NumberOfPlayers);
            bool unitNeedsPlace = false;

            foreach (Unit unit in activePlayer.Units)
            {
                if (unit.MapPosition.X < 0 || unit.MapPosition.Y < 0)
                {
                    unitNeedsPlace = true;
                    break;
                }
            }

            if (!unitNeedsPlace)
            {
                activePlayer = Player(NumberOfPlayers - 1);
                //camera.MapPosition = cursorPosition = activePlayer.Army[0].MapPosition;
                deployMode = false;
                EndTurn();
            }
        }
        public static void StartNetworkGame()
        {
            activePlayer = Player(NumberOfPlayers - 1);
            EndTurn();
        }

        public static void InitDeploy()
        {
            activePlayer = localPlayer;
        }
        

        #endregion

        #region Exit/Reset
        public static void ExitGame()
        {
            gameScreens.Clear();
        }

        public static void ReturnToMenu()
        {
            gameOver = false;
            activePlayer = null;
            playerWon = 0;
            NextPlayer = false;
            deployMode = true;
            startGame = false;
            messageLog = new string[] { "", "", "", "" };
            map = null;
            currentTurn = 0;
            players = null;
            transport?.Dispose();
            transport = NullTransport.Instance;

            gameScreens.Clear();
            gameScreens.Push(new MainMenu());
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        protected override void Render()
        {
            // No more game screens?
            if (gameScreens.Count == 0)
            {
                // Before quiting, stop music
                //Sound.StopMusic();

                // Then quit
                Exit();
                return;
            }

            // Handle current screen
            if (gameScreens.Peek().Render())
            {
                if (gameScreens.Count == 0)
                {
                    // Before quiting, stop music
                    //Sound.StopMusic();

                    // Then quit
                    Exit();
                    return;
                }
                
                // If this was the options screen and the resolution has changed,
                // apply the changes
                //if (gameScreens.Peek().GetType() == typeof(Options) &&
                //    (BaseGame.Width != GameSettings.Default.ResolutionWidth ||
               //     BaseGame.Height != GameSettings.Default.ResolutionHeight ||
               //     BaseGame.Fullscreen != GameSettings.Default.Fullscreen))
              //  {
             //       BaseGame.ApplyResolutionChange();
             //   }
            
                // Play sound for screen back
                Sound.Play(Sound.Sounds.ScreenBack);

                gameScreens.Pop();
            }
        }
        

        /// <summary>
        /// Post user interface rendering, in case we need it.
        /// Used for rendering the car selection 3d stuff after the UI.
        /// </summary>
        protected override void PostUIRender()
        {
            // Enable depth buffer again
            BaseGame.Device.DepthStencilState = Microsoft.Xna.Framework.Graphics.DepthStencilState.Default;

            // Do menu shader after everything
            //if (HeavyGearManager.InMenu) &&
                //PostScreenMenu.Started)
                //UI.PostScreenMenuShader.Show();
        }
        #endregion
    }
}
