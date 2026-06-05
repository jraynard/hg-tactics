#region File Description
//-----------------------------------------------------------------------------
// BaseGame.cs
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
using System.Threading;
using HeavyGear;
using HeavyGear.GameLogic;
using HeavyGear.Helpers;
using HeavyGear.Properties;
using HeavyGear.Sounds;
using HeavyGear.Shaders;
#endregion

namespace HeavyGear.Graphics
{
    /// <summary>
    /// Base game class for all the basic game support.
    /// Connects all our helper classes together and makes our life easier!
    /// Note: This game was designed for 16:9 (e.g. 1920x1200), but it
    /// also works just fine for 4:3 (1024x768, 800x600, etc.).
    /// This is the reason you might see 1024x640 instead of 1024x768 in
    /// some width/height calculations and the UI textures.
    /// The game looks best at 1920x1080 (HDTV 1080p resolution).
    /// </summary>
    public partial class BaseGame : Microsoft.Xna.Framework.Game
    {
        #region Constants
        private const float MouseCameraRate = 1200f;
        private const float CameraRate = 10.0f;
        /// <summary>
        /// Background color
        /// </summary>
        private static readonly Color BackgroundColor = Color.Black;
        /// <summary>
        /// Field of view and near and far plane distances for the
        /// ProjectionMatrix creation.
        /// </summary>
        private const float FieldOfView = (float)Math.PI / 2,
            NearPlane = 0.5f,
            FarPlane = 1750;

        /// <summary>
        /// Viewable field of view for object visibility testing (see Model class)
        /// </summary>
        public const float ViewableFieldOfView = FieldOfView / 1.125f;
        #endregion

        #region Variables
        /// <summary>
        /// Hexagon tile height in pixels
        /// </summary>
        private static float tileHeight = 1;// = BaseGame.graphicsManager.GraphicsDevice.Viewport.Height / 10.9f;
        /// <summary>
        /// Hexagon tile width in pixels
        /// </summary>
        private static float tileWidth = 0;// = BaseGame.graphicsManager.GraphicsDevice.Viewport.Width / 10.8f;
        /// <summary>
        /// Hexagon tile offset for x, set to length of one side of the hexagon + height of side
        /// triangle for vertical Hex or 0 for horizontal Hex
        /// </summary>
        private static float tileXOffset;// = (TileWidth - TileSide) / 2;//111;
        /// <summary>
        /// Length of one side of the hexagon tile, (height/2)/sin60
        /// </summary>
        private static float tileSide;// = (float)((TileHeight / 2) / Math.Sin(60)); //74;
        /// <summary>
        /// Hexagon tile y offset, set to half height for vertical Hex or full height 
        /// for horizontal Hex
        /// </summary>
        private static float tileYOffset;// = TileHeight/2;
        /// <summary>
        /// Camera move rate for Xbox controller camera movement
        /// </summary>
        private static float cameraMoveRate;// = 0.4f * (BaseGame.TileHeight);
        private static float mouseCameraMoveRate;
        private static float zoomValue = 1.0f;
        private static Vector2 tileScale;// = new Vector2(BaseGame.TileWidth + BaseGame.TileSide, BaseGame.TileYOffset);

        private static Vector2 unitOffset;

        public static PlatformID CurrentPlatform = Environment.OSVersion.Platform;

        /// <summary>
        /// Graphics device manager, used for the graphics creation and holds
        /// the GraphicsDevice.
        /// </summary>
        protected static GraphicsDeviceManager graphicsManager = null;

        /// <summary>
        /// Content manager
        /// </summary>
        protected static ContentManager content = null;

        /// <summary>
        /// UI Renderer helper class for all 2d rendering.
        /// </summary>
        protected static UIRenderer ui = null;

        /// <summary>
        /// Our screen resolution: Width and height of visible render area.
        /// </summary>
        protected static int width, height;
        /// <summary>
        /// Aspect ratio of our current resolution
        /// </summary>
        private static float aspectRatio = 1.0f;

        /// <summary>
        /// Remember windows title to check if we are in certain unit tests!
        /// </summary>
        private static string remWindowsTitle = "";

        /// <summary>
        /// Get windows title to check if we are in certain unit tests!
        /// </summary>
        public static string WindowsTitle
        {
            get
            {
                return remWindowsTitle;
            }
        }

        /// <summary>
        /// Line manager 2D
        /// </summary>
        private static LineManager2D lineManager2D = null;
       
        /// <summary>
        /// Matrices for shaders. Used in a similar way than in Rocket Commander,
        /// but since we don't have a fixed function pipeline here we just use
        /// these values in the shader. Make sure to set all matrices before
        /// calling a shader. Inside a shader you have to update the shader
        /// parameter too, just setting the WorldMatrix alone does not work.
        /// </summary>
        private static Matrix worldMatrix,
            viewMatrix,
            projectionMatrix;

        /// <summary>
        /// Light direction, please read matrices info above for more details.
        /// The same things apply here.
        /// </summary>
        private static Vector3 lightDirection = new Vector3(0, 0, 1);

