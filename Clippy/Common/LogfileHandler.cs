/// Clippy - File: "LogfileHandler.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.IO;
using System.Text;

namespace Clippy.Common
{

    public class LogfileHandler
    {
        #region properties

        private String path;

        /// <summary>
        /// Gets or sets the path where the logfile is located
        /// (removes last backslash if existing)
        /// </summary>
        public String Path
        {
            get { return path; }
            set 
            {
                //Remove last backslash from path if existing
                if (value.Length > 0)
                {
                    if (value.Substring(value.Length - 1, 1) == "\\")
                    {
                        path = value.Substring(0, value.Length - 1);
                    }
                    else { path = value; }
                }
                else { path = value; }
            }
        }

        /// <summary>
        /// Gets or sets the filename of the logfile
        /// (without date stamp)
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// Get the filename with datestamp 
        /// if "AddDateStamp" is TRUE
        /// </summary>
        public String FilenameDatestamp 
        { 
            get; 
            private set; 
        }

        private String actualFilename;

        private Boolean addDst;

        /// <summary>
        /// Add the actual date to the filename
        /// Format: Filename_YYYYMMDD.ext
        /// </summary>
        public Boolean AddDateStamp
        {
            get { return addDst; }
            set 
            {
                if (value)
                {
                    addDateStamp();
                }
                else
                {
                    FilenameDatestamp = "";
                }
                addDst = value; 
            }
        }

        /// <summary>
        /// Mode how to handle the logfile
        /// </summary>
        private LogMode LogMode { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Empty default constructor
        /// </summary>
        public LogfileHandler()
        {
            AddDateStamp = false;
            LogMode = LogMode.attach;
            FilenameDatestamp = "";
            actualFilename = "";
        }

        /// <summary>
        /// Creates new instance of LogfileHandler
        /// </summary>
        /// <param name="path">Folder where the logfile is located</param>
        /// <param name="filename">Filename of the log file</param>
        /// <param name="addDateStamp">Add a date stamp to the filename</param>
        /// <param name="logMode">Mode how to handle the logfile</param>
        public LogfileHandler(String path, String filename, Boolean addDateStamp, LogMode logMode)
        {
            Path = path;
            Filename = filename;
            AddDateStamp = addDateStamp;
            LogMode = logMode;
            actualFilename = "";
        }

        #endregion

        #region methods

        /// <summary>
        /// Adds a new entry to the logfile with a timestamp
        /// </summary>
        /// <param name="logEntry">Log entry text</param>
        /// <param name="useDateStamp">Adds the date to the timestamp (optional)</param>
        /// <param name="singleLine">
        /// Replace carriage returns and line breaks with blank spaces
        /// to fit the text into a single line (default: TRUE)
        /// </param>
        public void AddEntry(String logEntry, Boolean singleLine = true, Boolean useDateStamp = false)
        {
            if (singleLine) { logEntry = logEntry.Replace(Environment.NewLine, " "); }

            String timestamp;
            if (useDateStamp) { timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss | "); }
            else { timestamp = DateTime.Now.ToString("HH:mm:ss | "); }

            updateActualFilename();

            if (!File.Exists(actualFilename))  { createFile(); }

            StreamWriter logFile = new StreamWriter(actualFilename, true);
            logFile.WriteLine(timestamp + logEntry);
            logFile.Close();
        }

        /// <summary>
        /// Adds an empty line to the log file
        /// </summary>
        public void AddSpace()
        {
            updateActualFilename();

            if (!File.Exists(actualFilename)) { createFile(); }

            StreamWriter logFile = new StreamWriter(actualFilename, true);
            logFile.WriteLine();
            logFile.Close();
        }

        /// <summary>
        /// Creates a new log file if the file not exists or overwrites an existing one
        /// when LogMode = LogMode.overwrite
        /// </summary>
        /// <returns>TRUE when logfile was created successfully</returns>
        private Boolean createFile()
        {
            StreamWriter logFile;

            updateActualFilename();

            if (File.Exists(actualFilename))
            {
                if (LogMode == LogMode.overwrite)
                {
                    File.Delete(actualFilename);
                    logFile = new StreamWriter(actualFilename);
                }
                else
                {
                    throw new IOException("Logfile " + actualFilename + "already exists. Overwrite is disabled.");
                }
            }
            else
            {
                if (!Directory.Exists(Path)) { Directory.CreateDirectory(Path); }
                logFile = new StreamWriter(actualFilename);
            }

            logFile.WriteLine("Logfile created: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            logFile.WriteLine("--------------------------------------------------------------------------------");
            logFile.WriteLine();

            logFile.Close();
            return true;
        }

        /// <summary>
        /// Adds a datestamp to the filename like this:
        /// Filename_20120821.ext
        /// </summary>
        private void addDateStamp()
        {
            if (Filename != "")
            {
                String[] parts = Filename.Split('.');
                String datestamp = DateTime.Now.ToString("_yyyyMMdd");
                parts[parts.Length - 2] += datestamp;
                FilenameDatestamp = Implode(parts, ".");
            }
            else
            {
                FilenameDatestamp = "";
            }
        }

        private void updateActualFilename()
        {
            if (AddDateStamp) { actualFilename = Path + "\\" + FilenameDatestamp; }
            else { actualFilename = Path + "\\" + Filename; }
        }


        /// <summary>
        /// Joins all elements of an array using the "glue" string
        /// </summary>
        /// <param name="container">Array which contains the elements to implode</param>
        /// <param name="glue">String which will be placed between the elements</param>
        /// <returns>
        /// String which contains all array elements and the "glue" strings between
        /// Returns empty string when container is NULL or empty
        /// Returns content of first element if array has only one element
        /// </returns>
        private string Implode(object[] container, string glue)
        {
            if (container != null && container.Length > 0)
            {
                StringBuilder resultStr = new StringBuilder();
                for (int i = 0; i <= container.Length - 2; i++)
                {
                    resultStr.Append(container[i].ToString()).Append(glue);
                }
                resultStr.Append(container[container.Length - 1].ToString());
                return resultStr.ToString();
            }
            else if (container != null && container.Length == 1)
            {
                return container[0].ToString();
            }
            else
            {
                return "";
            }
        }

        #endregion
    }
}
