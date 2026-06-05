#region File Description
//-----------------------------------------------------------------------------
// EditorScreen.cs
//
// In-game hex map editor (MonoGame port of the WinForms HeavyGearMapEditor).
// Accessed from MainMenu. Allows editing tile types, elevation, and player
// start positions on any loaded map, then saving to XML.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Xml;
using HeavyGear.GameLogic;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using Texture = HeavyGear.Graphics.Texture;
#endregion

namespace HeavyGear.GameScreens
{
    /// <summary>
    /// Tool the user can select while in the editor.
    /// </summary>
    enum EditorTool
    {
        HexType,        // Left-click cycles through HexType values
        Elevation,      // Left-click +1 elevation, Right-click -1
        Player1Start,
        Player2Start,
        Player3Start,
        Player4Start,
    }

    /// <summary>
    /// Full-screen in-game map editor.
    /// Controls:
    ///   WASD / Arrow keys  — scroll camera
    ///   Tab                — cycle active tool
    ///   Left click         — apply tool to hovered hex
    ///   Right click        — decrement (for Elevation tool)
    ///   S                  — save map to HeavyGear/Maps/ in AppData
    ///   Escape             — exit editor
    /// </summary>
    class EditorScreen : IGameScreen
    {
        #region Constants
        private static readonly string[] ToolNames =
        {
            "Tool: Hex Type",
            "Tool: Elevation +/-",
            "Tool: Player 1 Start",
            "Tool: Player 2 Start",
            "Tool: Player 3 Start",
            "Tool: Player 4 Start",
        };

        private const float CameraSpeed = 300f; // pixels per second
        #endregion

        #region Variables
        private EditorTool activeTool = EditorTool.HexType;
        private HexTile hoveredTile;
        private bool toolCooldown; // prevent rapid tool cycling on single key press
        private string statusMessage = string.Empty;
        private float statusMessageTimer;
        #endregion

        #region Constructor
        public EditorScreen()
        {
            // If no map is loaded, load the first available map.
            if (HeavyGearManager.Map == null && HeavyGearManager.Maps != null && HeavyGearManager.Maps.Length > 0)
            {
                HeavyGearManager.Map = HeavyGearManager.Maps[0];
                HeavyGearManager.Map.LoadBackground();
                HeavyGearManager.Map.ResetScreenPositions();
            }
        }
        #endregion

