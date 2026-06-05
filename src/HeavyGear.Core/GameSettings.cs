#region File Description
//-----------------------------------------------------------------------------
// GameSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using HeavyGear.Helpers;
using System.Threading;
#endregion

namespace HeavyGear.Properties
{
    /// <summary>
    /// Game settings, stored in a custom xml file.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        #region Default
        /// <summary>
        /// Filename for our game settings file.
        /// </summary>
        const string SettingsFilename = "HeavyGearSettings.xml";

        /// <summary>
        /// Default instance for our game settings.
        /// </summary>
        private static GameSettings defaultInstance = null;

        /// <summary>
        /// Need saving the game settings file? Only set to true if
        /// we really changed some game setting here.
        /// </summary>
        private static bool needSave = false;

        /// <summary>
        /// Default
        /// </summary>
        /// <returns>Game settings</returns>
        public static GameSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create game settings, don't allow public constructor!
        /// </summary>
        private GameSettings()
        {
        }

        /// <summary>
        /// Create game settings. This constructor helps us to only load the
        /// GameSettings once, not again if GameSettings is recreated by
        /// the Deserialization process.
        /// </summary>
        /// <param name="loadSettings">Load settings</param>
        public static void Initialize()
        {
            defaultInstance = new GameSettings();
            Load();
        }
        #endregion

        #region Load
        /// <summary>
        /// Load
        /// </summary>
        public static void Load()
        {
            needSave = false;

            FileStream file = FileHelper.LoadGameContentFile(
                SettingsFilename);

            if (file == null)
            {
                // Create new file after quitting
                needSave = true;
                return;
            }

            // If the file is empty, just create a new file with the default
            // settings.
            if (file.Length == 0)
            {
                // Close the file
                file.Close();

                // But first check if there is maybe a file in the game directory
                // to load the default game settings from.
                file = FileHelper.LoadGameContentFile(SettingsFilename);
                if (file != null)
                {
                    // Load everything into this class
                    GameSettings loadedGameSettings =
                        (GameSettings)new XmlSerializer(typeof(GameSettings)).
                        Deserialize(file);
                    if (loadedGameSettings != null)
                        defaultInstance = loadedGameSettings;

                    // Close the file
                    file.Close();
                }

                // Save user settings file
                needSave = true;
                Save();
            }
            else
            {
                // Else load everything into this class with help of the
                // XmlSerializer.
                GameSettings loadedGameSettings =
                    (GameSettings)new XmlSerializer(typeof(GameSettings)).
                    Deserialize(file);
                if (loadedGameSettings != null)
                    defaultInstance = loadedGameSettings;

                // Close the file
                file.Close();
            }
        }
        #endregion

        #region Save
        /// <summary>
        /// Save
        /// </summary>
        public static void Save()
        {
            // No need to save if everything is up to date.
            if (needSave == false)
                return;

            needSave = false;

            FileStream file = FileHelper.SaveGameContentFile(
                SettingsFilename);

            // Save everything in this class with help of the XmlSerializer.
            new XmlSerializer(typeof(GameSettings)).
                Serialize(file, defaultInstance);

            // Close the file
            file.Close();
        }

        /// <summary>
        /// Sets all of the graphical settings to their minimum possible
        /// values and saves the changes.
        /// </summary>
        public static void SetMinimumGraphics()
        {
            GameSettings.Default.ResolutionWidth = GameSettings.MinimumResolutionWidth;
            GameSettings.Default.ResolutionHeight = GameSettings.MinimumResolutionHeight;
            GameSettings.Default.ShadowMapping = false;
            GameSettings.Default.HighDetail = false;
            GameSettings.Default.PostScreenEffects = false;
            GameSettings.Save();
        }
        #endregion

        #region Setting variables with properties
        /// <summary>
        /// Highscores
        /// </summary>
        string highscores = "";
        /// <summary>
        /// Highscores
        /// </summary>
        /// <returns>String</returns>
        public string Highscores
        {
            get
            {
                return highscores;
            }
            set
            {
                if (highscores != value)
                    needSave = true;
                highscores = value;
            }
        }