        /// <summary>
        /// Light direction
        /// </summary>
        /// <returns>Vector 3</returns>
        public static Vector3 LightDirection
        {
            get
            {
                return lightDirection;
            }
            set
            {
                lightDirection = value;
                lightDirection.Normalize();
            }
        }

        /// <summary>
        /// Elapsed time this frame in ms. Always have something valid here
        /// in case we devide through this values!
        /// </summary>
        private static float elapsedTimeThisFrameInMs = 0.001f, totalTimeMs = 0,
            lastFrameTotalTimeMs = 0;

        /// <summary>
        /// Helper for calculating frames per second. 
        /// </summary>
        private static float startTimeThisSecond = 0;

        /// <summary>
        /// For more accurate frames per second calculations,
        /// just count for one second, then fpsLastSecond is updated.
        /// Btw: Start with 1 to help some tests avoid the devide through zero
        /// problem.
        /// </summary>
        private static int
            frameCountThisSecond = 0,
            totalFrameCount = 0,
            fpsLastSecond = 60;

        /// <summary>
        /// Return true every checkMilliseconds.
        /// </summary>
        /// <param name="checkMilliseconds">Check ms</param>
        /// <returns>Bool</returns>
        public static bool EveryMillisecond(int checkMilliseconds)
        {
            return (int)(lastFrameTotalTimeMs / checkMilliseconds) !=
                (int)(totalTimeMs / checkMilliseconds);
        }
        #endregion

        #region Properties
        #region Device
        static public GraphicsDevice Device
        {
            get
            {
                return graphicsManager.GraphicsDevice;
            }
        }

        /// <summary>
        /// Back buffer depth format
        /// </summary>
        static DepthFormat backBufferDepthFormat = DepthFormat.Depth24;
        /// <summary>
        /// Back buffer depth format
        /// </summary>
        /// <returns>Surface format</returns>
        public static DepthFormat BackBufferDepthFormat
        {
            get
            {
                return backBufferDepthFormat;
            }
        }

        private static bool alreadyCheckedGraphicsOptions = false;

        /// <summary>
        /// Check options and PS version
        /// </summary>
        internal static void CheckOptionsAndPSVersion()
        {
            GraphicsDevice device = Device;

            if (device == null)
                throw new InvalidOperationException("Device is not created yet!");

            alreadyCheckedGraphicsOptions = true;

            usePostScreenShaders = GameSettings.Default.PostScreenEffects;
            allowShadowMapping = GameSettings.Default.ShadowMapping;
            highDetail = GameSettings.Default.HighDetail;
        }

        /// <summary>
        /// Fullscreen
        /// </summary>
        /// <returns>Bool</returns>
        public static bool Fullscreen
        {
            get
            {
                return graphicsManager.IsFullScreen;
            }
        }

        private static bool highDetail = true;
        /// <summary>
        /// High detail
        /// </summary>
        /// <returns>Bool</returns>
        public static bool HighDetail
        {
            get
            {
                if (alreadyCheckedGraphicsOptions == false)
                    CheckOptionsAndPSVersion();

                return highDetail;
            }
        }

        private static bool allowShadowMapping = true;
        /// <summary>
        /// Allow shadow mapping
        /// </summary>
        /// <returns>Bool</returns>
        public static bool AllowShadowMapping
        {
            get
            {
                if (alreadyCheckedGraphicsOptions == false)
                    CheckOptionsAndPSVersion();

                return allowShadowMapping;
            }
        }

        private static bool usePostScreenShaders = true;
        /// <summary>
        /// Use post screen shaders
        /// </summary>
        /// <returns>Bool</returns>
        public static bool UsePostScreenShaders
        {
            get
            {
                if (alreadyCheckedGraphicsOptions == false)
                    CheckOptionsAndPSVersion();

                return usePostScreenShaders;
            }
        }

