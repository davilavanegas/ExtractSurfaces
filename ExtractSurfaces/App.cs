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
using System.Reflection;
using Autodesk.Aec.Modeler;
using TS = TopologySampleCS;
using TopologySampleCS;


namespace CivilAPI
{
    public class App
    {
        [CommandMethod("ExtractTINSurfacesToDWG")]
        public void ExtractTINSurfacesToDWG()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            Editor editor = document.Editor;

            editor.WriteMessage("Extracting surfaces...\n");

            // Ensure the output directory exists
            string directoryPath = "D:\\Surfaces";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Ensure the output directory exists
            string user = "UROGGIO";
            string templatePath = $"C:\\Users\\{user}\\AppData\\Local\\Autodesk\\C3D 2024\\enu\\Template\\_Autodesk Civil 3D (Metric) NCS.dwt";

            database.Run(tr =>
            {
                TinSurface surface = editor.PickEntityOfType(tr, true, "AECC_TIN_SURFACE", "Select surface: ") as TinSurface;
                editor.WriteMessage($"Surface Name: {surface.Name}\n");

                List<Entity> polylines = editor.PickEntitiesOfType(tr, true, "LWPOLYLINE", "Select polylines: ");
                editor.WriteMessage($"Polylines: {polylines.Count}\n");

                foreach (Polyline polyline in polylines)
                {
                    List<Point2d> points = polyline.GetPoints();
                    editor.WriteMessage($"Polyline Points: {points.Count}\n");
                    Point2dCollection point2dCol = new Point2dCollection(points.ToArray());

                    // Generate file name
                    string fileName = $"{surface.Name}_{polyline.Handle.Value}.dwg";

                    Database exDatabase = ExternalUtils.CreateAndLoad(directoryPath, fileName, templatePath, true);

                    exDatabase.Run(exTr =>
                    {
                        HostApplicationServices.WorkingDatabase = exDatabase;
                        TinSurface.CreateByCropping(exDatabase, $"surface_{polyline.Handle.Value}", surface.ObjectId, point2dCol);
                    });
                    HostApplicationServices.WorkingDatabase = database;
                    exDatabase.SaveAs(directoryPath + "\\" + fileName, DwgVersion.Current);
                }
            });
        }
        
        [CommandMethod("ExtractTINSurfacesToXML")]
        public void ExtractTINSurfacesToXML()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            Editor editor = document.Editor;


            editor.WriteMessage("Extracting surfaces...\n");

            // Ensure the output directory exists
            string directoryPath = ExternalUtils.OpenFolderDialog("Select Destination Folder","xml");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Select objects
            ObjectId surfaceId = editor.PickObjectOfType("AECC_TIN_SURFACE", "Select surface: ");
            List<ObjectId> polylineIds = editor.PickObjectsOfType("LWPOLYLINE", "Select polylines: ");
            
            List<Point2dCollection> point2dCols = new List<Point2dCollection>();
            Point2dCollection point2dCol = new Point2dCollection();
            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                string name = "3D2u0mm0y";
                string name1 = name + "a";

                foreach (ObjectId polylineId in polylineIds)
                {
                    Polyline polyline = new Polyline();

                    using (Transaction tr2 = database.TransactionManager.StartTransaction())
                    {
                        TinSurface surface = (TinSurface)tr2.GetObject(surfaceId, OpenMode.ForRead);
                        ObjectIdCollection sborders;
                        sborders = surface.ExtractBorder(Autodesk.Civil.SurfaceExtractionSettingsType.Plan);
                        try
                        {
                            ObjectIdCollection border = new ObjectIdCollection() { polylineId };
                            //Selected polylines                            
                            MapTopologyCreator creator = new MapTopologyCreator();
                            creator.CreateMapTopology(name, border);
                            
                            //Surface Borders
                            MapTopologyCreator creator1 = new MapTopologyCreator();
                            creator1.CreateMapTopology(name1, sborders);
                            
                            PolygonOverlay overlay = new PolygonOverlay();
                            overlay.Intersect(name, name1);
                            Entity o = overlay.oEntity;

                            TS.Utility.Delete(name);
                            TS.Utility.Delete(name1);

                            polyline = (Polyline)o;
                        }
                        catch (System.Exception ex)
                        {
                            UtilDebug.DebugLog(ex.GetType()+ ex.Message+ex.StackTrace+ex.InnerException);
                            Polyline polyline2 = (Polyline)tr2.GetObject(polylineId, OpenMode.ForRead);
                            polyline = polyline2;
                        }
                        point2dCol = new Point2dCollection(polyline.GetPoints().ToArray());
                        surface.BoundariesDefinition.AddBoundaries(point2dCol, 0.001, Autodesk.Civil.SurfaceBoundaryType.Outer, true);

                        string fileName = $"{surface.Name}_{polyline.Handle.Value}.xml";
                        string filePath = directoryPath + Path.DirectorySeparatorChar + fileName;

                        editor.WriteMessage(filePath + "\n");
                        myLandXML myLandXML = new myLandXML(filePath, surface,surface.Name+"_"+polyline.Handle.Value);
                        tr2.Dispose();
                    }
                }
            }            
        }

        [CommandMethod("ExportTINSurfaceToXML")]
        public void ExportTINSurfaceToXML()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityOptions prompt = new PromptEntityOptions("select surface");
            prompt.SetRejectMessage("not");
            prompt.AllowNone = false;
            prompt.AddAllowedClass(typeof(TinSurface), true);

            PromptEntityResult res = ed.GetEntity(prompt);

            if (res.Status != PromptStatus.OK)
                return;
            ObjectId oidSurface = res.ObjectId;

            prompt.Message = "Selec Polyline";
            prompt.AddAllowedClass(typeof(Polyline), true);

            res = ed.GetEntity(prompt);
            ObjectId oidPolyline = res.ObjectId;

            if (res.Status != PromptStatus.OK)
                return;
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                Polyline polyline = (Polyline)tr.GetObject(oidPolyline, OpenMode.ForRead);
                Point2dCollection point2s = new Point2dCollection();
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    Point2d point = polyline.GetPoint2dAt(i);
                    point2s.Add(point);
                }

                TinSurface surface = (TinSurface)tr.GetObject(oidSurface, OpenMode.ForWrite);
                ed.WriteMessage(surface.GetTinProperties().NumberOfTriangles + "\n");
                //fictitious crop, add boundaries outer produce only remain triangles and points
                surface.BoundariesDefinition.AddBoundaries(point2s, 0.001, Autodesk.Civil.SurfaceBoundaryType.Outer, true);
                ed.WriteMessage(surface.GetTinProperties().NumberOfTriangles + "\n");

                //
                string filePath = UtilDebug.IntPath + Path.DirectorySeparatorChar + surface.Name + ".xml";
                ed.WriteMessage(filePath + "\n");
                myLandXML myLandXML = new myLandXML(filePath, surface, surface.Name + "_" + polyline.Handle.Value);
                tr.Dispose();
            }
        }
    }
}
