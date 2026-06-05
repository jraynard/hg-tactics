#region File Description
//-----------------------------------------------------------------------------
// RenderToTexture.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Text;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using Texture = HeavyGear.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
using Microsoft.Xna.Framework;
using HeavyGear.GameLogic;
using Microsoft.Xna.Framework.Input;
#endregion

namespace HeavyGear.Shaders
{
    /// <summary>
    /// Render to texture helper class based on the Texture class.
    /// This class allows to render stuff onto textures, if thats not
    /// supported, it will just not work and report an engine log message.
    /// This class is required for most PostScreenShaders.
    /// </summary>
    public class RenderToTexture : Texture
    {
        #region Variables
        /// <summary>
        /// Our render target we are going to render to. Much easier than in MDX
        /// where you have to use Surfaces, etc. Also supports the Xbox360 model
        /// of resolving the render target texture before we can use it, otherwise
        /// the RenderToTexture class would not work on the Xbox360.
        /// </summary>
        RenderTarget2D renderTarget = null;

        // object /* DepthStencilBuffer removed */ was removed in XNA 4.0 / MonoGame: depth is embedded in RenderTarget2D.
        // The zBufferSurface field is kept as null to avoid breaking callers until fully refactored.
        private readonly object zBufferSurface = null;
        /// <summary>ZBuffer surface (legacy stub – always null in MonoGame)</summary>
        public object ZBufferSurface => zBufferSurface;

        /// <summary>
        /// Posible size types for creating a RenderToTexture object.
        /// </summary>
        public enum SizeType
        {
            /// <summary>
            /// Uses the full screen size for this texture
            /// </summary>
            FullScreen,
            /// <summary>
            /// Uses half the full screen size, e.g. 800x600 becomes 400x300
            /// </summary>
            HalfScreen,
            /// <summary>
            /// Uses a quarter of the full screen size, e.g. 800x600 becomes 200x150
            /// </summary>
            QuarterScreen,
            /// <summary>
            /// Shadow map texture, usually 1024x1024, but can also be better
            /// like 2048x2048 or 4096x4096.
            /// </summary>
            ShadowMap,
        }

        /// <summary>
        /// Size type
        /// </summary>
        private SizeType sizeType;

        /// <summary>
        /// Calc size
        /// </summary>
        private void CalcSize()
        {
            switch (sizeType)
            {
                case SizeType.FullScreen:
                    texWidth = BaseGame.Width;
                    texHeight = BaseGame.Height;
                    break;
                case SizeType.HalfScreen:
                    texWidth = BaseGame.Width / 2;
                    texHeight = BaseGame.Height / 2;
                    break;
                case SizeType.QuarterScreen:
                    texWidth = BaseGame.Width / 4;
                    texHeight = BaseGame.Height / 4;
                    break;
                case SizeType.ShadowMap:
                    // Use a larger texture for high detail
                    if (BaseGame.HighDetail)
                    {
                        texWidth = 2048;
                        texHeight = 2048;
                    }
                    else
                    {
                        texWidth = 1024;
                        texHeight = 1024;
                    }
                    break;
            }
            CalcHalfPixelSize();
        }

        /// <summary>
        /// Does this texture use some high percision format? Better than 8 bit color?
        /// </summary>
        private bool usesHighPercisionFormat = false;
        #endregion

        #region Properties
        /// <summary>
        /// Render target
        /// </summary>
        /// <returns>Render target 2D</returns>
        public RenderTarget2D RenderTarget
        {
            get
            {
                return renderTarget;
            }
        }

        /// <summary>
        /// Override how to get XnaTexture, we have to resolve the render target
        /// for supporting the Xbox, which requires calling Resolve first!
        /// After that you can call this property to get the current texture.
        /// </summary>
        /// <returns>XnaTexture</returns>
        public override XnaTexture XnaTexture
        {
            get
            {
                if (alreadyResolved)
                    internalXnaTexture = (Texture2D)renderTarget;
                return internalXnaTexture;
            }
        }