        private static bool mustApplyDeviceChanges = false;
        internal static void ApplyResolutionChange()
        {
            int resolutionWidth = GameSettings.Default.ResolutionWidth;
            int resolutionHeight = GameSettings.Default.ResolutionHeight;
            // Use current desktop resolution if autodetect is selected.
            if (resolutionWidth <= 0 ||
                resolutionHeight <= 0)
            {
                resolutionWidth =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                resolutionHeight =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

#if XBOX360
            // Xbox 360 graphics settings are fixed
            graphicsManager.IsFullScreen = true;
            graphicsManager.PreferredBackBufferWidth =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphicsManager.PreferredBackBufferHeight =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else
            graphicsManager.PreferredBackBufferWidth = resolutionWidth;
            graphicsManager.PreferredBackBufferHeight = resolutionHeight;
            graphicsManager.IsFullScreen = GameSettings.Default.Fullscreen;

            mustApplyDeviceChanges = true;
#endif
        }
        #endregion

        #region Content manager
        /// <summary>
        /// Content
        /// </summary>
        /// <returns>Content manager</returns>
        public static new ContentManager Content
        {
            get
            {
                return content;
            }
        }
        #endregion

        #region UI
        /// <summary>
        /// User interface renderer helper ^^
        /// </summary>
        /// <returns>UIRenderer</returns>
        public static UIRenderer UI
        {
            get
            {
                return ui;
            }
        }
        #endregion

        #region Resolution and stuff
        /// <summary>
        /// Width
        /// </summary>
        /// <returns>Int</returns>
        public static int Width
        {
            get
            {
                return width;
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        /// <returns>Int</returns>
        public static int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Aspect ratio
        /// </summary>
        /// <returns>Float</returns>
        public static float AspectRatio
        {
            get
            {
                return aspectRatio;
            }
        }

        /// <summary>
        /// Resolution rectangle
        /// </summary>
        /// <returns>Rectangle</returns>
        public static Rectangle ResolutionRect
        {
            get
            {
#if XBOX360
                return new Rectangle(10, 10, width - 20, height - 20);
#else
                return new Rectangle(0, 0, width, height);
#endif
            }
        }
        #endregion

        #region Calc rectangle helpers
        /// <summary>
        /// XToRes helper method to convert 1024x640 to the current
        /// screen resolution. Used to position UI elements.
        /// </summary>
        /// <param name="xIn1024px">X in 1024px width resolution</param>
        /// <returns>Int</returns>
        public static int XToRes(int xIn1024px)
        {
            return (int)Math.Round(xIn1024px * BaseGame.Width / 1024.0f);
        }

        /// <summary>
        /// YToRes helper method to convert 1024x640 to the current
        /// screen resolution. Used to position UI elements.
        /// </summary>
        /// <param name="yIn640px">Y in 640px height</param>
        /// <returns>Int</returns>
        public static int YToRes(int yIn640px)
        {
            return (int)Math.Round(yIn640px * BaseGame.Height / 640.0f);
        }

        /// <summary>
        /// YTo res 768
        /// </summary>
        /// <param name="yIn768px">Y in 768px</param>
        /// <returns>Int</returns>
        public static int YToRes768(int yIn768px)
        {
            return (int)Math.Round(yIn768px * BaseGame.Height / 768.0f);
        }

        /// <summary>
        /// XTo res 1600
        /// </summary>
        /// <param name="xIn1600px">X in 1600px</param>
        /// <returns>Int</returns>
        public static int XToRes1600(int xIn1600px)
        {
            return (int)Math.Round(xIn1600px * BaseGame.Width / 1600.0f);
        }

        /// <summary>
        /// YTo res 1200
        /// </summary>
        /// <param name="yIn768px">Y in 1200px</param>
        /// <returns>Int</returns>
        public static int YToRes1200(int yIn1200px)
        {
            return (int)Math.Round(yIn1200px * BaseGame.Height / 1200.0f);
        }

        /// <summary>
        /// XTo res 1400
        /// </summary>
        /// <param name="xIn1400px">X in 1400px</param>
        /// <returns>Int</returns>
        public static int XToRes1400(int xIn1400px)
        {
            return (int)Math.Round(xIn1400px * BaseGame.Width / 1400.0f);
        }

        /// <summary>
        /// YTo res 1200
        /// </summary>
        /// <param name="yIn1050px">Y in 1050px</param>
        /// <returns>Int</returns>
        public static int YToRes1050(int yIn1050px)
        {
            return (int)Math.Round(yIn1050px * BaseGame.Height / 1050.0f);
        }

        /// <summary>
        /// Calc rectangle, helper method to convert from our images (1024)
        /// to the current resolution. Everything will stay in the 16/9
        /// format of the textures.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor = height / 640.0f;
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle with bounce effect, same as CalcRectangle, but sizes
        /// the resulting rect up and down depending on the bounceEffect value.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <param name="bounceEffect">Bounce effect</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleWithBounce(
            int relX, int relY, int relWidth, int relHeight, float bounceEffect)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor = height / 640.0f;
            float middleX = (relX + relWidth / 2) * widthFactor;
            float middleY = (relY + relHeight / 2) * heightFactor;
            float retWidth = relWidth * widthFactor * bounceEffect;
            float retHeight = relHeight * heightFactor * bounceEffect;
            return new Rectangle(
                (int)Math.Round(middleX - retWidth / 2),
                (int)Math.Round(middleY - retHeight / 2),
                (int)Math.Round(retWidth),
                (int)Math.Round(retHeight));
        }

        /// <summary>
        /// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
        /// ratio for the image. The Rect will take same screen space in
        /// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
        /// Should be used for 1024px width graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor = height / 768.0f;
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
        /// ratio for the image. The Rect will take same screen space in
        /// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
        /// Should be used for 1024px width graphics.
        /// </summary>
        /// <param name="gfxRect">Gfx rectangle</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3(
            Rectangle gfxRect)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor = height / 768.0f;
            return new Rectangle(
                (int)Math.Round(gfxRect.X * widthFactor),
                (int)Math.Round(gfxRect.Y * heightFactor),
                (int)Math.Round(gfxRect.Width * widthFactor),
                (int)Math.Round(gfxRect.Height * heightFactor));
        }

