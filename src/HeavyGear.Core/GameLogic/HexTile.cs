#region File Description
//-----------------------------------------------------------------------------
// HexTile.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HeavyGear.Graphics;
#endregion

namespace HeavyGear.GameLogic
{
    /// <summary>
    /// Class used to represent a hex tile within the game
    /// </summary>

    public enum HexType
    {
        Clear,
        Rough,
        Sand,
        Woodland,
        Jungle,
        Swamp,
        Water
    }
    
    [Serializable]
    public class HexTile
    {
        #region Fields
        private int elevation = 0;
        private HexType hexType = HexType.Clear;
        /// <summary>
        /// Whether to highlight this tile
        /// </summary>
        private bool highlight = false;
        /// <summary>
        /// Map Position in Hex Grid Coordinates
        /// </summary>
        private Point mapPosition;
        /// <summary>
        /// Standardized Position in Pixel Coordinates, used for LOS algorithims to be more accurate
        /// </summary>
        private Vector2 position;
        /// <summary>
        /// Screen position for relative resolution, recalculated when resolution changes
        /// </summary>
        private Vector2 screenPosition;
        /// <summary>
        /// Center of the hex
        /// </summary>
        private Vector2 center;
        /// <summary>
        /// The hex's vertices starting with the top left
        /// </summary>
        private Vector2[] points = new Vector2[6];

        #endregion

        #region Initialization
        public HexTile(Point mapPosition, int elevation, HexType hexType)
        {
            this.elevation = elevation;
            this.hexType = hexType;

            Load(mapPosition);
        }
        /// <summary>
        /// Blank constructor for serialization
        /// </summary>
        public HexTile(Point mapPosition)
        {
            Load(mapPosition);
        }
        public HexTile(int x, int y)
        {
            Load(new Point(x, y));
        }
        /// <summary>
        /// Creates a new hex for a given point on the map
        /// </summary>
        /// <param name="mapPosition"></param>
        private void Load(Point mapPosition)
        {
            this.mapPosition = mapPosition;

            //Get Standardized Position
            BaseGame.ConvertMapToPixelStandardized(ref mapPosition, out position);
            //
            BaseGame.ConvertMapToPixel(ref mapPosition, out screenPosition);

            points = new Vector2[] {
                new Vector2(position.X + 37, position.Y), //Top Left
                new Vector2(position.X + 111, position.Y),//Top Right
                new Vector2(position.X + 148, position.Y + 64),//Right
                new Vector2(position.X + 111, position.Y + 128),//Bottom Right
                new Vector2(position.X + 37, position.Y + 128),//Bottom Left
                new Vector2(position.X, position.Y + 64)};//Left


            center.X = position.X + 74;
            center.Y = position.Y + 64;
        }

        #endregion

        #region Properties
        public bool Highlight
        {
            get
            {
                return highlight;
            }
            set
            {
                highlight = value;
            }
        }
        public Point MapPosition
        {
            get
            {
                return mapPosition;
            }
        }
        public Vector2 Position
        {
            get
            {
                return position;
            }
        }
        public Vector2 Center
        {
            get
            {
                return center;
            }
        }
        public Vector2[] Points
        {
            get
            {
                return points;
            }
        }
        public int Elevation
        {
            get
            {
                return this.elevation;
            }
            set
            {
                this.elevation = value;
            }
        }

        public HexType HexType
        {
            get
            {
                return this.hexType;
            }
            set
            {
                this.hexType = value;
            }
        }
        public Vector2 ScreenPosition
        {
            get
            {
                return screenPosition;
            }
        }
        #endregion

        #region Methods
        public void ResetScreenPosition()
        {
            BaseGame.ConvertMapToPixel(ref mapPosition, out screenPosition);
        }
        /// <summary>
        /// Method to determine if one line intersects another
        /// </summary>
        /// <param name="x0">x of the first line</param>
        /// <param name="y0">y of the first line</param>
        /// <param name="x1">x of the second line</param>
        /// <param name="y1">y of the second line</param>
        /// <returns>true if this hexagon intersects the line passing through the given two points.</returns>
        public bool Intersects(double x0,double y0,double x1,double y1) 
        {
            int side1,i,j;
            side1 = Map.Turns(x0,y0,x1,y1,points[0].X,points[0].Y);
            if (side1 == Map.STRAIGHT) return true;
            for (i=1;i<6;i++) 
            {
                j = Map.Turns( x0, y0, x1, y1, points[i].X, points[i].Y);
                if (j == Map.STRAIGHT || j != side1) return true;
            }
            return false;
        }

        /// <summary>
        /// Method to tell if a hexagon is "left" of a given boundry
        /// </summary>
        /// <param name="hc">the hex the line originates from</param>
        /// <param name="ac">A hex lying on the clockwise arm of the arc</param>
        /// <returns>true if the hex is "left" or counterclockwise of a given line between two hexes</returns>
        public bool LeftOfArc(HexTile hc, HexTile ac)
        {
            int i, carm;
            for (i = 0; i < 6; i++)
            {
                carm = Map.Turns(hc.Center.X, hc.Center.Y, ac.Center.X, ac.Center.Y, points[i].X, points[i].Y);
                if (carm == Map.LEFT) 
                    return true;
            }
            return false;
        }

        public bool LeftOfArc(HexTile hc, double cx, double cy)
        {
            int i, carm;
            for (i = 0; i < 6; i++)
            {
                carm = Map.Turns(hc.Center.X, hc.Center.Y, cx, cy, points[i].X, points[i].Y);
                if (carm == Map.LEFT)
                    return true;
            }
            return false;
        }

        // A routine to tell if a hexagon has any part of it is "right" of a given
        // arc boundary (counter-clockwise arm of the cone)
        public bool RightOfArc(HexTile hc, HexTile acc)
        {
            int i, ccarm;
            for (i = 0; i < 6; i++)
            {
                ccarm = Map.Turns(hc.Center.X, hc.Center.Y, acc.Center.X, acc.Center.Y, points[i].X, points[i].Y);
                if (ccarm == Map.RIGHT) 
                    return true;
            }
            return false;
        }

        public bool RightOfArc(HexTile hc, double ccx, double ccy)
        {
            int i, ccarm;
            for (i = 0; i < 6; i++)
            {
                ccarm = Map.Turns(hc.Center.X, hc.Center.Y, ccx, ccy, points[i].X, points[i].Y);
                if (ccarm == Map.RIGHT)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether this hex is inside the arc defined by the center hex and the two arms
        /// </summary>
        /// <param name="center">The center hex the cone radiates out from, ie tile unit is standing on</param>
        /// <param name="clockwiseArm">Any hex (except arc center) on the right or clockwise most arm</param>
        /// <param name="counterClockwiseArm">Any hex (except arc center) on the left or counterclockwise most arm</param>
        /// <param name="facing">The quadrant this cone is in</param>
        /// <returns></returns>
        public bool InsideArc(HexTile centerHex, HexTile clockwiseArm, HexTile counterClockwiseArm)
        {
            if (RightOfArc(centerHex, clockwiseArm) && LeftOfArc(centerHex, counterClockwiseArm))
                return true;
            
            return false;
        }

        #endregion
    }
}