        /// <summary>
        /// Player name
        /// </summary>
        string playerName = "Player";
        /// <summary>
        /// Player name
        /// </summary>
        /// <returns>String</returns>
        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                if (playerName != value)
                    needSave = true;
                playerName = value;
            }
        }

        public const int MinimumResolutionWidth = 640;

        /// <summary>
        /// Resolution width
        /// </summary>
        int resolutionWidth = 0;
        /// <summary>
        /// Resolution width
        /// </summary>
        /// <returns>Int</returns>
        public int ResolutionWidth
        {
            get
            {
                return resolutionWidth;
            }
            set
            {
                if (resolutionWidth != value)
                    needSave = true;
                resolutionWidth = value;
            }
        }

        public const int MinimumResolutionHeight = 480;

        /// <summary>
        /// Resolution height
        /// </summary>
        int resolutionHeight = 0;
        /// <summary>
        /// Resolution height
        /// </summary>
        /// <returns>Int</returns>
        public int ResolutionHeight
        {
            get
            {
                return resolutionHeight;
            }
            set
            {
                if (resolutionHeight != value)
                    needSave = true;
                resolutionHeight = value;
            }
        }

        /// <summary>
        /// Fullscreen
        /// </summary>
#if DEBUG
        bool fullscreen = false;  //Turn off fullscreen for debugging purposes
#else
        bool fullscreen = true;
#endif
        /// <summary>
        /// Fullscreen
        /// </summary>
        /// <returns>Bool</returns>
        public bool Fullscreen
        {
            get
            {
                return fullscreen;
            }
            set
            {
                if (fullscreen != value)
                    needSave = true;
                fullscreen = value;
            }
        }

        bool postScreenEffects = true;
        /// <summary>
        /// Post screen effects
        /// </summary>
        /// <returns>Bool</returns>
        public bool PostScreenEffects
        {
            get
            {
                return postScreenEffects;
            }
            set
            {
                if (postScreenEffects != value)
                    needSave = true;
                postScreenEffects = value;
            }
        }

        bool shadowMapping = true;
        /// <summary>
        /// ShadowMapping
        /// </summary>
        /// <returns>Bool</returns>
        public bool ShadowMapping
        {
            get
            {
                return shadowMapping;
            }
            set
            {
                if (shadowMapping != value)
                    needSave = true;
                shadowMapping = value;
            }
        }

        bool highDetail = true;
        /// <summary>
        /// HighDetail
        /// </summary>
        /// <returns>Bool</returns>
        public bool HighDetail
        {
            get
            {
                return highDetail;
            }
            set
            {
                if (highDetail != value)
                    needSave = true;
                highDetail = value;
            }
        }

        /// <summary>
        /// Sound volume
        /// </summary>
        float soundVolume = 0.8f;
        /// <summary>
        /// Sound volume
        /// </summary>
        /// <returns>Float</returns>
        public float SoundVolume
        {
            get
            {
                return soundVolume;
            }
            set
            {
                if (soundVolume != value)
                    needSave = true;
                soundVolume = value;
            }
        }

        /// <summary>
        /// Music volume
        /// </summary>
        float musicVolume = 0.6f;
        /// <summary>
        /// Music volume
        /// </summary>
        /// <returns>Float</returns>
        public float MusicVolume
        {
            get
            {
                return musicVolume;
            }
            set
            {
                if (musicVolume != value)
                    needSave = true;
                musicVolume = value;
            }
        }

        /// <summary>
        /// Controller sensitivity
        /// </summary>
        float controllerSensitivity = 0.5f;
        /// <summary>
        /// Controller sensitivity
        /// </summary>
        /// <returns>Float</returns>
        public float ControllerSensitivity
        {
            get
            {
                return controllerSensitivity;
            }
            set
            {
                if (controllerSensitivity != value)
                    needSave = true;
                controllerSensitivity = value;
            }
        }
        #endregion
    }
}