        /// <summary>
        /// Calc rectangle for 1600px width graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle1600(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 1600.0f;

            float heightFactor = (height / 1200.0f);// / (aspectRatio / (16 / 9));
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle 2000px, just a helper to scale stuff down
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle2000(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 2000.0f;
            float heightFactor = (height / 1500.0f);
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle keep 4 to 3 align bottom
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3AlignBottom(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor16To9 = height / 640.0f;
            float heightFactor4To3 = height / 768.0f;
            return new Rectangle(
                (int)(relX * widthFactor),
                (int)(relY * heightFactor16To9) -
                (int)Math.Round(relHeight * heightFactor4To3),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor4To3));
        }

        /// <summary>
        /// Calc rectangle keep 4 to 3 align bottom right
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3AlignBottomRight(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor16To9 = height / 640.0f;
            float heightFactor4To3 = height / 768.0f;
            return new Rectangle(
                (int)(relX * widthFactor) -
                (int)Math.Round(relWidth * widthFactor),
                (int)(relY * heightFactor16To9) -
                (int)Math.Round(relHeight * heightFactor4To3),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor4To3));
        }

        /// <summary>
        /// Calc rectangle centered with given height.
        /// This one uses relX and relY points as the center for our rect.
        /// The relHeight is then calculated and we align everything
        /// with help of gfxRect (determinating the width).
        /// Very useful for buttons, logos and other centered UI textures.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relHeight">Rel height</param>
        /// <param name="gfxRect">Gfx rectangle</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleCenteredWithGivenHeight(
            int relX, int relY, int relHeight, Rectangle gfxRect)
        {
            float widthFactor = width / 1024.0f;
            float heightFactor = height / 640.0f;
            int rectHeight = (int)Math.Round(relHeight * heightFactor);
            // Keep aspect ratio
            int rectWidth = (int)Math.Round(
                gfxRect.Width * rectHeight / (float)gfxRect.Height);
            return new Rectangle(
                Math.Max(0, (int)Math.Round(relX * widthFactor) - rectWidth / 2),
                Math.Max(0, (int)Math.Round(relY * heightFactor) - rectHeight / 2),
                rectWidth, rectHeight);
        }
        #endregion

        #region Frames per second
        /// <summary>
        /// Fps
        /// </summary>
        /// <returns>Int</returns>
        public static int Fps
        {
            get
            {
                return fpsLastSecond;
            }
        }

        /// <summary>
        /// Interpolated fps over the last 10 seconds.
        /// Obviously goes down if our framerate is low.
        /// </summary>
        private static float fpsInterpolated = 100.0f;

        /// <summary>
        /// Total frames
        /// </summary>
        /// <returns>Int</returns>
        public static int TotalFrames
        {
            get
            {
                return totalFrameCount;
            }
        }
        #endregion

        #region Timing stuff
        /// <summary>
        /// Elapsed time this frame in ms
        /// </summary>
        /// <returns>Int</returns>
        public static float ElapsedTimeThisFrameInMilliseconds
        {
            get
            {
                return elapsedTimeThisFrameInMs;
            }
        }

        /// <summary>
        /// Total time in seconds
        /// </summary>
        /// <returns>Int</returns>
        public static float TotalTime
        {
            get
            {
                return totalTimeMs / 1000.0f;
            }
        }

        /// <summary>
        /// Total time ms
        /// </summary>
        /// <returns>Float</returns>
        public static float TotalTimeMilliseconds
        {
            get
            {
                return totalTimeMs;
            }
        }

        /// <summary>
        /// Move factor per second, when we got 1 fps, this will be 1.0f,
        /// when we got 100 fps, this will be 0.01f.
        /// </summary>
        public static float MoveFactorPerSecond
        {
            get
            {
                return elapsedTimeThisFrameInMs / 1000.0f;
            }
        }
        #endregion

        #region Camera
        /// <summary>
        /// World matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
                // Update worldViewProj here?
            }
        }

        /// <summary>
        /// View matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ViewMatrix
        {
            get
            {
                return viewMatrix;
            }
            set
            {
                // Set view matrix, usually only done in ChaseCamera.Update!
                viewMatrix = value;

                // Update camera pos and rotation, used all over the game!
                invViewMatrix = Matrix.Invert(viewMatrix);
                camPos = invViewMatrix.Translation;
                cameraRotation = Vector3.TransformNormal(
                    new Vector3(0, 0, 1), invViewMatrix);
            }
        }

        /// <summary>
        /// Projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
            set
            {
                projectionMatrix = value;
                // Update worldViewProj here?
            }
        }

        /// <summary>
        /// Camera pos, updated each frame in ViewMatrix!
        /// Public to allow easy access from everywhere, will be called a lot each
        /// frame, for example Model.Render uses this for distance checks.
        /// </summary>
        private static Vector3 camPos;

        /// <summary>
        /// Get camera position from inverse view matrix. Similar to method
        /// used in shader. Works only if ViewMatrix is correctly set.
        /// </summary>
        /// <returns>Vector 3</returns>
        public static Vector3 CameraPos
        {
            get
            {
                return camPos;
            }
        }

        /// <summary>
        /// Camera rotation, used to compare objects for visibility.
        /// </summary>
        private static Vector3 cameraRotation = new Vector3(0, 0, 1);

        /// <summary>
        /// Camera rotation
        /// </summary>
        /// <returns>Vector 3</returns>
        public static Vector3 CameraRotation
        {
            get
            {
                return cameraRotation;
            }
        }

        /// <summary>
        /// Remember inverse view matrix.
        /// </summary>
        private static Matrix invViewMatrix;

        /// <summary>
        /// Inverse view matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix InverseViewMatrix
        {
            get
            {
                return invViewMatrix;//Matrix.Invert(ViewMatrix);
            }
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ViewProjectionMatrix
        {
            get
            {
                return ViewMatrix * ProjectionMatrix;
            }
        }

        /// <summary>
        /// World view projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix WorldViewProjectionMatrix
        {
            get
            {
                return WorldMatrix * ViewMatrix * ProjectionMatrix;
            }
        }
        #endregion

        #region Tiles

        public static float TileHeight
        {
            get
            {
                return tileHeight;
            }
        }

        public static float TileWidth
        {
            get
            {
                return tileWidth;
            }
        }

        public static float TileXOffset
        {
            get
            {
                return tileXOffset;
            }
        }

        public static float TileYOffset
        {
            get
            {
                return tileYOffset;
            }
        }

        public static float TileSide
        {
            get
            {
                return tileSide;
            }
        }

        public static Vector2 TileScale
        {
            get
            {
                return tileScale;
            }
        }

        public static float MouseCameraMoveRate
        {
            get
            {
                return mouseCameraMoveRate;
            }
        }

        public static float CameraMoveRate
        {
            get
            {
                return cameraMoveRate;
            }
        }

        public static Vector2 UnitOffset
        {
            get
            {
                return unitOffset;
            }
        }
        public static float ZoomValue
        {
            get
            {
                return zoomValue;
            }
        }

        #endregion

        #region Render states
        /// <summary>
        /// Alpha blending
        /// </summary>
        public static void SetAlphaBlendingEnabled(bool value)
        {
            if (value)
            {
                Device.BlendState = BlendState.AlphaBlend;
                // TODO-MG: Device.RenderState.SourceBlend = Blend.SourceAlpha;
                // TODO-MG: Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            }
            else
            {
                Device.BlendState = BlendState.Opaque;
            }
        }

        /// <summary>
        /// Alpha modes
        /// </summary>
        public enum AlphaMode
        {
            /// <summary>
            /// Disable alpha blending for this (even if the texture has alpha)
            /// </summary>
            DisableAlpha,
            /// <summary>
            /// Default alpha mode: SourceAlpha and InvSourceAlpha, which does
            /// nothing if the texture does not have alpha, else it just displays
            /// it as it is (with transparent pixels).
            /// </summary>
            Default,
            /// <summary>
            /// Use source alpha one mode, this is the default mode for lighting
            /// effects.
            /// </summary>
            SourceAlphaOne,
            /// <summary>
            /// One one alpha mode.
            /// </summary>
            OneOne,
        }

        /// <summary>
        /// Current alpha mode
        /// </summary>
        public static void SetCurrentAlphaMode(AlphaMode value)
        {
            switch (value)
            {
                case AlphaMode.DisableAlpha:
                    // TODO-MG: Device.RenderState.SourceBlend = Blend.Zero;
                    // TODO-MG: Device.RenderState.DestinationBlend = Blend.One;
                    break;
                case AlphaMode.Default:
                    // TODO-MG: Device.RenderState.SourceBlend = Blend.SourceAlpha;
                    // TODO-MG: Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    break;
                case AlphaMode.SourceAlphaOne:
                    // TODO-MG: Device.RenderState.SourceBlend = Blend.SourceAlpha;
                    // TODO-MG: Device.RenderState.DestinationBlend = Blend.One;
                    break;
                case AlphaMode.OneOne:
                    // TODO-MG: Device.RenderState.SourceBlend = Blend.One;
                    // TODO-MG: Device.RenderState.DestinationBlend = Blend.One;
                    break;
            }
        }
        #endregion
        #endregion

        #region Constructor
        /// <summary>
        /// Create base game
        /// </summary>
        /// <param name="setWindowsTitle">Set windows title</param>
        protected BaseGame(string setWindowsTitle)
        {
            // Set graphics
            graphicsManager = new GraphicsDeviceManager(this);

            // Set minimum requirements
            graphicsManager.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(
                    graphics_PrepareDevice);
            GameSettings.Initialize();
            ApplyResolutionChange();

//#if DEBUG
            // Disable vertical retrace to get highest framerates possible for
            // testing performance.
            graphicsManager.SynchronizeWithVerticalRetrace = false;
//#endif
            // Update as fast as possible, do not use fixed time steps.
            // The whole game is designed this way, if you remove this line
            // the car will not behave normal anymore!
            this.IsFixedTimeStep = false;

            // Init content manager
            content = new ContentManager(this.Services);
            content.RootDirectory = "Content";

            // Update windows title (used for unit testing)
            this.Window.Title = setWindowsTitle;
            remWindowsTitle = setWindowsTitle;

            // Replaces static Constructors with simple inits.
            Sound.Initialize(Content);
            Log.Initialize();

        }

        /// <summary>
        /// Empty constructor for the designer support
        /// </summary>
        protected BaseGame()
            : this("Game")
        {
        }

        void graphics_PrepareDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                PresentationParameters presentParams =
                    e.GraphicsDeviceInformation.PresentationParameters;
                if (graphicsManager.PreferredBackBufferHeight == 720)
                {
    // TODO-MG: MultiSampleType removed; MSAA configured via PreferMultiSampling
#if !DEBUG
                    presentParams.PresentationInterval = PresentInterval.One;
#endif
                }
                else
                {
    // TODO-MG: MultiSampleType removed; MSAA configured via PreferMultiSampling
#if !DEBUG
                    presentParams.PresentationInterval = PresentInterval.Two;
#endif
                }
            }
        }

        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
