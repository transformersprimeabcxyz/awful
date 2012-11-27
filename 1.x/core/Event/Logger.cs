using System;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace Awful.Core.Event
{
    public class Logger
    {
        private static readonly Logger instance = new Logger();
        private bool isEnabled;

        private readonly StringBuilder logBuilder = new StringBuilder();
        private Logger() { }

        public static bool IsEnabled
        {
            get { return instance.isEnabled; }
        }

        public static void Enable() { instance.isEnabled = true; }
        public static void Disable() { instance.isEnabled = false; }

        public static void ImportLog(string text)
        {
            instance.logBuilder.Clear();
            instance.logBuilder.AppendLine(text);
        }

        public static void AddEntry(string caption, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Empty);
            sb.AppendLine(caption);
            sb.AppendLine("----------");
            sb.AppendLine(ex.GetType().ToString());
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            sb.AppendLine("----------");
            AddEntry(sb.ToString());
        }

        public static void AddEntry(Exception ex)
        {
            AddEntry("An unknown error occured:", ex);
        }

        public static void AddEntry(string text)
        {
            var debug = System.Diagnostics.Debugger.IsAttached;
            if (debug)
            {
                Debug.WriteLine(text);
            }

            if (IsEnabled == false)
                return;

            instance.logBuilder.AppendLine(string.Format("[{0}] {1}", DateTime.Now, text));
        }

        public static string ExportToText()
        {
            return instance.logBuilder.ToString();
        }

        public static void ImportFromFile()
        {
            Stream stream = null;
            try
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.DirectoryExists("logs") == false)
                    store.CreateDirectory("logs");

                string filePath = string.Format("logs/log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                stream = store.OpenFile(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);

                using (StreamReader reader = new StreamReader(stream))
                {
                    instance.logBuilder.Clear();
                    string text = reader.ReadToEnd();
                    instance.logBuilder.AppendLine(text);
                }
            }

            catch (Exception)
            {
                MessageBox.Show("There was an error reading from the debug file.", ":(", MessageBoxButton.OK);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        public static void ExportToFile()
        {
            Stream stream = null;
            try
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.DirectoryExists("logs") == false)
                    store.CreateDirectory("logs");

                string filePath = string.Format("logs/log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                stream = store.OpenFile(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        reader.ReadToEnd();
                        writer.WriteLine(instance.logBuilder.ToString());
                        instance.logBuilder.Clear();
                    }
                }
            }

            catch (Exception) { 
                MessageBox.Show("There was an error writing to the debug file.", ":(", MessageBoxButton.OK); }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
    }
}
