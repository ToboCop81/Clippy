using Clippy.Common;
using Clippy.Interfaces;
using Clippy.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Clippy.Functionality
{
    /// <summary>
    /// Manager class for clipboard data objects
    /// </summary>
    public class ClipDataManager
    {
        private const string s_autosaveFileName = "ClippyAutoSave";
        private const string s_fileExtension = "clp";

        private static ClipDataManager s_instance;

        private ObservableCollection<IClipboardItem> m_items;
        private long m_idCounter;
        private string m_autoSaveFile;
        private string m_status;

        private ClipDataManager() { }

        public static ClipDataManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ClipDataManager();
                }

                return s_instance;
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
        /// Occurs when the items list has changed
        /// </summary>
        public event ItemsChangedEventHandler ItemsChanged;

        public ObservableCollection<IClipboardItem> Items
        {
            get
            {
                InitializeItems();
                return m_items;
            }
        }

        /// <summary>
        /// The path and filename of the autoSave file
        /// </summary>
        public string AutoSaveFile
        {
            get
            {
                if (string.IsNullOrEmpty(m_autoSaveFile))
                {
                    string executionPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileInfo fi = new FileInfo(executionPath);
                    m_autoSaveFile = Path.Combine(fi.DirectoryName, $"{s_autosaveFileName}.{s_fileExtension}");
                }

                return m_autoSaveFile;
            }
        }

        /// <summary>
        /// The default file extension for clipboard list files
        /// </summary>
        public string FileExtension
        {
            get { return s_fileExtension; }
        }

        /// <summary>
        /// Gets data from clipboard and adds it to the items list
        /// </summary>
        /// <returns>
        /// TRUE when data was added
        /// FALSE when clipboard was empty or did not contain supported data
        /// </returns>
        public bool GetDataFromClipboard()
        {
            DataKind type;
            if (!CheckForClipboardData(out type))
            {
                return false;
            }

            InitializeItems();
            switch (type)
            {
                case DataKind.PlainText:
                    m_idCounter++;
                    PlainTextItem newPlainText = new PlainTextItem(m_idCounter, Clipboard.GetText());
                    if (ClippySettings.Instance.TextItemNameFromContent)
                    {
                        newPlainText.Title = GetTextTeaser(newPlainText.GetText(), 25);
                    }

                    m_items.Add(newPlainText);
                    ItemsChanged(ItemsChangeType.ItemAdded, new ItemsChangedEventArgs(newPlainText));
                    break;

                case DataKind.Image:
                    m_idCounter++;
                    ImageItem newImage = new ImageItem(m_idCounter, ClipboardImageHelper.ImageFromClipboardDib() as BitmapSource);
                    ItemsChanged(ItemsChangeType.ItemAdded, new ItemsChangedEventArgs(newImage));
                    m_items.Add(newImage);
                    break;

                default:
                    return false;
            }

            m_status = "Added item from clipboard";
            return true;
        }

        /// <summary>
        /// Removes the item with the given index from the list
        /// </summary>
        public bool RemoveItem(long index)
        {
            if (m_items == null || m_items.Count == 0)
            {
                m_status = $"Removed no item with index '{index}'. List is empty";
                return false;
            }

            IClipboardItem matchingItem = m_items.FirstOrDefault(i => i.Index == index);
            if (matchingItem == null)
            {
                m_status = $"No item with index '{index}' found to remove";
                return false;
            }

            ItemsChangedEventArgs args = new ItemsChangedEventArgs(matchingItem);
            m_items.Remove(matchingItem);
            ItemsChanged(ItemsChangeType.ItemRemoved, args);
            m_status = $"Removed item with index '{index}'";
            return true;
        }

        /// <summary>
        /// Removes all items from the list and resets the index counter
        /// </summary>
        public void ClearList( bool clearAutoSaveFile)
        {
            if (m_items != null)
            {
                m_items.Clear();
                ItemsChanged(ItemsChangeType.ListCleared, null);
                m_status = "Cleared the list";
            }

            if (clearAutoSaveFile && File.Exists(AutoSaveFile))
            {
                try
                {
                    File.Delete(AutoSaveFile);
                    m_status = "Cleared the list and successfully deleted the autosave file";
                }
                catch (Exception ex)
                {
                    m_status = $"Failed to delete the autosave file: {ex.Message}";
                }
            }
            
            InitializeItems();
        }

        public void RemoveSelectedItems()
        {
            List<long> removeIds = new List<long>();
            foreach (IClipboardItem currentItem in m_items)
            {
                if (currentItem.Selected)
                {
                    removeIds.Add(currentItem.Index);
                }
            }

            if (removeIds.Count == 0)
            {
                return;
            }

            foreach (long index in removeIds)
            {
                RemoveItem(index);
            }

            m_status = $"Removed {removeIds.Count} items from list";
        }

        /// <summary>
        /// Gets the highest Index contained in clipboardItems list
        /// </summary>
        /// <returns></returns>
        public long GetMaxIndex()
        {
            if (m_items == null || m_items.Count == 0)
            {
                return -1;
            }

            return m_items.Max(i => i.Index);
        }

        /// <summary>
        /// Saves the current items list to the autoSave file
        /// </summary>
        public bool AutoSave()
        {
            return SaveList(AutoSaveFile);
        }

        /// <summary>
        /// Saves the current items list to the given file
        /// </summary>
        public bool SaveList(string fileName)
        {
            if (m_items == null || m_items.Count == 0)
            {
                m_status =  "List not saved. No items in clipboard list";
                return false;
            }

            try
            {
                using (Stream saveStream = File.Open(fileName, FileMode.Create))
                {
                    var bformatter = new BinaryFormatter();
                    saveStream.Position = 0;
                    bformatter.Serialize(saveStream, m_items);          
                }
            }
            catch (Exception ex)
            {
                m_status = $"Saving clipboard list failed: {ex.Message}";
                return false;
            }

            m_status = "Clipboard list saved successfully.";
            return true;
        }

        /// <summary>
        /// Loads the current items list from the autoSave file (if existing)
        /// </summary>
        public bool AutoLoad()
        {
            return LoadList(AutoSaveFile);
        }

        /// <summary>
        /// Loads the current items list from the given file
        /// </summary>
        public bool LoadList(string fileName)
        {          
            if (!File.Exists(fileName))
            {
                m_status = $"Items list not loaded. File {fileName} not found.";
                return false;
            }

            ObservableCollection<IClipboardItem> loadedItems = null;
            try
            {
                using (Stream loadStream = File.Open(fileName, FileMode.Open))
                {
                    var bformatter = new BinaryFormatter();
                    loadStream.Position = 0;
                    loadedItems = (ObservableCollection<IClipboardItem>)bformatter.Deserialize(loadStream);            
                }
            }
            catch (Exception ex)
            {
                m_status = $"Loading items failed: {ex.Message}";
                return false;
            }

            if (loadedItems == null || loadedItems.Count == 0)
            {
                m_status = "Items list not loaded. File seems to be empty or corrupt";
                return false;
            }

            ClearList(clearAutoSaveFile: false);

            m_items = loadedItems;
            m_idCounter = GetMaxIndex();
            ItemsChanged(ItemsChangeType.ItemsLoaded, null);
            m_status = "Items list loaded successfully.";

            return true;
        }

        /// <summary>
        /// Checks if the clipboard conains supported data
        /// </summary>
        private bool CheckForClipboardData(out DataKind type)
        {
            type = DataKind.Undefined;

            if (Clipboard.ContainsText())
            {
                type = DataKind.PlainText;
                return true;
            }
            else if (Clipboard.ContainsImage())
            {
                type = DataKind.Image;
                return true;
            }

            m_status = "No supported dabta in clipboard or clipboard is empty";
            return false;
        }

        private void InitializeItems()
        {
            if (m_items == null)
            {
                m_items = new ObservableCollection<IClipboardItem>();
                m_idCounter = 0;
                ItemsChanged(ItemsChangeType.Initialized, null);
                m_status = "Items initialized";
            }
        }

        private string GetTextTeaser(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (maxLength < 5) { maxLength = 5; }

            string returnText = text.Trim();
            if (returnText.Length <= maxLength)
            {
                if (returnText == text) { return returnText; }
                return returnText + "...";
            }

            return returnText.Substring(0, maxLength) + "...";
        }
    }
}