        /// <summary>
        /// Does this texture use some high percision format? Better than 8 bit color?
        /// </summary>
        public bool UsesHighPercisionFormat
        {
            get
            {
                return usesHighPercisionFormat;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Id for each created RenderToTexture for the generated filename.
        /// </summary>
        private static int RenderToTextureGlobalInstanceId = 0;
        /// <summary>
        /// Creates an offscreen texture with the specified size which
        /// can be used for render to texture.
        /// </summary>
        public RenderToTexture(SizeType setSizeType)
        {
            sizeType = setSizeType;
            CalcSize();

            texFilename = "RenderToTexture instance " +
                RenderToTextureGlobalInstanceId++;

            Create();

            BaseGame.AddRemRenderToTexture(this);
        }
        #endregion

        #region Handle device reset
        /// <summary>
        /// Handle the DeviceReset event, we have to re-create all our render targets.
        /// </summary>
        public void HandleDeviceReset()
        {
            // Respond to resolution changes
            CalcSize();
            // Clear resolved texture
            alreadyResolved = false;
            internalXnaTexture = null;
            // Re-create
            Create();
        }
        #endregion

        #region Create
        /// <summary>
        /// Check if we can use a specific surface format for render targets.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static bool CheckRenderTargetFormat(SurfaceFormat format)
        {
            // TODO-MG: Device.CreationParameters removed in MonoGame.
            // Assume all SurfaceFormats are supported; rely on runtime exception if not.
            return true;
        }

        /// <summary>
        /// Create
        /// </summary>
        private void Create()
        {
            SurfaceFormat format = SurfaceFormat.Color;
            // Try to use R32F format for shadow mapping if possible (ps20),
            // else just use A8R8G8B8 format for shadow mapping and
            // for normal RenderToTextures too.
            if (sizeType == SizeType.ShadowMap)
            {
                // Can do R32F format?
                if (CheckRenderTargetFormat(SurfaceFormat.Single))
                    format = SurfaceFormat.Single;
                // Else try R16F format, thats still much better than A8R8G8B8
                else if (CheckRenderTargetFormat(SurfaceFormat.HalfSingle))
                    format = SurfaceFormat.HalfSingle;
                // And check a couple more formats (mainly for the Xbox360 support)
                else if (CheckRenderTargetFormat(SurfaceFormat.HalfVector2))
                    format = SurfaceFormat.HalfVector2;
                else if (CheckRenderTargetFormat(SurfaceFormat.Single /* was Luminance16 */))
                    format = SurfaceFormat.Single /* was Luminance16 */;
                // Else nothing found, well, then just use the 8 bit Color format.
            }
            // MSAA removed (MonoGame configures MSAA via PreferMultiSampling)
            // Create render target of specified size.
            renderTarget = new RenderTarget2D(BaseGame.Device, texWidth, texHeight);

            if (format != SurfaceFormat.Color)
                usesHighPercisionFormat = true;

            loaded = true;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Clear render target (call SetRenderTarget first)
        /// </summary>
        public void Clear(Color clearColor)
        {
            if (loaded == false ||
                renderTarget == null)
                return;

            BaseGame.Device.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer,
                clearColor, 1.0f, 0);
        }
        #endregion

        #region Set render target
        /// <summary>
        /// Set render target to this texture to render stuff on it.
        /// </summary>
        public bool SetRenderTarget()
        {
            if (loaded == false ||
                renderTarget == null)
                return false;

            BaseGame.SetRenderTarget(renderTarget, false);
            return true;
        }


        #endregion

        #region Resolve
        /// <summary>
        /// Make sure we don't call XnaTexture before resolving for the first time!
        /// </summary>
        bool alreadyResolved = false;
        /// <summary>
        /// Resolve render target. For windows developers this method may seem
        /// strange, why not just use the rendertarget's texture? Well, this is
        /// just for the Xbox360 support. The Xbox requires that you call Resolve
        /// first before using the rendertarget texture. The reason for that is
        /// copying the data over from the EPRAM to the video memory, for more
        /// details read the XNA docs.
        /// Note: This method will only work if the render target was set before
        /// with SetRenderTarget, else an exception will be thrown to ensure
        /// correct calling order.
        /// </summary>
        public void Resolve()
        {
            // Make sure this render target is currently set!
            if (BaseGame.CurrentRenderTarget != renderTarget)
                throw new InvalidOperationException(
                    "You can't call Resolve without first setting the render target!");

            alreadyResolved = true;
            // Obsolete - Device.ResolveRenderTarget(0) removed in MonoGame
        }
        #endregion
    }
}
