#region File Description
//-----------------------------------------------------------------------------
// Sound.cs
//
// MonoGame port: replaced XACT (AudioEngine/WaveBank/SoundBank) with
// SoundEffect loaded via ContentManager.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using HeavyGear.Helpers;
using HeavyGear.Properties;
#endregion

namespace HeavyGear.Sounds
{
    /// <summary>
    /// Sound manager -- wraps MonoGame SoundEffect for the four menu sounds.
    /// Gear / crash / brake sounds from the original XNA template have been
    /// removed (they were unused in this tactics game).
    /// </summary>
    class Sound
    {
        #region Enums
        public enum Sounds
        {
            ButtonClick,
            ScreenClick,
            ScreenBack,
            Highlight,
        }
        #endregion

        #region Variables
        static SoundEffect buttonClick;
        static SoundEffect screenClick;
        static SoundEffect screenBack;
        static SoundEffect highlight;
        #endregion

        #region Constructor
        private Sound() { }
        #endregion

        #region Initialize
        /// <summary>Load sound effects. Call inside Game.LoadContent.</summary>
        public static void Initialize(ContentManager content)
        {
            try
            {
                buttonClick = content.Load<SoundEffect>("Audio/menu_buttonclick");
                screenClick = content.Load<SoundEffect>("Audio/menu_screenclick");
                screenBack  = content.Load<SoundEffect>("Audio/menu_screenback");
                highlight   = content.Load<SoundEffect>("Audio/menu_highlight");
                SetVolumes(GameSettings.Default.SoundVolume, GameSettings.Default.MusicVolume);
            }
            catch (Exception ex)
            {
                Log.Write("Sound.Initialize failed: " + ex.Message);
            }
        }
        #endregion

        #region Volume
        public static void SetVolumes(float sound, float music)
        {
            SoundEffect.MasterVolume = Math.Clamp(sound, 0f, 1f);
        }
        #endregion

        #region Play
        public static void Play(string soundName)
        {
            if (Enum.TryParse<Sounds>(soundName, out var s))
                Play(s);
        }

        public static void Play(Sounds sound)
        {
            try
            {
                SoundEffect sfx = sound switch
                {
                    Sounds.ButtonClick => buttonClick,
                    Sounds.ScreenClick => screenClick,
                    Sounds.ScreenBack  => screenBack,
                    Sounds.Highlight   => highlight,
                    _                  => null,
                };
                sfx?.Play();
            }
            catch (Exception ex)
            {
                Log.Write("Sound.Play failed: " + ex.Message);
            }
        }
        #endregion

        #region StopMusic
        /// <summary>No-op -- music not yet implemented.</summary>
        public static void StopMusic() { }
        #endregion
    }
}
