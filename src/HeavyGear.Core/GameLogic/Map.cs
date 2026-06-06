#region File Description
//-----------------------------------------------------------------------------
// Map.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HeavyGear.Sounds;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using Texture = HeavyGear.Graphics.Texture;
#endregion

namespace HeavyGear.GameLogic
{
    public enum MissionType
    {
        CaptureAndHold,
        Assault,
        Skirmish
    }
    public enum ObjectiveType
    {
        Location,
        Unit
    }
    /// <summary>
    /// Represents a hex grid where units will move
    /// </summary>
    public class Map
    {
        #region Constants
        private static Point[] HexNeighboursOdd = new Point[]{ new Point(0,-2), new Point(1,-1), new Point(1, 1), 
                                       new Point(0, 2), new Point(0, 1), new Point(0,-1) };
        private static Point[] HexNeighboursEven = new Point[]{ new Point(0,-2), new Point(0,-1), new Point(0, 1), 
                                       new Point(0, 2), new Point(-1, 1), new Point(-1,-1) };

        public static int LEFT = -1;
        public static int RIGHT = 1;
        public static int STRAIGHT = 0;

        public static float TextureTileWidth = 74;
        public static float TextureTileHeight = 64;
        public static float TextureTileSide = 37;

        public static Vector2 TextureScale = new Vector2(111, 32);
        public static float TextureOffset = 111 / 2.0f;
        #endregion

        #region Variables
        private HexTile[][] grid;
        private int width;
        private int height;

        private string name;
        private string description;

        private Point player1Start;
        private Point player2Start;
        private Point player3Start;
        private Point player4Start;

        private int threatValue;

        //drawing parameters
        private Vector2 origin;
        private Rectangle viewRect;
        private Rectangle renderRect;
        private Rectangle mapRect;
        private Rectangle visibleTiles;
        private Rectangle visibleBackground;
        private Vector2 scaleValue = Vector2.One;
        private Point textureScale = Point.Zero;
        private float zoomValue = 1.0f;

        private bool visibilityChanged = true;

        private bool showHexType = false;
        private bool showElevation = false;
        private bool showPosition = false;
        private bool showPlayerStarts = false;
        private bool showHexes = true;

        private Texture background;
        private string backgroundFilename;

        //private Arc arcList;
        #endregion

        #region Properties
        public bool ShowHexType
        {
            get
            {
                return showHexType;
            }
            set
            {
                showHexType = value;
            }
        }
        public bool ShowHexes
        {
            get
            {
                return showHexes;
            }
            set
            {
                showHexes = value;
            }
        }
        public bool ShowElevation
        {
            get
            {
                return showElevation;
            }
            set
            {
                showElevation = value;
            }
        }

