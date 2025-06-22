using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractSurfaces.Extensions
{
    class UtilDebug
    {
        private static readonly bool debug = true;
        public static string IntPath { get; } = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static void DebugWline(string message)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (debug)
            {
                doc.Editor.WriteMessage(message);
            }
        }

        public static void DebugLogClean()
        {
            string path1 = IntPath + Path.DirectorySeparatorChar + "log.txt";
            if (File.Exists(path1))
                File.Delete(path1);
        }
        public static void DebugLog(string message)
        {

            using (StreamWriter sw = new StreamWriter(IntPath + Path.DirectorySeparatorChar + "log.txt", true))
            {
                sw.WriteLine(message);
            }
        }
    }
}
