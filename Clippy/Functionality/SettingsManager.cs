/// Clippy - File: "SettingsManager.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Clippy.Functionality
{
    public class SettingsManager
    {
        private const string s_settingsFileName = "ClippySettings.cls";
        private static SettingsManager s_instance;
        private List<Setting> m_settings;
        private string m_settingsFile;
        private int m_idCounter;
        private bool m_initialized = false;
        private string m_status;

        private SettingsManager() { }

        public static SettingsManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new SettingsManager();
                }

                return s_instance;
            }
        }

        /// <summary>
        /// The path and filename of the Settings file
        /// </summary>
        public string SettingsFile
        {
            get
            {
                if (string.IsNullOrEmpty(m_settingsFile))
                {
                    string executionPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileInfo fi = new FileInfo(executionPath);
                    m_settingsFile = Path.Combine(fi.DirectoryName, s_settingsFileName);
                }

                return m_settingsFile;
            }
        }

        /// <summary>
        /// The current status message
        /// </summary>
        public string Status
        {
            get { return m_status; }
        }

        /// <summary>
        /// The current number of settings available.
        /// </summary>
        public int SettingsCount
        {
            get
            {
                if (!CheckSettingsOk())
                {
                    return 0;
                }

                return m_settings.Count;
            }
         }


        /// <summary>
        /// Saves the current settings list to the settings file
        /// </summary>
        public bool SaveSettings()
        {
            string checkResultMsg;
            if (!CheckSettingsOk(out checkResultMsg))
            {
                m_status = $"Saving the settings failed: {checkResultMsg}";
                return false;
            }

            try
            {
                using (Stream saveStream = File.Open(SettingsFile, FileMode.Create))
                {
                    var bformatter = new BinaryFormatter();
                    saveStream.Position = 0;
                    bformatter.Serialize(saveStream, m_settings);
                }
            }
            catch (Exception ex)
            {
                m_status = $"Saving the settings failed: {ex.Message}";
                return false;
            }

            m_status = "Settings successfully saved.";
            return true;
        }

        /// <summary>
        /// Loads the settings from the settings file (if existing)
        /// </summary>
        public bool LoadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                m_status = "Settings not loaded. No settings file found";
                return false;
            }

            List<Setting>loadedItems = null;
            try
            {
                using (Stream loadStream = File.Open(SettingsFile, FileMode.Open))
                {
                    var bformatter = new BinaryFormatter();
                    loadStream.Position = 0;
                    loadedItems = (List<Setting>)bformatter.Deserialize(loadStream);
                }
            }
            catch (Exception ex)
            {
                m_status = $"Loading Settings failed: {ex.Message}";
                return false;
            }

            if (loadedItems == null || loadedItems.Count == 0)
            {
                m_status = "Loading settings failed. Settings file seems to be empty or corrupt";
                return false;
            }

            if (m_settings != null)
            {
                m_settings.Clear();
            }

            m_settings = loadedItems;

            if (m_settings == null || m_settings.Count == 0)
            {
                m_idCounter = -1;
            }
            else
            {
                m_idCounter = m_settings.Max(i => i.Id);
            }

            m_initialized = true;
            m_status = "Settings loaded successfully.";
            return true;
        }

        /// <summary>
        /// Initializes the settings for the first use
        /// If a settings file exists it will be loaded. If not a new one will be created at the next save
        /// </summary>
        /// <param name="forceNew">Throws away existing settings and skips loading</param>
        public void Initialize(bool forceNew = false)
        {
            if (!forceNew)
            {
                if (m_initialized)
                {
                    return;
                }

                // Try to load an existing settings file
                if (LoadSettings())
                {
                    return;
                }
            }

            // Create a new list of settings
            m_settings = new List<Setting>();
            m_idCounter = 0;
            m_initialized = true;
            m_status = "Settings initialized successfully.";
        }

        /// <summary>
        /// Adds a new setting to the list
        /// </summary>
        ///<returns>The id of the new setting when added successfully. Otherwise -1</returns>
        public int AddSetting(string name, object value, bool isActive = true)
        {
            if (!CheckSettingsOk())
            {
                return -1;
            }

            m_idCounter++;
            Setting newSetting = new Setting(m_idCounter, name, value, isActive);
            m_settings.Add(newSetting);

            m_status = $"Successfully added the new setting \"{newSetting.Id}: {newSetting.Name}\"";
            return newSetting.Id;
        }

        /// <summary>
        /// Updates the setting with the given name.
        /// If more settings with the same name exist, it will update the first instance.
        /// </summary>
        /// <param name="name">Name of the setting to update</param>
        /// <param name="newValue">The new value of the setting</param>
        /// <param name="isActive">Flag if the setting is active or not</param>
        /// <returns>TRUE if the setting was updated successfully. Otherwise FALSE.</returns>
        public bool UpdateSetting(string name, object newValue, bool isActive = true)
        {
            if (!CheckSettingsOk())
            {
                return false;
            }

            Setting settingToUpdate = GetSetting(name);
            if (settingToUpdate == null)
            {
                return false;
            }

            settingToUpdate.Value = newValue;
            settingToUpdate.IsActive = isActive;

            m_status = $"Successfully updated the setting \"{settingToUpdate.Id}: {settingToUpdate.Name}\"";
            return true;
        }

        /// <summary>
        /// Updates the setting with the given id.
        /// </summary>
        /// <param name="id">Id of the setting to update</param>
        /// <param name="newValue">The new value of the setting</param>
        /// <param name="isActive">Flag if the setting is active or not</param>
        /// <returns>TRUE if the setting was updated successfully. Otherwise FALSE.</returns>
        public bool UpdateSetting(int id, object newValue, bool isActive = true)
        {
            if (!CheckSettingsOk())
            {
                return false;
            }

            Setting settingToUpdate = GetSetting(id);
            if (settingToUpdate == null)
            {
                return false;
            }

            settingToUpdate.Value = newValue;
            settingToUpdate.IsActive = isActive;

            m_status = $"Successfully updated the setting \"{settingToUpdate.Id}: {settingToUpdate.Name}\"";
            return true;
        }

        /// <summary>
        /// Gets the value of the setting with the given name.
        /// If more settings with the same name exist, it will return the value of the first instance.
        /// </summary>
        /// <param name="settingName">The name of the setting</param>
        /// <returns>The value of the setting</returns>
        public object GetValue(string settingName)
        {
            bool isActive;
            return GetValue(settingName, out isActive);
        }

        /// <summary>
        /// Gets the value of the setting with the given name.
        /// If more settings with the same name exist, it will return the value of the first instance.
        /// </summary>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="isActive">Flag if the setting is active </param>
        /// <returns>The value of the setting</returns>
        public object GetValue(string settingName, out bool isActive)
        {
            isActive = false;
            if (!CheckSettingsOk())
            {
                return null;
            }

            Setting settingToReturn = GetSetting(settingName);
            if (settingToReturn == null)
            {
                m_status = $"Setting  with name\"{settingName}\" not found";
                return null;
            }

            isActive = settingToReturn.IsActive;

            m_status = $"Successfully returned value of the setting \"{settingToReturn.Id}: {settingToReturn.Name}\"";
            return settingToReturn.Value;
        }

        /// <summary>
        /// Gets the value of the setting with the given id.
        /// </summary>
        /// <param name="settingId">The id of the setting</param>
        /// <returns>The value of the setting</returns>
        public object GetValue(int settingId)
        {
            bool isActive;
            return GetValue(settingId, out isActive);
        }

        /// <summary>
        /// Gets the value of the setting with the given id.
        /// </summary>
        /// <param name="settingId">The id of the setting</param>
        /// <param name="isActive">Flag if the setting is active </param>
        /// <returns>The value of the setting</returns>
        public object GetValue(int settingId, out bool isActive)
        {
            isActive = false;
            if (!CheckSettingsOk())
            {
                return null;
            }

            Setting settingToReturn = GetSetting(settingId);
            if (settingToReturn == null)
            {
                m_status = $"Setting  with id\"{settingId}\" not found";
                return null;
            }

            isActive = settingToReturn.IsActive;

            m_status = $"Successfully returned value of the setting \"{settingToReturn.Id}: {settingToReturn.Name}\"";
            return settingToReturn.Value;
        }

        /// <summary>
        /// Gets a setting by its name.
        /// If more settings with the same name exist, it will return the first hit.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <returns>The setting if found. Otherwise NULL</returns>
        private Setting GetSetting(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                m_status = "Failed to get the setting. Name is NULL or empty";
                return null;
            }

            if (!CheckSettingsOk())
            {
                return null;
            }

            Setting foundSetting = m_settings.FirstOrDefault(s => s.Name == name);
            if (foundSetting == null)
            {
                m_status = $"Found no setting with name '{name}'";
            }

            m_status = $"Successfully found setting with name '{name}'";
            return foundSetting;
        }

        /// <summary>
        /// Gets a setting by its id.
        /// </summary>
        /// <param name="name">Id of the setting</param>
        /// <returns>The setting if found. Otherwise NULL</returns>
        private Setting GetSetting(int id)
        {
            if (id < 0)
            {
                m_status = "Failed to get the setting. Invalid setting id.";
                return null;
            }

            if (!CheckSettingsOk())
            {
                return null;
            }

            Setting foundSetting = m_settings.FirstOrDefault(s => s.Id == id);
            if (foundSetting == null)
            {
                m_status = $"Found no setting with name '{id}'";
            }

            m_status = $"Successfully found setting with name '{id}'";
            return foundSetting;
        }

        private bool CheckSettingsOk()
        {
            return CheckSettingsOk(out m_status);
        }

        /// <summary>
        /// Validates if the status of the settings is ok
        /// </summary>
        private bool CheckSettingsOk(out string resultMessage)
        {
            resultMessage = "Settings Ok.";
            if (!m_initialized || m_settings == null)
            {
                resultMessage = "Settings are not initialized";
                return false;
            }

            return true;
        }
    }
}