        public bool ShowPosition
        {
            get
            {
                return showPosition;
            }
            set
            {
                showPosition = value;
            }
        }
        public bool ShowPlayerStarts
        {
            get
            {
                return showPlayerStarts;
            }
            set
            {
                showPlayerStarts = value;
            }
        }
        public Texture Background
        {
            get
            {
                return background;
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
        }
        public bool VisibilityChanged
        {
            get
            {
                return visibilityChanged;
            }
            set
            {
                visibilityChanged = value;
            }
        }

        public int ThreatValue
        {
            get
            {
                return threatValue;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public Point Player1Start
        {
            get
            {
                return player1Start;
            }
            set
            {
                player1Start = value;
            }
        }

        public Point Player2Start
        {
            get
            {
                return player2Start;
            }
            set
            {
                player2Start = value;
            }
        }

        public Point Player3Start
        {
            get
            {
                return player3Start;
            }
            set
            {
                player3Start = value;
            }
        }

        public Point Player4Start
        {
            get
            {
                return player4Start;
            }
            set
            {
                player4Start = value;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return origin;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public float ZoomValue
        {
            get
            {
                return zoomValue;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for a hex grid
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="gridFileName">The name of the xml file containing the grid of HexTiles to be used</param>
        public Map(string gridFileName)
        {
            if (!string.IsNullOrEmpty(gridFileName))
            {
                //Loads HexGrid from file here
                string filename = Path.GetFileNameWithoutExtension(gridFileName);
                string fullFilename = Path.Combine(Directories.ContentDirectory, "Maps", filename + ".xml");
                backgroundFilename = "Maps/" + filename; // content-relative for Content.Load

                //now calls LoadBackground() to load the map texture on game screen init using backgroungFileName
                //background = new HeavyGear.Graphics.Texture(BaseGame.Content.Load<Texture2D>(backgroundFilename));

                XmlDocument doc = new XmlDocument();
                doc.Load(fullFilename);

                Load(doc);
            }

            visibleTiles = new Rectangle(0, 0, width, height);
        }


        /// <summary>
        /// Used for map editor, creates a new grid based on standard hex size
        /// </summary>
        /// <param name="texture"></param>
        public Map(Texture texture)
        {
            width = texture.Width / 222;
            height = texture.Height / 64 - 1;

            grid = new HexTile[width][];
            for (int i = 0; i < width; i++)
            {
                grid[i] = new HexTile[height];
                for (int j = 0; j < height; j++)
                {
                    grid[i][j] = new HexTile(new Point(i, j));
                }
            }
        }
        #endregion

        #region LoadBackground
        public void LoadBackground()
        {
            background = new HeavyGear.Graphics.Texture(BaseGame.Content.Load<Texture2D>(backgroundFilename));
        }
        #endregion

        #region Load
        public void Load(XmlDocument doc)
        {
            XmlNodeList nameNode = doc.GetElementsByTagName("Name");
            name = nameNode[0].InnerXml;

            XmlNodeList descriptionNode = doc.GetElementsByTagName("Description");
            description = descriptionNode[0].InnerXml;

            XmlNodeList threatValueNode = doc.GetElementsByTagName("ThreatValue");
            threatValue = Convert.ToInt32(threatValueNode[0].InnerXml);

            XmlNodeList rows = doc.GetElementsByTagName("Row");
            XmlNodeList player1StartNode = doc.GetElementsByTagName("Player1Start");
            XmlNodeList player2StartNode = doc.GetElementsByTagName("Player2Start");
            XmlNodeList player3StartNode = doc.GetElementsByTagName("Player3Start");
            XmlNodeList player4StartNode = doc.GetElementsByTagName("Player4Start");

            player1Start.X = Convert.ToInt32(player1StartNode[0].ChildNodes[0].InnerXml);
            player1Start.Y = Convert.ToInt32(player1StartNode[0].ChildNodes[1].InnerXml);

            player2Start.X = Convert.ToInt32(player2StartNode[0].ChildNodes[0].InnerXml);
            player2Start.Y = Convert.ToInt32(player2StartNode[0].ChildNodes[1].InnerXml);

            player3Start.X = Convert.ToInt32(player3StartNode[0].ChildNodes[0].InnerXml);
            player3Start.Y = Convert.ToInt32(player3StartNode[0].ChildNodes[1].InnerXml);

            player4Start.X = Convert.ToInt32(player4StartNode[0].ChildNodes[0].InnerXml);
            player4Start.Y = Convert.ToInt32(player4StartNode[0].ChildNodes[1].InnerXml);

            width = rows.Count;
            height = rows[0].ChildNodes.Count;

            grid = new HexTile[width][];
            for (int i = 0; i < width; i++)
            {
                grid[i] = new HexTile[height];
                XmlNode row = rows[i];
                for (int j = 0; j < height; j++)
                {
                    XmlNode tileNode = row.ChildNodes[j];
                    grid[i][j] = new HexTile(new Point(i, j),
                                            Convert.ToInt32(tileNode.ChildNodes[0].InnerXml),
                                            (HexType)Convert.ToInt32(tileNode.ChildNodes[1].InnerXml));
                }
            }

            
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the map as an xml file, used with editor
        /// </summary>
        /// <param name="fileName"></param>
        public XmlDocument Save()
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument xmlDoc = new XmlDocument();
            sb.Append("<Grid>");

            sb.Append("\t<Name>" + name + "</Name>");

            sb.Append("\t<Description>" + description + "</Description>");

            sb.Append("\t<ThreatValue>" + threatValue + "</ThreatValue>");

            for (int i = 0; i < width; i++)
            {
                sb.Append("\t<Row>");
                for (int j = 0; j < height; j++)
                {
                    HexTile tile = GetTile(i, j);
                    sb.Append("\t\t<HexTile>");
                    sb.Append("\t\t\t<Elevation>" + tile.Elevation + "</Elevation>");
                    sb.Append("\t\t\t<HexType>" + (int)tile.HexType + "</HexType>");
                    sb.Append("\t\t</HexTile>");
                }
                sb.Append("\t</Row>");
            }

            sb.Append("\t<Player1Start>");
            sb.Append("\t\t<X>" + (int)player1Start.X + "</X>");
            sb.Append("\t\t<Y>" + (int)player1Start.Y + "</Y>");
            sb.Append("\t</Player1Start>");

            sb.Append("\t<Player2Start>");
            sb.Append("\t\t<X>" + (int)player2Start.X + "</X>");
            sb.Append("\t\t<Y>" + (int)player2Start.Y + "</Y>");
            sb.Append("\t</Player2Start>");

            sb.Append("\t<Player3Start>");
            sb.Append("\t\t<X>" + (int)player3Start.X + "</X>");
            sb.Append("\t\t<Y>" + (int)player3Start.Y + "</Y>");
            sb.Append("\t</Player3Start>");

            sb.Append("\t<Player4Start>");
            sb.Append("\t\t<X>" + (int)player4Start.X + "</X>");
            sb.Append("\t\t<Y>" + (int)player4Start.Y + "</Y>");
            sb.Append("\t</Player4Start>");

            sb.Append("</Grid>");

            xmlDoc.LoadXml(sb.ToString());

            return xmlDoc;
        }
        #endregion

        #region Methods
        public void SetTile(int x, int y, HexTile tile)
        {
            if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
                throw new ArgumentException("Invalid x, y");
            grid[x][y] = tile;
        }
        public HexTile GetTile(int x, int y)
        {
            if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
                return null;
            else
                return grid[x][y];
        }
        public HexTile GetTile(Point point)
        {
            return GetTile(point.X, point.Y);
        }
        #endregion

        #region DetermineVisibility
        /// <summary>
        /// This function determines which tiles are visible on the screen,
        /// given the current camera position, rotation, zoom, and tile scale
        /// </summary>
        private void DetermineVisibility()
        {
            Vector2 cameraPositionValue = HeavyGearManager.Camera.Position;

            origin.X = cameraPositionValue.X - BaseGame.Width / 2;
            origin.Y = cameraPositionValue.Y - BaseGame.Height / 2;

            HeavyGearManager.Camera.Position = cameraPositionValue;

            //represents the area in view in terms of map pixel coordinates
            viewRect = new Rectangle((int)origin.X, (int)origin.Y, BaseGame.Width, BaseGame.Height);

            //the area of the screen to render the map texture to
            renderRect = new Rectangle(0, 0, BaseGame.Width, BaseGame.Height);

            float TileScaleX = BaseGame.TileScale.X / zoomValue;
            float TileScaleY = BaseGame.TileScale.Y / zoomValue;
            float TileSide = BaseGame.TileSide / zoomValue;

            if (viewRect.X < 0) renderRect.X = (int)Math.Abs(origin.X);
            if (viewRect.Y < 0) renderRect.Y = (int)Math.Abs(origin.Y);
            if (viewRect.Right > width * TileScaleX + TileSide)
                renderRect.Width -= (int)(viewRect.Right + renderRect.X - (width * TileScaleX + TileSide));
            if (viewRect.Bottom > height * TileScaleY + TileScaleY)
                renderRect.Height -= (int)(viewRect.Bottom + renderRect.Y - (height * TileScaleY + TileScaleY));

            //create the view rectangle
            Vector2 upperLeft = Vector2.Zero;
            Vector2 upperRight = Vector2.Zero;
            Vector2 lowerLeft = Vector2.Zero;
            Vector2 lowerRight = Vector2.Zero;
            lowerRight.X = (BaseGame.Width / 2); /// zoomValue);
            lowerRight.Y = (BaseGame.Height / 2);// / zoomValue);
            upperRight.X = lowerRight.X;
            upperRight.Y = -lowerRight.Y;
            lowerLeft.X = -lowerRight.X;
            lowerLeft.Y = lowerRight.Y;
            upperLeft.X = -lowerRight.X;
            upperLeft.Y = -lowerRight.Y;

            //rotate the view rectangle appropriately
            //Vector2.Transform(ref upperLeft, ref rotationMatrix, out upperLeft);
            //Vector2.Transform(ref lowerRight, ref rotationMatrix, out lowerRight);
            //Vector2.Transform(ref upperRight, ref rotationMatrix, out upperRight);
            //Vector2.Transform(ref lowerLeft, ref rotationMatrix, out lowerLeft);

            lowerLeft += (cameraPositionValue);
            lowerRight += (cameraPositionValue);
            upperRight += (cameraPositionValue);
            upperLeft += (cameraPositionValue);

            //the idea here is to figure out the smallest square
            //(in tile space) that contains tiles
            //the offset is calculated before scaling
            float top = MathHelper.Min(
                MathHelper.Min(upperLeft.Y, lowerRight.Y),
                MathHelper.Min(upperRight.Y, lowerLeft.Y));

            float bottom = MathHelper.Max(
                MathHelper.Max(upperLeft.Y, lowerRight.Y),
                MathHelper.Max(upperRight.Y, lowerLeft.Y));
            float right = MathHelper.Max(
                MathHelper.Max(upperLeft.X, lowerRight.X),
                MathHelper.Max(upperRight.X, lowerLeft.X));
            float left = MathHelper.Min(
                MathHelper.Min(upperLeft.X, lowerRight.X),
                MathHelper.Min(upperRight.X, lowerLeft.X));


            //now figure out where we are in the tile sheet
            float scaledTileWidth = BaseGame.TileScale.X / zoomValue;
            float scaledTileHeight = BaseGame.TileScale.Y / zoomValue;

            //get the visible tiles
            visibleTiles.X = (int)(left / (scaledTileWidth)) - 1;
            visibleTiles.Y = (int)(top / (scaledTileHeight)) - 1;

            //get the number of visible tiles
            visibleTiles.Height =
                (int)((bottom) / (scaledTileHeight)) - visibleTiles.Y + 1;
            visibleTiles.Width =
                (int)((right) / (scaledTileWidth)) - visibleTiles.X + 1;

            //clamp the "upper left" values to 0
            if (visibleTiles.X < 0) visibleTiles.X = 0;
            if (visibleTiles.X > (width - 1)) visibleTiles.X = width;
            if (visibleTiles.Y < 0) visibleTiles.Y = 0;
            if (visibleTiles.Y > (height - 1)) visibleTiles.Y = height;

            //clamp the "lower right" values to the gameboard size
            //if (visibleTiles.Right > (width - 1))
            //    visibleTiles.Width = (width - visibleTiles.X);

            //if (visibleTiles.Right < 0) visibleTiles.Width = 0;

            //if (visibleTiles.Bottom > (height - 1))
            //    visibleTiles.Height = (height - visibleTiles.Y);

            //if (visibleTiles.Bottom < 0) visibleTiles.Height = 0;

            float ratioX = TextureTileWidth / (BaseGame.TileWidth / zoomValue);
            float ratioY = TextureTileHeight / (BaseGame.TileHeight / zoomValue);

            //figure out what part of the background is visible
            int mapX = (int)(viewRect.X * ratioX);//(int)((background.Width * viewRect.X) / (width * scaledTileWidth));
            int mapY = (int)(viewRect.Y * ratioY);//(int)((background.Height * viewRect.Y) / (height * scaledTileHeight));
            int mapWidth = (int)(viewRect.Width * ratioX);//(int)((background.Width * viewRect.Width) / (width * scaledTileWidth));
            int mapHeight = (int)(viewRect.Height * ratioY);//(int)((background.Height * viewRect.Height) / (height * scaledTileHeight));

            //represents the section of the background texture corresponding to the part of the grid on screen
            visibleBackground = new Rectangle(mapX, mapY, mapWidth, mapHeight);


            //now figure out the offset if the map is not extended past the edge of the screen
            mapX = (int)(renderRect.X * ratioX);//(int)((background.Width * renderRect.X) / (width * scaledTileWidth));
            mapY = (int)(renderRect.Y * ratioY);//(int)((background.Height * renderRect.Y) / (height * scaledTileHeight));
            mapWidth = (int)(renderRect.Width * ratioX);//(int)((background.Width * renderRect.Width) / (width * scaledTileWidth));
            mapHeight = (int)(renderRect.Height * ratioY);//(int)((background.Height * renderRect.Height) / (height * scaledTileHeight));

            mapRect = new Rectangle(mapX, mapY, mapWidth, mapHeight);

            if (renderRect.Y > 0)
                visibleBackground.Y += mapRect.Y;
            if (renderRect.Height < BaseGame.Height)
                visibleBackground.Height = mapRect.Height;
            if (renderRect.X > 0)
                visibleBackground.X += mapRect.X;
            if (renderRect.Width < BaseGame.Width)
                visibleBackground.Width = mapRect.Width;

            visibilityChanged = false;
        }

        #endregion

        #region Draw
        /// <summary>
        /// Draw method gets called every time the screen renders in game mode. Make sure it's optimized
        /// </summary>
        /// <param name="editorMode">whether to allow special editor mode display options</param>
        public void Draw(bool editorMode)
        {
            if (visibilityChanged)
                DetermineVisibility();

            //The area in view at the moment
            background.RenderOnScreen(renderRect, visibleBackground);

            Rectangle tileRect = new Rectangle(0, 0, (int)(BaseGame.TileWidth / zoomValue), (int)(BaseGame.TileHeight / zoomValue + 2));

            int tilesWidth = visibleTiles.X + visibleTiles.Width;
            int tilesHeight = visibleTiles.Y + visibleTiles.Height;
            for (int x = visibleTiles.X; x < tilesWidth && x < width; x++)
            {
                for (int y = visibleTiles.Y; y < tilesHeight && y < height; y++)
                {
                    if (grid[x][y] != null)
                    {
                        //Get the tile's screen position
                        Vector2 screenPosition = grid[x][y].ScreenPosition;

                        //Get the camera position relative to the tile's position
                        Vector2.Subtract(ref screenPosition, ref origin,
                            out screenPosition);

                        //only render this tile if its within the screen limits
                        if (screenPosition.X > -BaseGame.TileScale.X && screenPosition.X < BaseGame.Width &&
                            screenPosition.Y > -BaseGame.TileHeight && screenPosition.Y < BaseGame.Height)
                        {
                            //Set render rect to the correct position
                            tileRect.X = (int)screenPosition.X;
                            tileRect.Y = (int)screenPosition.Y;

                            //Show Hexes is for map editor use
                            if (showHexes)
                            {
                                if (grid[x][y].Highlight)
                                    BaseGame.UI.Hex.RenderOnScreen(tileRect, UIRenderer.HexTileHighlightGfxRect);
                                else
                                    BaseGame.UI.Hex.RenderOnScreen(tileRect, UIRenderer.HexTileGfxRect);
                            }

                            #region Editor Mode
                            if (editorMode)
                            {
                                HexTile tile = GetTile(x, y);
                                Vector2 textPosition = new Vector2(screenPosition.X + BaseGame.TileWidth / 2,
                                                                   screenPosition.Y + BaseGame.TileScale.Y);

                                //draw the elevation, hex type and position on top of the tile if needed for debugging
                                if (showElevation)
                                    TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y - 30,
                                                      tile.Elevation.ToString());

                                if (showPosition)
                                    TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y, "(" + x + "," + y + ")");

                                if (showHexType)
                                    TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y + 30, tile.HexType.ToString());

                                if (showPlayerStarts)
                                {
                                    if (player1Start.X == x && player1Start.Y == y)
                                        TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y + 60, "Player 1 Start");
                                    else if (player2Start.X == x && player2Start.Y == y)
                                        TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y + 60, "Player 2 Start");
                                    else if (player3Start.X == x && player3Start.Y == y)
                                        TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y + 60, "Player 3 Start");
                                    else if (player4Start.X == x && player4Start.Y == y)
                                        TextureFont.WriteTextCentered((int)textPosition.X, (int)textPosition.Y + 60, "Player 4 Start");
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
        }
        #endregion

        #region GetRange
        /// <summary>
        /// Checks the range by moving tile by tile from origin to target in a line
        /// and counting the intervening Hex crossed
        /// </summary>
        /// <param name="origin">The hex map position of the origin tile</param>
        /// <param name="target">The hex map position of the target tile</param>
        /// <returns>An int containing the number of tiles between the two</returns>
        public int GetRange(Point origin, Point target)
        {
            if (origin == target)
                return 0;

            if (target.X < 0 || target.X > width - 1 ||
                target.Y < 0 || target.Y > height - 1)
                throw new ArgumentException("Invalid target point");

            int range = 0;
            Point current = origin;

            while (current.X != target.X || current.Y != target.Y)
            {
                int row = current.Y;

                #region Determine Angle
                int angle = 0;
                int dX = target.X - current.X;
                int dY = current.Y - target.Y;
                if (dX == 0)
                {
                    // if dx = 0 and dy = 1, move up and left
                    if (dY == 1)
                    {
                        if (row % 2 > 0)
                            angle = FacingInt.NorthWest;
                        else
                            angle = FacingInt.NorthEast;
                    }
                    else if (dY >= 2)
                        angle = FacingInt.North;
                    else if (dY == -1)
                    {
                        if (row % 2 > 0)
                            angle = FacingInt.SouthWest;
                        else
                            angle = FacingInt.SouthEast;
                    }
                    else
                        angle = FacingInt.South;
                }
                else if (dX > 0)
                {
                    // Right movement
                    if (dY > 0)
                        angle = FacingInt.NorthEast;
                    else if (dY == 0)
                    {
                        if (current.Y == 0)
                            angle = FacingInt.SouthEast;
                        else
                            angle = FacingInt.NorthEast;
                    }
                    else
                        angle = FacingInt.SouthEast;
                }
                else
                {
                    //Left movement
                    if (dY > 0)
                        angle = FacingInt.NorthWest;
                    else if (dY == 0)
                    {
                        if (current.Y == 0)
                            angle = FacingInt.SouthWest;
                        else
                            angle = FacingInt.NorthWest;
                    }
                    else
                        angle = FacingInt.SouthWest;
                }

                #endregion

                #region Move current tile
                //move current tile in the y direction
                switch (angle)
                {
                    case FacingInt.North:
                        current.Y -= 2;
                        break;
                    case FacingInt.NorthEast:
                    case FacingInt.NorthWest:
                        current.Y -= 1;
                        break;
                    case FacingInt.SouthEast:
                    case FacingInt.SouthWest:
                        current.Y += 1;
                        break;
                    case FacingInt.South:
                        current.Y += 2;
                        break;
                }
                // move current tile in x direction
                if (row % 2 > 0)
                {
                    //odd rows
                    switch (angle)
                    {
                        case FacingInt.NorthEast:
                        case FacingInt.SouthEast:
                            current.X += 1;
                            break;
                    }
                }
                else
                {
                    //even rows
                    switch (angle)
                    {
                        case FacingInt.SouthWest:
                        case FacingInt.NorthWest:
                            current.X -= 1;
                            break;
                    }
                }
                #endregion

                range++;
            }

            return range;
        }
        #endregion

        #region GetArea
        public List<HexTile> GetArea(Point startPoint, int range)
        {
            List<HexTile> area = new List<HexTile>();
            
            area.Add(GetTile(startPoint));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int thisRange = GetRange(startPoint, new Point(x, y));
                    if ( thisRange <= range)
                        area.Add(GetTile(x, y));
                }
            }

            return area;
        }
        #endregion

        #region GetDeploymentArea
        public List<HexTile> GetDeploymentArea(int side, int range)
        {
            List<HexTile> area = new List<HexTile>();
            int xStart = 0, xEnd = 0, yStart = 0, yEnd = 0;

            //Find which side to get
            switch (side)
            {
                case 0:
                    xStart = 0; xEnd = width;
                    yStart = 0; yEnd = range * 2;
                    break;
                case 1:
                    xStart = 0; if (range % 2 == 0) xEnd = range / 2; else xEnd = range / 2 + 1;
                    yStart = 0; yEnd = height;
                    break;
                case 2:
                    xStart = 0; xEnd = width;
                    yStart = height - range * 2; yEnd = height;
                    break;
                case 3:
                    if (range % 2 == 0) xStart = width - range / 2; else xStart = width - (range / 2 + 1); xEnd = width;
                    yStart = 0; yEnd = height;
                    break;
            }

            //Build the tile list
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    area.Add(GetTile(x, y));
                }
            }

            return area;
        }
        #endregion

        #region GetMoveArea
        public List<HexTile> GetMoveArea(Point startPoint, int mpRemaining, UnitType unitType)
        {
            List<HexTile> area = new List<HexTile>();
            int startX = startPoint.X - mpRemaining;
            int startY = startPoint.Y - mpRemaining * 2;
            int endX = startPoint.X + mpRemaining;
            int endY = startPoint.Y + mpRemaining * 2 + 1;

            if (startX < 0) startX = 0; if (endX > width - 1) endX = width - 1;
            if (startY < 0) startY = 0; if (endY > height - 1) endY = height - 1;

            area.Add(GetTile(startPoint));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    int mpCost = GetMoveCost(startPoint, new Point(x, y), unitType);
                    if (mpCost <= mpRemaining && mpCost > 0)
                    {
                        if (!area.Contains(GetTile(x, y)))
                            area.Add(GetTile(x, y));
                    }
                }
            }

            return area;
        }
        #endregion

        #region Turns
        // returns  -1 if (x0,y0)-->(x1,y1)-->(x2,y2) turns to the left,
        //          1 if (x0,y0)-->(x1,y1)-->(x2,y2) turns to the right
        //          0 if (x2,y2) is colinear with (x0,y0)-->(x1,y1)
        public static int Turns(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            double cross;
            cross = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0);
            return ((cross < 0.0) ? LEFT : ((cross == 0.0) ? STRAIGHT : RIGHT));
        }
        #endregion