        #region Render
        public bool Render()
        {
            if (HeavyGearManager.Map == null)
            {
                TextureFont.WriteTextCentered(BaseGame.Width / 2, BaseGame.Height / 2, "No map loaded.");
                return Input.KeyboardEscapeJustPressed;
            }

            float dt = HeavyGearManager.ElapsedTimeThisFrameInMilliseconds / 1000f;

            // ── Camera panning ─────────────────────────────────────────────
            float move = CameraSpeed * dt;
            if (Input.KeyboardLeftPressed  || Input.Keyboard.IsKeyDown(Keys.A)) HeavyGearManager.Camera.MoveLeft(move);
            if (Input.KeyboardRightPressed || Input.Keyboard.IsKeyDown(Keys.D)) HeavyGearManager.Camera.MoveRight(move);
            if (Input.KeyboardUpPressed    || Input.Keyboard.IsKeyDown(Keys.W)) HeavyGearManager.Camera.MoveUp(move);
            if (Input.KeyboardDownPressed  || Input.Keyboard.IsKeyDown(Keys.S) && !Input.Keyboard.IsKeyDown(Keys.LeftControl))
                HeavyGearManager.Camera.MoveDown(move);

            // ── Tool cycling (Tab) ─────────────────────────────────────────
            if (Input.Keyboard.IsKeyDown(Keys.Tab))
            {
                if (!toolCooldown)
                {
                    activeTool = (EditorTool)(((int)activeTool + 1) % Enum.GetValues(typeof(EditorTool)).Length);
                    toolCooldown = true;
                }
            }
            else
            {
                toolCooldown = false;
            }

            // ── Find hovered hex ───────────────────────────────────────────
            hoveredTile = GetHexUnderMouse();

            // ── Apply tool on click ────────────────────────────────────────
            if (hoveredTile != null)
            {
                if (Input.MouseLeftButtonJustPressed)
                    ApplyTool(hoveredTile, increment: true);
                else if (Input.MouseRightButtonJustPressed)
                    ApplyTool(hoveredTile, increment: false);
            }

            // ── Save (Ctrl+S) ──────────────────────────────────────────────
            if (Input.Keyboard.IsKeyDown(Keys.LeftControl) && Input.Keyboard.IsKeyDown(Keys.S))
            {
                if (!toolCooldown)
                {
                    SaveMap();
                    toolCooldown = true;
                }
            }

            // ── Status message timer ───────────────────────────────────────
            if (statusMessageTimer > 0)
                statusMessageTimer -= HeavyGearManager.ElapsedTimeThisFrameInMilliseconds;

            // ── Draw map ───────────────────────────────────────────────────
            // Flush sprite batches the game opened, then restart for editor
            Texture.additiveSprite.End();
            Texture.alphaSprite.End();

            Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            HeavyGearManager.Map.Draw(editorMode: true);

            // Highlight hovered tile
            if (hoveredTile != null)
            {
                Vector2 sp = hoveredTile.ScreenPosition;
                Vector2 origin = HeavyGearManager.Map.Origin;
                Rectangle rect = new Rectangle(
                    (int)(sp.X - origin.X),
                    (int)(sp.Y - origin.Y),
                    (int)BaseGame.TileWidth,
                    (int)BaseGame.TileHeight);
                BaseGame.UI.Hex.RenderOnScreen(rect, UIRenderer.HexTileHighlightGfxRect, Color.Yellow * 0.5f);
            }

            // ── HUD overlay ────────────────────────────────────────────────
            DrawHUD();

            return Input.KeyboardEscapeJustPressed;
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Returns the HexTile currently under the mouse cursor, or null if none.
        /// </summary>
        private HexTile GetHexUnderMouse()
        {
            Vector2 mouseWorld = new Vector2(
                Input.MousePos.X + HeavyGearManager.Map.Origin.X,
                Input.MousePos.Y + HeavyGearManager.Map.Origin.Y);

            BaseGame.ConvertPixelToMap(ref mouseWorld, out Point mapPos);

            int x = mapPos.X;
            int y = mapPos.Y;
            if (x >= 0 && x < HeavyGearManager.Map.Width &&
                y >= 0 && y < HeavyGearManager.Map.Height)
                return HeavyGearManager.Map.GetTile(x, y);

            return null;
        }

        /// <summary>Apply the active tool to the given tile.</summary>
        private void ApplyTool(HexTile tile, bool increment)
        {
            switch (activeTool)
            {
                case EditorTool.HexType:
                    int count = Enum.GetValues(typeof(HexType)).Length;
                    int cur = (int)tile.HexType;
                    tile.HexType = (HexType)((cur + (increment ? 1 : count - 1)) % count);
                    break;

                case EditorTool.Elevation:
                    tile.Elevation = Math.Clamp(tile.Elevation + (increment ? 1 : -1), 0, 9);
                    break;

                case EditorTool.Player1Start:
                    HeavyGearManager.Map.Player1Start = tile.MapPosition;
                    break;
                case EditorTool.Player2Start:
                    HeavyGearManager.Map.Player2Start = tile.MapPosition;
                    break;
                case EditorTool.Player3Start:
                    HeavyGearManager.Map.Player3Start = tile.MapPosition;
                    break;
                case EditorTool.Player4Start:
                    HeavyGearManager.Map.Player4Start = tile.MapPosition;
                    break;
            }
        }

        /// <summary>Save the map XML to the user's AppData HeavyGear/Maps folder.</summary>
        private void SaveMap()
        {
            try
            {
                string mapsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HeavyGear", "Maps");
                Directory.CreateDirectory(mapsDir);

                string mapName = string.IsNullOrWhiteSpace(HeavyGearManager.Map.Name)
                    ? "NewMap"
                    : HeavyGearManager.Map.Name;

                string path = Path.Combine(mapsDir, mapName + ".xml");
                XmlDocument doc = HeavyGearManager.Map.Save();
                doc.Save(path);

                ShowStatus("Saved: " + path);
            }
            catch (Exception ex)
            {
                ShowStatus("Save failed: " + ex.Message);
                Log.Write("EditorScreen.SaveMap: " + ex);
            }
        }

        private void ShowStatus(string msg)
        {
            statusMessage = msg;
            statusMessageTimer = 3000f; // show for 3 seconds
        }

        /// <summary>Draw the editor HUD in screen-space.</summary>
        private void DrawHUD()
        {
            int x = BaseGame.XToRes(10);
            int y = BaseGame.YToRes(10);
            int lineH = BaseGame.YToRes(20);

            // Tool name
            TextureFont.WriteText(x, y, ToolNames[(int)activeTool]);
            y += lineH;

            // Hovered tile info
            if (hoveredTile != null)
            {
                TextureFont.WriteText(x, y,
                    $"Hex ({hoveredTile.MapPosition.X},{hoveredTile.MapPosition.Y})  " +
                    $"Type: {hoveredTile.HexType}  Elev: {hoveredTile.Elevation}");
                y += lineH;
            }

            // Player starts
            TextureFont.WriteText(x, y,
                $"P1({HeavyGearManager.Map.Player1Start.X},{HeavyGearManager.Map.Player1Start.Y}) " +
                $"P2({HeavyGearManager.Map.Player2Start.X},{HeavyGearManager.Map.Player2Start.Y})");
            y += lineH;

            // Controls hint
            TextureFont.WriteText(x, y, "Tab=Tool  LClick=Apply  RClick=Dec  Ctrl+S=Save  Esc=Exit");
            y += lineH;

            // Status message
            if (statusMessageTimer > 0)
                TextureFont.WriteText(x, y, statusMessage);
        }
        #endregion
    }
}