#if !XBOX360
            // Add screenshot capturer. Note: Don't do this in constructor,
            // we need the correct window name for screenshots!
            this.Components.Add(new ScreenshotCapturer(this));
#endif

            base.Initialize();

            // Set depth format
            backBufferDepthFormat = graphicsManager.PreferredDepthStencilFormat;

            // Update resolution if it changes
            graphicsManager.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
            graphics_DeviceReset(null, EventArgs.Empty);

            // Init global manager classes, which will be used all over the place ^^
            lineManager2D = new LineManager2D();
            ui = new UIRenderer();

            graphicsManager.GraphicsDevice.Reset();
        }

        /// <summary>
        /// Graphics device reset
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">E</param>
        void graphics_DeviceReset(object sender, EventArgs e)
        {
            // Update width and height
            width = Device.Viewport.Width;//Window.ClientBounds.Width;
            height = Device.Viewport.Height;//Window.ClientBounds.Height;
            aspectRatio = (float)width / (float)height;

            // Re-Set device
            // Restore z buffer state
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;
            // MG: DepthBufferWriteEnable handled via DepthStencilState.Default
            // Set u/v addressing back to wrap (must assign a new SamplerState — default is immutable)
            BaseGame.Device.SamplerStates[0] = new SamplerState
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
            };
            // Restore normal alpha blending
            BaseGame.SetCurrentAlphaMode(BaseGame.AlphaMode.Default);

            // Set 128 and greate alpha compare for Model.Render
            // TODO-MG: ReferenceAlpha removed; alpha testing done in shader
            // TODO-MG: AlphaFunction removed; alpha testing done in shader

            // Set hexagon dimensions, set to 148 x 128 at 1600 x 1000 or 1600 x 1200
            tileWidth = XToRes1600(148);
            if (aspectRatio > 1.4)
                tileHeight = YToRes1050(128);
            else
                tileHeight = YToRes1200(128);
            
            float radius = (float)Math.Round(((tileWidth * Math.Cos(MathHelper.ToRadians(30))) / 2));
            tileSide = (float)Math.Round((Math.Tan(MathHelper.ToRadians(30)) * 2.0f * radius)); //74;
            tileXOffset = (tileWidth - tileSide) / 2 + tileSide;//111;
            tileYOffset = tileHeight / 2;
           
            cameraMoveRate = CameraRate * tileHeight;
            //Should be set to 15 at about 60 fps
            mouseCameraMoveRate = MouseCameraRate;// *tileHeight;

            tileScale = new Vector2(tileWidth + tileSide, tileYOffset);

            unitOffset = new Vector2(tileWidth / 2, tileHeight / 2);

            if (HeavyGearManager.Map != null)
                HeavyGearManager.Map.ResetScreenPositions();

        }
        #endregion

        #region Line helper methods
        /// <summary>
        /// Draw line
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">End point</param>
        /// <param name="color">Color</param>
        public static void DrawLine(Point startPoint, Point endPoint, Color color)
        {
            lineManager2D.AddLine(startPoint, endPoint, color);
        }

        /// <summary>
        /// Draw line
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">End point</param>
        public static void DrawLine(Point startPoint, Point endPoint)
        {
            lineManager2D.AddLine(startPoint, endPoint, Color.White);
        }

        /// <summary>
        /// Flush line manager 2D. Renders all lines and allows more lines
        /// to be rendered. Used to render lines into textures and stuff.
        /// </summary>
        public static void FlushLineManager2D()
        {
            lineManager2D.Render();
        }

        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Update all input states
            Input.Update();

            lastFrameTotalTimeMs = totalTimeMs;
            elapsedTimeThisFrameInMs =
                (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            totalTimeMs += elapsedTimeThisFrameInMs;

            // Make sure elapsedTimeThisFrameInMs is never 0
            if (elapsedTimeThisFrameInMs <= 0)
                elapsedTimeThisFrameInMs = 0.001f;

            // Increase frame counter for FramesPerSecond
            frameCountThisSecond++;
            totalFrameCount++;

            // One second elapsed?
            if (totalTimeMs - startTimeThisSecond > 1000.0f)
            {
                // Calc fps
                fpsLastSecond = (int)(frameCountThisSecond * 1000.0f /
                    (totalTimeMs - startTimeThisSecond));
                // Reset startSecondTick and repaintCountSecond
                startTimeThisSecond = totalTimeMs;
                frameCountThisSecond = 0;

                fpsInterpolated =
                    MathHelper.Lerp(fpsInterpolated, fpsLastSecond, 0.1f);

            }

        }
        #endregion

        #region On activated and on deactivated
        // Check if app is currently active
        static bool isAppActive = true;
        /// <summary>
        /// Is app active
        /// </summary>
        /// <returns>Bool</returns>
        public static bool IsAppActive
        {
            get
            {
                return isAppActive;
            }
        }

        /// <summary>
        /// On activated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            isAppActive = true;
        }

        /// <summary>
        /// On deactivated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            isAppActive = false;
        }
        #endregion

        #region Draw