        #region NextHex
        /// <summary>
        /// A routine that finds the next hex or 2 after cur, making
        /// sure it's/they're not cur2 (or cur) or last1 or last2.  
        /// </summary>
        /// <param name="cur">The first hex that is currently being checked</param>
        /// <param name="cur2">The second hex if any currently being checked</param>
        /// <param name="next1">Set to the first next hex to be checked</param>
        /// <param name="next2">Set to second next hex, if any</param>
        /// <param name="last1">The last hex checked</param>
        /// <param name="last2">The second last hex checked if any</param>
        /// <param name="x0">X coord of the line's starting point</param>
        /// <param name="y0">Y coord of the line's starting point</param>
        /// <param name="x1">X coord of the line's endpoint</param>
        /// <param name="y1">Y coord of the line's endpoint</param>
        /// <returns>Returns the total number of next Hex found (should always be 1 or 2)</returns>
        private int NextHex(ref HexTile cur, ref HexTile cur2, out HexTile next1, out HexTile next2,
                      ref HexTile last1, ref HexTile last2,
                      double x0, double y0, double x1, double y1)
        {
            HexTile h;
            int turn1, turn2;

            next1 = null; next2 = null;

            Point[] hexNeighbours;

            if (cur.MapPosition.Y % 2 > 0)
                hexNeighbours = HexNeighboursOdd;
            else
                hexNeighbours = HexNeighboursEven;

            for (int i = 0; i < 6; i++)
            {
                turn1 = Turns(x0, y0, x1, y1, cur.Points[i].X, cur.Points[i].Y);
                turn2 = Turns(x0, y0, x1, y1, cur.Points[(i + 1) % 6].X, cur.Points[(i + 1) % 6].Y);

                if (turn1 == STRAIGHT || turn2 == STRAIGHT || turn1 != turn2)
                {

                    // in each of these cases we'll have to consider the hexagon
                    // adjacent to edge (i,i+1)
                    h = GetTile(cur.MapPosition.X + hexNeighbours[i].X,
                            cur.MapPosition.Y + hexNeighbours[i].Y);

                    if (h != null)
                    {
                        //make sure we start in the right direction
                        if (last1 == null &&
                            (Math.Abs(x1 - h.Center.X) <= Math.Abs(x1 - cur.Center.X) &&
                            Math.Abs(h.Center.Y - y1) <= Math.Abs(cur.Center.Y - y1)))
                        {

                            if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                h != last1 && h != last2)
                            {
                                if (next1 == null) next1 = h;
                                else if (next2 == null) next2 = h;
                            }
                        }
                        else if (last1 != null)
                        {
                            if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                   h != last1 && h != last2)
                            {
                                if (next1 == null) next1 = h;
                                else if (next2 == null) next2 = h;
                            }
                        }
                    }

                    if (turn1 == STRAIGHT)
                    {
                        // current vertex (i) lies on the line
                        // hex next to edge (i-1,i)
                        h = GetTile(cur.MapPosition.X + hexNeighbours[(i + 5) % 6].X,
                            cur.MapPosition.Y + hexNeighbours[(i + 5) % 6].Y);

                        if (h != null)
                        {
                            if (last1 == null &&
                                (Math.Abs(x1 - h.Center.X) <= Math.Abs(x1 - cur.Center.X) || x1 - cur.Center.X == 0.0) &&
                                (Math.Abs(h.Center.Y - y1) <= Math.Abs(cur.Center.Y - y1) || cur.Center.Y - y1 == 0.0))
                            {

                                if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                    h != last1 && h != last2)
                                {
                                    if (next1 == null) next1 = h;
                                    else if (next2 == null) next2 = h;
                                }
                            }
                            else if (last1 != null)
                            {

                                if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                    h != last1 && h != last2)
                                {
                                    if (next1 == null) next1 = h;
                                    else if (next2 == null) next2 = h;
                                }
                            }
                        }
                    }
                    if (turn2 == STRAIGHT)
                    {
                        // next vertex (i+1) lies on the line
                        // hex next to edge (i+1,i+2)
                        h = GetTile(cur.MapPosition.X + hexNeighbours[(i + 1) % 6].X,
                                cur.MapPosition.Y + hexNeighbours[(i + 1) % 6].Y);

                        if (h != null)
                        {
                            if (last1 == null &&
                                (Math.Abs(x1 - h.Center.X) <= Math.Abs(x1 - cur.Center.X) || x1 - cur.Center.X == 0.0) &&
                                (Math.Abs(h.Center.Y - y1) <= Math.Abs(cur.Center.Y - y1) || cur.Center.Y - y1 == 0.0))
                            {

                                if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                    h != last1 && h != last2)
                                {
                                    if (next1 == null) next1 = h;
                                    else if (next2 == null) next2 = h;
                                }
                            }
                            else if (last1 != null)
                            {
                                if (h != cur && h != cur2 && h != next1 && h != next2 &&
                                    h != last1 && h != last2)
                                {
                                    if (next1 == null) next1 = h;
                                    else if (next2 == null) next2 = h;
                                }
                            }
                        }
                    }
                }
            }
            return ((next2 == null) ? 1 : 2);

        }
        #endregion

        #region GetObscurement
        /// <summary>
        /// Draws a euclidean line from the origin to the target hex, then checks all Hex that
        /// intersect that line for obscurement due to terrain or unit/elevation occlusion
        /// </summary>
        /// <param name="origin">The point in hex map coordinates of the unit to check for</param>
        /// <param name="target">The point in hex map coordinates of the target</param>
        /// <returns></returns>
        public int GetObscurement(Point origin, Point target)
        {
            if (origin == target)
                return 0;

            if (target.X < 0 || target.X > width - 1 ||
                target.Y < 0 || target.Y > height - 1)
                throw new ArgumentException("Invalid target point");

            HexTile h0 = GetTile(origin.X, origin.Y);
            HexTile h1 = GetTile(target.X, target.Y);

            HexTile next1, next2, cur1, cur2, last1, last2;

            int obscurement = 0;

            cur1 = h0;
            cur2 = null;
            last1 = last2 = null;
            do
            {
                if (cur1 == h1)
                {
                    //if the target is behind an elevation change, and thus in an elevation "dead zone"
                    //it is occluded
                    if (last1.Elevation - cur1.Elevation > 0)
                        return 1000;
                    else
                        return obscurement;
                }

                next1 = next2 = null;

                NextHex(ref cur1, ref cur2, out next1, out next2, ref last1, ref last2,
                      h0.Center.X, h0.Center.Y, h1.Center.X, h1.Center.Y);

                if (cur2 != null && next1 == null)
                    NextHex(ref cur2, ref cur1, out next1, out next2, ref last1, ref last2,
                        h0.Center.X, h0.Center.Y, h1.Center.X, h1.Center.Y);

                last1 = cur1; last2 = cur2;
                cur1 = next1; cur2 = next2;

                #region Apply Obscurement Modifiers

                switch (cur1.HexType)
                {
                    case HexType.Jungle:
                        obscurement += 2;
                        break;
                    case HexType.Woodland:
                    case HexType.Swamp:
                        obscurement += 1;
                        break;
                }

                //if any intervening tiles are occupied or higher than origin and target, sight is occluded
                if (cur1 != h1)
                {
                    if (cur1.Elevation - h0.Elevation > 0 &&
                        cur1.Elevation - h1.Elevation > 0)
                        return 1000;

                    for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                    {
                        Player player = HeavyGearManager.Player(i);
                        foreach (Unit unit in player.Units)
                        {
                            if (unit.MapPosition == cur1.MapPosition)
                                return 1000;
                        }
                    }
                }
                /*
                if (cur2 != null)
                {
                    switch (cur2.HexType)
                    {
                        case HexType.Jungle:
                            obscurement += 2;
                            break;
                        case HexType.Woodland:
                        case HexType.Swamp:
                            obscurement += 1;
                            break;
                    }

                    //if any intervening tiles are occupied or higher than origin and target, sight is occluded
                    if (cur2 != h1)
                    {
                        if (cur2.Elevation - h0.Elevation > 0 &&
                            cur2.Elevation - h1.Elevation > 0)
                            return 1000;

                        for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                        {
                            Player player = HeavyGearManager.Player(i);
                            foreach (Unit unit in player.Army)
                            {
                                if (unit.MapPosition == cur2.MapPosition)
                                    return 1000;
                            }

                        }
                    }
                }*/
                #endregion

            } while (true);
        }
        #endregion

        #region GetLOSArea
        /// <summary>
        /// Searches through the unit's LOS for a target point
        /// </summary>
        /// <param name="unit">The unit for whom LOS is being checked</param>
        /// <param name="TilesInLOS">List to put the tiles into</param>
        /// <returns>true if the hex is within sight, false if not</returns>
        public void GetLOSArea(Unit unit, out List<HexTile> TilesInLOS)
        {
            TilesInLOS = new List<HexTile>();

            Point origin = unit.MapPosition;
            HexTile originTile = GetTile(origin.X, origin.Y);

            int unitFacing = (int)Math.Round(MathHelper.ToDegrees(unit.Rotation));

            #region Check Sight Range
            int sightRange = 0;
            if (unit.UnitType != UnitType.Infantry)
            {
                if (!((Vehicle)unit).SensorsDestroyed)
                    sightRange = ((Vehicle)unit).SensorRange;
                else
                    sightRange = 20;
            }
            else
            {
                sightRange = 20;
            }

            #endregion

            #region Get Passive Detection Value
            int detection;
            if (unit.UnitType != UnitType.Infantry)
                detection = ((Vehicle)unit).Crew.ElectronicWarfare.Level + ((Vehicle)unit).Sensors;
            else
                detection = 4;

            if (detection < 4)
                detection = 4;
            #endregion

            #region Setup LOS Area
            // the directions that the clockwise (left) most and counterclockwise (right) most arms of the arc will extend
            int leftFacing = 0, rightFacing = 0;
            //int hexStart = 0, hexEnd = 0;
            switch (unitFacing)
            {
                case FacingInt.North:
                    leftFacing = FacingInt.NorthWest;
                    rightFacing = FacingInt.NorthEast;
                    break;
                case FacingInt.NorthEast:
                    leftFacing = FacingInt.North;
                    rightFacing = FacingInt.SouthEast;
                    break;
                case FacingInt.SouthEast:
                    leftFacing = FacingInt.NorthEast;
                    rightFacing = FacingInt.South;
                    break;
                case FacingInt.South:
                    leftFacing = FacingInt.SouthEast;
                    rightFacing = FacingInt.SouthWest;
                    break;
                case FacingInt.SouthWest:
                    leftFacing = FacingInt.South;
                    rightFacing = FacingInt.NorthWest;
                    break;
                case FacingInt.NorthWest:
                    leftFacing = FacingInt.SouthWest;
                    rightFacing = FacingInt.North;
                    break;
            }

            #endregion

            #region Get LOS Area
            //get the left and right arms of the cone
            Point leftArm = origin;
            Point rightArm = origin;

            #region Get Left Arm
            //moves the arm out 1 Hex to get the line of the arc's arm
            int leftRow = leftArm.Y;
            switch (leftFacing)
            {
                case FacingInt.North:
                    leftArm.Y -= 2;
                    break;
                case FacingInt.NorthEast:
                case FacingInt.NorthWest:
                    leftArm.Y -= 1;
                    break;
                case FacingInt.SouthEast:
                case FacingInt.SouthWest:
                    leftArm.Y += 1;
                    break;
                case FacingInt.South:
                    leftArm.Y += 2;
                    break;
            }
            if (leftRow % 2 > 0)
            {
                //odd rows
                switch (leftFacing)
                {
                    case FacingInt.NorthEast:
                    case FacingInt.SouthEast:
                        leftArm.X += 1;
                        break;
                }
            }
            else
            {
                //even rows
                switch (leftFacing)
                {
                    case FacingInt.SouthWest:
                    case FacingInt.NorthWest:
                        leftArm.X -= 1;
                        break;
                }
            }

            #endregion

            #region Get Right Arm
            int rightRow = rightArm.Y;
            switch (rightFacing)
            {
                case FacingInt.North:
                    rightArm.Y -= 2;
                    break;
                case FacingInt.NorthEast:
                case FacingInt.NorthWest:
                    rightArm.Y -= 1;
                    break;
                case FacingInt.SouthEast:
                case FacingInt.SouthWest:
                    rightArm.Y += 1;
                    break;
                case FacingInt.South:
                    rightArm.Y += 2;
                    break;
            }
            if (rightRow % 2 > 0)
            {
                //odd rows
                switch (rightFacing)
                {
                    case FacingInt.NorthEast:
                    case FacingInt.SouthEast:
                        rightArm.X += 1;
                        break;
                }
            }
            else
            {
                //even rows
                switch (rightFacing)
                {
                    case FacingInt.SouthWest:
                    case FacingInt.NorthWest:
                        rightArm.X -= 1;
                        break;
                }
            }

            #endregion

            //Get the target tiles at the end of the arcs
            HexTile leftTarget = GetTile(leftArm.X, leftArm.Y);
            HexTile rightTarget = GetTile(rightArm.X, rightArm.Y);

            //if the targets are null, construct "fake" hex tiles
            //so that the arm lines may be drawn
            if (leftTarget == null) leftTarget = new HexTile(leftArm);
            if (rightTarget == null) rightTarget = new HexTile(rightArm);

            List<HexTile> tilesToCheck = new List<HexTile>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    HexTile tile = GetTile(x, y);
                    if (tile.InsideArc(originTile, leftTarget, rightTarget))
                    {
                        tilesToCheck.Add(tile);
                    }
                }
            }

            #region Check Tiles for Obscurement

            foreach (HexTile tile in tilesToCheck)
            {
                int obscurement = GetObscurement(origin, tile.MapPosition);
                int range = GetRange(origin, tile.MapPosition);
                if (range <= sightRange &&
                     obscurement <= detection)
                {
                    TilesInLOS.Add(tile);
                }
            }

            #endregion
            
            #endregion

        }
        #endregion

        #region CheckLOS
        /// <summary>
        /// Checks for LOS between two units without building the full LOS area.
        /// Directly tests range, arc membership, and obscurement for the target tile only.
        /// </summary>
        public bool CheckLOS(Unit unit, Unit target)
        {
            Point origin = unit.MapPosition;
            Point targetPos = target.MapPosition;

            if (origin == targetPos)
                return true;

            // Sight range
            int sightRange;
            if (unit.UnitType != UnitType.Infantry)
                sightRange = ((Vehicle)unit).SensorsDestroyed ? 20 : ((Vehicle)unit).SensorRange;
            else
                sightRange = 20;

            // Bail early if out of range
            int range = GetRange(origin, targetPos);
            if (range > sightRange)
                return false;

            // Detection threshold
            int detection;
            if (unit.UnitType != UnitType.Infantry)
                detection = ((Vehicle)unit).Crew.ElectronicWarfare.Level + ((Vehicle)unit).Sensors;
            else
                detection = 4;
            if (detection < 4)
                detection = 4;

            // Determine the same arc boundaries used by GetLOSArea
            int unitFacing = (int)Math.Round(MathHelper.ToDegrees(unit.Rotation));
            int leftFacing = 0, rightFacing = 0;
            switch (unitFacing)
            {
                case FacingInt.North:     leftFacing = FacingInt.NorthWest; rightFacing = FacingInt.NorthEast; break;
                case FacingInt.NorthEast: leftFacing = FacingInt.North;     rightFacing = FacingInt.SouthEast; break;
                case FacingInt.SouthEast: leftFacing = FacingInt.NorthEast; rightFacing = FacingInt.South;     break;
                case FacingInt.South:     leftFacing = FacingInt.SouthEast; rightFacing = FacingInt.SouthWest; break;
                case FacingInt.SouthWest: leftFacing = FacingInt.South;     rightFacing = FacingInt.NorthWest; break;
                case FacingInt.NorthWest: leftFacing = FacingInt.SouthWest; rightFacing = FacingInt.North;     break;
                default:                  leftFacing = FacingInt.NorthWest; rightFacing = FacingInt.NorthEast; break;
            }

            Point leftArm  = origin;
            Point rightArm = origin;
            StepArmPoint(ref leftArm,  leftFacing,  leftArm.Y);
            StepArmPoint(ref rightArm, rightFacing, rightArm.Y);

            HexTile originTile  = GetTile(origin.X, origin.Y);
            HexTile leftTarget  = GetTile(leftArm.X,  leftArm.Y)  ?? new HexTile(leftArm);
            HexTile rightTarget = GetTile(rightArm.X, rightArm.Y) ?? new HexTile(rightArm);
            HexTile targetTile  = GetTile(targetPos.X, targetPos.Y);

            if (targetTile == null || !targetTile.InsideArc(originTile, leftTarget, rightTarget))
                return false;

            return GetObscurement(origin, targetPos) <= detection;
        }
        #endregion

        #region StepArmPoint
        /// <summary>
        /// Advances a hex-grid point one step in the given facing direction.
        /// Used when computing LOS arc boundary arms.
        /// </summary>
        private static void StepArmPoint(ref Point arm, int facing, int row)
        {
            switch (facing)
            {
                case FacingInt.North:                            arm.Y -= 2; break;
                case FacingInt.NorthEast: case FacingInt.NorthWest: arm.Y -= 1; break;
                case FacingInt.SouthEast: case FacingInt.SouthWest: arm.Y += 1; break;
                case FacingInt.South:                            arm.Y += 2; break;
            }
            if (row % 2 > 0)
            {
                if (facing == FacingInt.NorthEast || facing == FacingInt.SouthEast) arm.X += 1;
            }
            else
            {
                if (facing == FacingInt.SouthWest || facing == FacingInt.NorthWest) arm.X -= 1;
            }
        }
        #endregion

        #region GetMoveCost
        public int GetMoveCost(Point startPoint, Point destination, UnitType unitType)
        {
            List<HexTile> path = GetMovePath(startPoint, destination);
            HexTile current, next;

            if (path == null)
                return 1000;

            int moveCost = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                current = path[i];
                next = path[i + 1];
                moveCost += GetTileCost(unitType, ref current, ref next);
            }

            return moveCost;
        }
        #endregion

        #region GetMovePath
        /// <summary>
        /// Attempts to find the optimal movement path between two hex tiles
        /// </summary>
        /// <param name="origin">the origin tile in hex grid ccoordinates</param>
        /// <param name="destination">the destination tile in hex grid coordinates</param>
        /// <returns>A list of tiles representing the path</returns>
        public List<HexTile> GetMovePath(Point origin, Point destination)
        {
            if (origin == destination)
                return null;

            if (destination.X < 0 || destination.X > width - 1 ||
                destination.Y < 0 || destination.Y > height - 1)
                throw new ArgumentException("Invalid target point");

            HexTile h0 = GetTile(origin.X, origin.Y);
            HexTile h1 = GetTile(destination.X, destination.Y);

            HexTile next1, next2, cur1, cur2, last1, last2;

            List<HexTile> path = new List<HexTile>();
            //Add the origin tile for move cost checking
            path.Add(h0);

            cur1 = h0;
            cur2 = null;
            last1 = last2 = null;
            do
            {
                if (cur1 == h1)
                {
                    if (!path.Contains(cur1))
                        path.Add(cur1);
                    return path;
                }

                next1 = next2 = null;

                NextHex(ref cur1, ref cur2, out next1, out next2, ref last1, ref last2,
                      h0.Center.X, h0.Center.Y, h1.Center.X, h1.Center.Y);

                if (cur2 != null && next1 == null)
                    NextHex(ref cur2, ref cur1, out next1, out next2, ref last1, ref last2,
                        h0.Center.X, h0.Center.Y, h1.Center.X, h1.Center.Y);

                last1 = cur1; last2 = cur2;
                cur1 = next1; cur2 = next2;
                bool addHex = true;

                #region Check whether to add the tile to the path

                for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                {
                    Player player = HeavyGearManager.Player(i);
                    foreach (Unit unit in player.Units)
                    {
                        if (unit.MapPosition == cur1.MapPosition)
                        {
                            addHex = false;
                            break;
                        }
                    }
                    if (!addHex)
                        break;
                }
                if (addHex)
                    path.Add(cur1);
                else if (cur2 != null)
                {
                    addHex = true;
                    for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                    {
                        Player player = HeavyGearManager.Player(i);
                        foreach (Unit unit in player.Units)
                        {
                            if (unit.MapPosition == cur2.MapPosition)
                            {
                                addHex = false;
                                break;
                            }
                        }
                        if (!addHex)
                            break;
                    }

                    if (addHex)
                        path.Add(cur2);
                }
                //else
                //    return null;
                #endregion

            } while (true);
        }
        #endregion

        #region GetTileCost
        /// <summary>
        /// Gets the Movement Point cost of moving between one tile and the next
        /// </summary>
        /// <param name="unitTyoe">the type of unit moving</param>
        /// <param name="currentTile">reference to the tile the unit is on</param>
        /// <param name="destinationTile">reference to the destination tile</param>
        /// <returns>returns 0 if the unit cannot cross because of elevation or a unit occupying the destination, 
        /// else returns the MP cost to move to the destination tile</returns>
        public int GetTileCost(UnitType unitType, ref HexTile currentTile, ref HexTile destinationTile)
        {
            int tileCost = 0;

            #region Check for unreachable destination tile

            if (Math.Abs(destinationTile.Elevation - currentTile.Elevation) > 1)
                return tileCost;

            for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
            {
                Player player = HeavyGearManager.Player(i);
                foreach (Unit unit in player.Units)
                {
                    if (unit.MapPosition == destinationTile.MapPosition)
                        return tileCost;
                }
            }

            #endregion

            #region Find MP Cost

            if (unitType == UnitType.Walker)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                    case HexType.Rough:
                    case HexType.Woodland:
                        tileCost = 1;
                        break;
                    case HexType.Sand:
                    case HexType.Jungle:
                    case HexType.Water:
                        tileCost = 2;
                        break;
                    case HexType.Swamp:
                        tileCost = 3;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 2;
                else if (currentTile.Elevation - destinationTile.Elevation == 1)
                    tileCost += 1;
            }

            if (unitType == UnitType.Ground || unitType == UnitType.Infantry)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                        tileCost = 1;
                        break;
                    case HexType.Rough:
                    case HexType.Woodland:
                    case HexType.Sand:
                        tileCost = 2;
                        break;
                    case HexType.Jungle:
                    case HexType.Water:
                        tileCost = 3;
                        break;
                    case HexType.Swamp:
                        tileCost = 4;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 2;
            }

            if (unitType == UnitType.Hover)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                    case HexType.Rough:
                    case HexType.Sand:
                    case HexType.Swamp:
                    case HexType.Water:
                        tileCost = 1;
                        break;
                    case HexType.Jungle:
                    case HexType.Woodland:
                        tileCost = 2;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 4;
            }

            #endregion

            return tileCost;
        }
        #endregion

        #region ResetScreenPositions
        /// <summary>
        /// Resets the pixel coordinates of all hexes in the map
        /// </summary>
        public void ResetScreenPositions()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GetTile(x, y).ResetScreenPosition();
                }
            }
        }
        #endregion

        /*
        // Hexagons of a bubble are ordered by counter-clockwise
        // indexing.  This returns the hexagon for the given index and radius.
        // Returns null if the hex is not within our rectangular grid.
        public HexTile HexOnCircle(int r, int t, Point center)
        {
            int side, e;
            Point mapPosition = new Point();
            //c.toHex();
            side = (t / r) % 6;  // find which side it's on
            e = t % r;

            Point[] hexNeighbours;

            mapPosition = center;

            // locate first hex on that side
            for (int i = 0; i < r; i++)
            {
                if (mapPosition.Y % 2 == 0)
                    hexNeighbours = HexNeighboursEven;
                else
                    hexNeighbours = HexNeighboursOdd;

                mapPosition.X += hexNeighbours[side].X;
                mapPosition.Y += hexNeighbours[side].Y;
            }
            //mapPosition.X = center.X + r * (hexNeighbours[(side + 4) % 6].X); 
           // mapPosition.Y = center.Y + r * (hexNeighbours[(side + 4) % 6].Y);
            for (int i = 0; i < e; i++)
            {
                if (mapPosition.Y % 2 == 0)
                    hexNeighbours = HexNeighboursEven;
                else
                    hexNeighbours = HexNeighboursOdd;

                mapPosition.X += hexNeighbours[(side + 4) % 6].X;
                mapPosition.Y += hexNeighbours[(side + 4) % 6].Y;
            }

            return GetTile(mapPosition);
        }

        public HexTile FakeHexOnCircle(int r, int t, Point center)
        {
            int side, e;
            Point mapPosition = new Point();
            //c.toHex();
            side = (t / r) % 6;  // find which side it's on
            e = t % r;

            Point[] hexNeighbours;

            mapPosition = center;

            // locate first hex on that side
            for (int i = 0; i < r; i++)
            {
                if (mapPosition.Y % 2 == 0)
                    hexNeighbours = HexNeighboursEven;
                else
                    hexNeighbours = HexNeighboursOdd;

                mapPosition.X += hexNeighbours[side].X;
                mapPosition.Y += hexNeighbours[side].Y;
            }
            //mapPosition.X = center.X + r * (hexNeighbours[(side + 4) % 6].X); 
            // mapPosition.Y = center.Y + r * (hexNeighbours[(side + 4) % 6].Y);
            for (int i = 0; i < e; i++)
            {
                if (mapPosition.Y % 2 == 0)
                    hexNeighbours = HexNeighboursEven;
                else
                    hexNeighbours = HexNeighboursOdd;

                mapPosition.X += hexNeighbours[(side + 4) % 6].X;
                mapPosition.Y += hexNeighbours[(side + 4) % 6].Y;
            }

            return new HexTile(mapPosition);
        }

        // Assuming the current arcs are at radius r, this computes the
        // next set of arcs at radius r+1
        void NextArc(int r)
        {
            Arc a,ahead,atail,anext;
      
            // split up arcs according to obstacles
            ahead = null;
            atail = null;
            a = arcList;
            while (a != null) 
            {
                anext = a.next;
                a.next = null;
                a = a.breakArc(this,r);
                if (a==null) 
                {
                    // nothing visible here---need to merge adjacent arcs...
                } 
                else 
                {
                    if (atail!=null) atail.next = a;
                    if (ahead==null) ahead = a;
                    atail = a;
                    while (atail.next !=null && atail.next != anext) 
                    {
                        atail = atail.next;
                    }
                }
                a = anext;
            }
            arcList = ahead;
      
            // now to expand our bubble out one radius unit
            a = arcList;
            while(a != null) 
            {
                a.expandArc(this,r);
                a = a.next;
            }
        }

        // Does the first part of expansion
        void SplitArc(int r)
        {
            Arc arc, arcHead, arcTail, arcNext;

            // split up arcs according to obstacles
            arcHead = null;
            arcTail = null;
            arc = arcList;
            while (arc != null)
            {
                arcNext = arc.next;
                arc.next = null;
                arc = arc.breakArc(this, r);
                if (arc != null)
                {
                    if (arcTail != null) arcTail.next = arc;
                    if (arcHead == null) arcHead = arc;
                    arcTail = arc;
                    while (arcTail.next != null && arcTail.next != arcNext)
                    {
                        arcTail = arcTail.next;
                    }
                }
                arc = arcNext;
            }
            arcList = arcHead;
        }*/
    }
}