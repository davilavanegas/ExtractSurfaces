using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using AcWi = Autodesk.AutoCAD.Windows;

namespace CivilAPI.Extensions
{
    public static class ExternalUtils
    {
        public static void Create(string directoryPath, string fileName, string templateFilePath, bool overwrite = false)
        {
            FileCreationChecks(directoryPath, fileName, templateFilePath, overwrite);

            try
            {
                var filePath = directoryPath + "\\" + fileName;

                // Using false here for buildDefaultDrawing because we are reading from a template file.
                using (Database db = new Database(false, true))
                {
                    db.ReadDwgFile(templateFilePath, FileShare.Read, true, null);
                    db.SaveAs(filePath, DwgVersion.Current);
                }
            }
            catch { throw; }
        }

        public static Database LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("Invalid file path.");
            }

            if (!File.Exists(filePath))
            {
                throw new ArgumentException("File does not exist.");
            }

            if (!HasValidExtension(filePath))
            {
                throw new ArgumentException("Invalid file extension");
            }

            try
            {
                Database database = new Database(false, true);
                database.ReadDwgFile(filePath, FileOpenMode.OpenForReadAndAllShare, true, null);
                return database;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
        public static Database CreateAndLoad(string directoryPath, string fileName, string templateFilePath, bool overwrite = false)
        {
            FileCreationChecks(directoryPath, fileName, templateFilePath, overwrite);

            try
            {
                var filePath = directoryPath + "\\" + fileName;

                // Using false here for buildDefaultDrawing because we are reading from a template file.
                using (Database db = new Database(false, true))
                {
                    db.ReadDwgFile(templateFilePath, FileShare.Read, true, null);
                    db.SaveAs(filePath, DwgVersion.Current);
                }
                return LoadFromFile(filePath);
            }
            catch { throw; }
        }
        public static void Save(this Database database)
        {
            try
            {
                database.SaveAs("temp", DwgVersion.Current);
                return;
            }
            catch
            {
                throw new InvalidOperationException("The file is currently in use by another application and cannot be saved.");
            }
        }
        private static bool HasValidExtension(string fileName)
        {
            // Check extensions
            if (!fileName.Contains(".dwg") && !fileName.Contains(".dwt") && !fileName.Contains(".dws") && !fileName.Contains(".dxf"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private static void FileCreationChecks(string directoryPath, string fileName, string templateFilePath, bool overwrite = false)
        {
            // Check inputs
            if (string.IsNullOrEmpty(directoryPath)) { throw new ArgumentException("Invalid directory path"); }
            if (string.IsNullOrEmpty(fileName)) { throw new ArgumentException("Invalid file name"); }
            if (!File.Exists(templateFilePath)) { throw new ArgumentException("A valid file does not exist at the template file path."); }

            // Check for valid DWT file
            var fileInfo = new FileInfo(templateFilePath);
            if (fileInfo.Extension != ".dwg" && fileInfo.Extension != ".dwt" && fileInfo.Extension != ".dws")
            {
                throw new ArgumentException("Invalid template file extension.");
            }

            // Check extension for new file
            if (!HasValidExtension(fileName)) { throw new ArgumentException("Invalid file extension."); }

            // Check if file with same name already exists
            if (File.Exists(directoryPath + "\\" + fileName)
                && overwrite == false)
            {
                throw new InvalidOperationException("File does not exist.");
            }
        }

        public static string OpenFolderDialog(string title, string extension)
        {
            var openFileDialog = new AcWi.OpenFileOrFolderDialog(
                title,
                "",
                "xml",
                "",
                AcWi.OpenFileDialog.OpenFileDialogFlags.AllowFoldersOnly);
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
                return openFileDialog.FileOrFoldername;
            return null;
        }
    }
}
