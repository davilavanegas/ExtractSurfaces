using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.Settings;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using CivilAPI.Extensions;
using ExtractSurfaces.Extensions;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace CivilAPI
{
    public class App
    {
        [CommandMethod("ExtractSurfaces")]
        public void ExtractSurfaces()
        {
            DateTime dateTime = DateTime.Now;
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            Editor editor = document.Editor;

                editor.WriteMessage("Extracting surfaces...\n");

                // Ensure the output directory exists
                string directoryPath = "C:\\Users\\RINAT\\Downloads\\Surfaces";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Ensure the output directory exists
                string user = "RINAT";
                string templatePath = "C:\\Users\\"+user+"\\AppData\\Local\\Autodesk\\C3D 2024\\enu\\Template\\_Autodesk Civil 3D (Metric) NCS.dwt";

            database.Run(tr =>
                {
                    TinSurface surface = editor.PickEntityOfType(tr, true, "AECC_TIN_SURFACE", "Select surface: ") as TinSurface;
                    editor.WriteMessage($"Surface Name: {surface.Name}\n");
                    
                    List<Entity> polylines = editor.PickEntitiesOfType(tr, true, "LWPOLYLINE", "Select polylines: ");
                    UtilDebug.DebugLog($"Polylines: {polylines.Count}\n");

                    foreach (Polyline polyline in polylines)
                    {
                        List<Point2d> points = polyline.GetPoints();
                        UtilDebug.DebugLog(polyline.Handle.Value.ToString());
                        Point2dCollection point2dCol = new Point2dCollection(points.ToArray());

                        // Generate file name
                        string fileName = $"{surface.Name}_{polyline.Handle.Value}.dwg";

                        Database exDatabase = ExternalDocument.CreateAndLoad(directoryPath, fileName, templatePath, true);

                    exDatabase.Run(exTr =>
                        {
                            HostApplicationServices.WorkingDatabase = exDatabase;
                        ObjectId newSurfaceId = TinSurface.CreateByCropping(exDatabase, $"surface_{polyline.Handle.Value}", surface.ObjectId, point2dCol);
                        TinSurface newSurface = exTr.GetObject(newSurfaceId, OpenMode.ForWrite) as TinSurface;
                    });
                            HostApplicationServices.WorkingDatabase = database;

                    exDatabase.SaveAs(directoryPath + "\\" + fileName, DwgVersion.Current);
                    break;
            }

            });

        }
    }
}