#if !DEBUG
        int renderLoopErrorCount = 0;
#endif
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                // Clear anyway, makes unit tests easier and fixes problems if
                // we don't have the z buffer cleared (some issues with line
                // rendering might happen else). Performance drop is not significant!
                ClearBackground();

                // Get our sprites ready to draw...
                Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // Handle custom user render code
                Render();

                // Render UI and font texts, this also handles all collected
                // screen sprites (on top of 3d game code)
                UIRenderer.Render(lineManager2D);

                PostUIRender();

                ui.RenderTextsAndMouseCursor();
            }
            // Only catch exceptions here in release mode, when debugging
            // we want to see the source of the error. In release mode
            // we want to play and not be annoyed by some bugs ^^
#if !DEBUG
            catch (Exception ex)
            {
                Log.Write("Render loop error: " + ex.ToString());
                if (renderLoopErrorCount++ > 100)
                    throw;
            }
#endif
            finally
            {
                // Dummy block to prevent error in debug mode
            }

            base.Draw(gameTime);

            // Apply device changes
            if (mustApplyDeviceChanges)
            {
                graphicsManager.ApplyChanges();
                mustApplyDeviceChanges = false;
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Render delegate for rendering methods, also used for many other
        /// methods.
        /// </summary>
        public delegate void RenderHandler();

        /// <summary>
        /// Render
        /// </summary>
        protected virtual void Render()
        {

        }

        /// <summary>
        /// Post user interface rendering, in case we need it.
        /// Used for rendering the car selection 3d stuff after the UI.
        /// </summary>
        protected virtual void PostUIRender()
        {
            // Overwrite this for your custom render code ..
        }

        /// <summary>
        /// Clear background
        /// </summary>
        public static void ClearBackground()
        {
            //unsure if it clears depth correctly: Device.Clear(BackgroundColor);
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                BackgroundColor, 1.0f, 0);
        }
        #endregion

        #region Set and reset render targets
        /// <summary>
        /// Remember scene render target. This is very important because
        /// for our post screen shaders we have to render our whole scene
        /// to this render target. But in the process we will use many other
        /// shaders and they might set their own render targets and then
        /// reset it, but we need to have this scene still to be set.
        /// Don't reset to the back buffer (with SetRenderTarget(0, null), this
        /// would stop rendering to our scene render target and the post screen
        /// shader will not be able to process our screen.
        /// The whole reason for this is that we can't use StrechRectangle
        /// like in Rocket Commander because XNA does not provide that function
        /// (the reason for that is cross platform compatibility with the XBox360).
        /// Instead we could use ResolveBackBuffer, but that method is VERY SLOW.
        /// Our framerate would drop from 600 fps down to 20, not good.
        /// However, multisampling will not work, so we will disable it anyway!
        /// </summary>
        static RenderTarget2D remSceneRenderTarget = null;
        /// <summary>
        /// Remember the last render target we set, this way we can check
        /// if the rendertarget was set before calling resolve!
        /// </summary>
        static RenderTarget2D lastSetRenderTarget = null;

        /// <summary>
        /// Remember render to texture instances to allow recreating them all
        /// when DeviceReset is called.
        /// </summary>
        static List<RenderToTexture> remRenderToTextures =
            new List<RenderToTexture>();

        /// <summary>
        /// Add render to texture instance to allow recreating them all
        /// when DeviceReset is called with help of the remRenderToTextures list. 
        /// </summary>
        public static void AddRemRenderToTexture(RenderToTexture renderToTexture)
        {
            remRenderToTextures.Add(renderToTexture);
        }

        /// <summary>
        /// Current render target we have set, null if it is just the back buffer.
        /// </summary>
        public static RenderTarget2D CurrentRenderTarget
        {
            get
            {
                return lastSetRenderTarget;
            }
        }

        /// <summary>
        /// Set render target
        /// </summary>
        /// <param name="isSceneRenderTarget">Is scene render target</param>
        internal static void SetRenderTarget(RenderTarget2D renderTarget,
            bool isSceneRenderTarget)
        {
            Device.SetRenderTarget(renderTarget);
            if (isSceneRenderTarget)
                remSceneRenderTarget = renderTarget;
            lastSetRenderTarget = renderTarget;
        }

        /// <summary>
        /// Reset render target
        /// </summary>
        /// <param name="fullResetToBackBuffer">Full reset to back buffer</param>
        internal static void ResetRenderTarget(bool fullResetToBackBuffer)
        {
            if (remSceneRenderTarget == null ||
                fullResetToBackBuffer)
            {
                remSceneRenderTarget = null;
                lastSetRenderTarget = null;
                Device.SetRenderTarget(null);
            }
            else
            {
                Device.SetRenderTarget(remSceneRenderTarget);
                lastSetRenderTarget = remSceneRenderTarget;
            }
        }
        #endregion

        #region PixelToMap Conversion
        public static void ConvertPixelToMap(ref Vector2 pixelPosition, out Point mapPosition)
        {
            int xPos, yPos;

            float zoomValue = 1.0f;
            if (HeavyGearManager.Map != null)
                zoomValue = HeavyGearManager.Map.ZoomValue;

            yPos = (int)(pixelPosition.Y / (TileScale.Y / zoomValue));

            if ((int)(pixelPosition.X / (TileXOffset / zoomValue)) % 2 == 0)
            {
                //aprrox. even column
                if (pixelPosition.Y % (TileHeight / zoomValue) >= TileScale.Y / zoomValue)
                {
                    //bottom part of the tile, so subtract one
                    yPos--;
                }
            }
            else
            {
                if ((pixelPosition.Y - (TileScale.Y / zoomValue)) % (TileHeight / zoomValue) >= TileScale.Y / zoomValue)
                {
                    //bottom part of the tile, so subtract one
                    yPos--;
                }
            }

            
            xPos = (int)(pixelPosition.X / (TileScale.X / zoomValue));

            //constrain to the limits of the grid
            if (yPos < 0)
                yPos = 0;

            if (xPos < 0)
                xPos = 0;

            if (yPos > (HeavyGearManager.Map.Height - 1))
                yPos = HeavyGearManager.Map.Height - 1;

            if (xPos > (HeavyGearManager.Map.Width - 1))
                xPos = HeavyGearManager.Map.Width - 1;

            mapPosition = new Point(xPos, yPos);
        }
        #endregion

        #region MapToPixel Conversion
        public static void ConvertMapToPixel(int x, int y, out Vector2 pixelPosition)
        {
            pixelPosition = new Vector2(x, y);

            Vector2.Multiply(ref pixelPosition, ref tileScale, out pixelPosition);
            Vector2.Divide(ref pixelPosition, zoomValue, out pixelPosition);
            if (y % 2 > 0)
            {
                pixelPosition.X += TileXOffset / zoomValue;
            }
        }

        public static void ConvertMapToPixelStandardized(ref Point mapPosition, out Vector2 pixelPosition)
        {
            float xPos, yPos;

            xPos = mapPosition.X * 222;
            if (mapPosition.Y % 2 > 0)
            {
                xPos += 111;
            }

            yPos = mapPosition.Y * 64;

            pixelPosition = new Vector2(xPos, yPos);
        }

        public static void ConvertMapToPixel(int x, int y, int xOffset, ref Point scale, out Point pixelPosition)
        {
            int xPos, yPos;

            xPos = x * scale.X;
            if (y % 2 > 0)
            {
                 xPos += xOffset;
            }

            yPos = y * scale.Y;

            pixelPosition = new Point(xPos, yPos);
        }
        public static void ConvertMapToPixel(ref Point mapPosition, out Vector2 pixelPosition)
        {
            ConvertMapToPixel(mapPosition.X, mapPosition.Y, out pixelPosition);
        }
        #endregion

        #region Zoom
        public static void Zoom(float zoomValue)
        {
            zoomValue += zoomValue;
            if (zoomValue < 0.70)
                zoomValue = 0.70f;
            if (zoomValue > 1.75)
                zoomValue = 1.75f;

            HeavyGearManager.Map.VisibilityChanged = true;
            HeavyGearManager.Map.ResetScreenPositions();
        }
        public static void ZoomReset()
        {
            zoomValue = 1.0f;

            HeavyGearManager.Map.VisibilityChanged = true;
            HeavyGearManager.Map.ResetScreenPositions();
        }
        #endregion
    }
}
