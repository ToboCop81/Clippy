/// Clippy - File: "ClipDataManager.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Interfaces;
using Clippy.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr CloseClipboard();

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
                    AddPlainTextItem(m_idCounter, Clipboard.GetText());
                    break;

                case DataKind.Image:
                    m_idCounter++;
                    AddImageItem(m_idCounter, ClipboardImageHelper.ImageFromClipboardDib() as BitmapSource);
                    break;

                default:
                    return false;
            }

            m_status = "Added item from clipboard";
            return true;
        }

        /// <summary>
        /// Copies the data of the given item to the clipboard
        /// </summary>
        /// <param name="index">Index of the clipboard item</param>
        /// <returns></returns>
        public bool CopyDataToClipboard(long index)
        {
            IClipboardItem matchingItem = m_items.FirstOrDefault(i => i.Index == index);
            if (matchingItem == null)
            {
                m_status = $" Failed to copy data to the clipboard. No item with index '{index}' found.";
                return false;
            }

            return CopyDataToClipboard(matchingItem);
        }

        /// <summary>
        /// Copies the data of the given item to the clipboard
        /// </summary>
        public bool CopyDataToClipboard(IClipboardItem itemToCopy)
        {
            if (!itemToCopy.HasData)
            {
                m_status = $" Failed to copy data to the clipboard. Given item has no data";
                return false;
            }

            bool clipboardBlocked = false;

            switch (itemToCopy.Type)
            {
                case DataKind.PlainText:
                    try
                    {
                        Clipboard.SetText(((PlainTextItem)itemToCopy).GetText());
                    }
                    catch (Exception)
                    {
                        try
                        {
                            FreeAndClearClipboard();
                            Clipboard.SetText(((PlainTextItem)itemToCopy).GetText());
                        }
                        catch (Exception)
                        {
                            clipboardBlocked = true;
                        }
                    }
                    break;

                case DataKind.Image:
                    try
                    {
                        Clipboard.SetImage(((ImageItem)itemToCopy).GetImage());
                    }
                    catch (Exception)
                    {
                        try
                        {
                            FreeAndClearClipboard();
                            Clipboard.SetImage(((ImageItem)itemToCopy).GetImage());
                        }
                        catch (Exception)
                        {
                            clipboardBlocked = true;
                        }
                    }
                    break;

                default:
                    m_status = $" Failed to copy data to the clipboard. Unsupported item type: {itemToCopy.Type.ToString()}";
                    return false;
            }

            if (clipboardBlocked)
            {
                string blockingWindow = StaticHelper.GetOpenClipboardWindowInfo();
                m_status = $" Failed to copy data to the clipboard. Cliboard is blocked by another process: {blockingWindow}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds data from a file to the items list
        /// </summary>
        /// <param name="type">The type of data (currently only plain text)</param>
        /// <returns>
        /// TRUE when data was added
        /// FALSE when file was empty or type is not supported
        /// </returns>
        public bool GetDataFromFile(DataKind type)
        {
            if (type != DataKind.PlainText)
            {
                m_status = $"Failed to get data from file. Item type '{type}' not supported.";
                return false;
            }

            string content = GetPlainTextFromFile();
            if (string.IsNullOrEmpty(content))
            {
                if (!ClippySettings.Instance.AllowEmptyClipboardFiles)
                {
                    m_status = $"Unable to get the text from a file. The file content is null or empty";
                    return false;
                }

                content = string.Empty;
            }

            InitializeItems();
            m_idCounter++;
            AddPlainTextItem(m_idCounter,content);

            return true;
        }

        public bool WriteDataToFile(long index)
        {
            IClipboardItem matchingItem = m_items.FirstOrDefault(i => i.Index == index);
            if (matchingItem == null)
            {
                m_status = $" Failed to write data to file. No item with index '{index}' found.";
                return false;
            }

            if (matchingItem.Type != DataKind.PlainText)
            {
                m_status = $"Failed to get data from file. Item type '{matchingItem.Type}' not supported.";
                return false;
            }

            string text = ((PlainTextItem)matchingItem).GetText();
            return WriteTextToFile(text);
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

            ClipboardItemEventArgs args = new ClipboardItemEventArgs(matchingItem);
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

        private bool WriteTextToFile(string text)
        {
            string textoWrite = text;
            string fileName = ClippySettings.Instance.ClipboardTextFileName;
            if (string.IsNullOrEmpty(fileName))
            {
                m_status = $"Unable to write the text to a file. The filename is null or empty";
                return false;
            }

            if (string.IsNullOrEmpty(textoWrite))
            {
                if (!ClippySettings.Instance.AllowEmptyClipboardFiles)
                {
                    m_status = $"Unable to write the text to a file. The content is null or empty";
                    return false;
                }

                textoWrite = string.Empty;
            }

            Encoding encoding = Encoding.GetEncoding(ClippySettings.Instance.ClipboardTextFileEncoding);
            m_status = $"Writing text to '{fileName}'...";

            try
            {
                File.WriteAllText(fileName, textoWrite, encoding);
            }
            catch (Exception ex)
            {
                m_status = $"Failed to write the text to the file: {ex.Message}";
                return false;
            }

            m_status = "Text successfully written to file.";
            return true;
        }

        private string GetPlainTextFromFile()
        {
            string fileName = ClippySettings.Instance.ClipboardTextFileName;
            if (!File.Exists(fileName))
            {
                m_status = $"Unable to get the text from the file. The file was not found.";
                return null;
            }

            m_status = $"Getting text from '{fileName}'...";

            Encoding encoding = Encoding.GetEncoding(ClippySettings.Instance.ClipboardTextFileEncoding);
            string content = null;
            try
            {
                content = File.ReadAllText(fileName, encoding);
            }
            catch (Exception ex)
            {
                m_status = $"Failed to get the text from the file: {ex.Message}";
                return null;
            }

            m_status = "Text successfully retrieved from file.";
            return content;
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

            m_status = "No supported data in clipboard or clipboard is empty";
            return false;
        }

        private void AddPlainTextItem(long id, string text)
        {
            PlainTextItem newPlainText = new PlainTextItem(id, text);
            if (ClippySettings.Instance.TextItemNameFromContent && !string.IsNullOrEmpty(text))
            {
                newPlainText.Title = GetTextTeaser(newPlainText.GetText(), 25);
            }

            m_items.Add(newPlainText);
            ItemsChanged(ItemsChangeType.ItemAdded, new ClipboardItemEventArgs(newPlainText));
        }

        private void AddImageItem(long counter, BitmapSource image)
        {
            ImageItem newImage = new ImageItem(counter, image);
            ItemsChanged(ItemsChangeType.ItemAdded, new ClipboardItemEventArgs(newImage));
            m_items.Add(newImage);
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

            string returnText = text.Replace(Environment.NewLine, " ").Trim();
            if (returnText.Length <= maxLength)
            {
                if (returnText == text) { return returnText; }
                return returnText + "...";
            }

            return returnText.Substring(0, maxLength) + "...";
        }

        private void FreeAndClearClipboard()
        {
            CloseClipboard();
            Clipboard.Clear();
        }
    }
}
