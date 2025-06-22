using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using ExtractSurfaces.Extensions;
using System;
using System.IO;


public class LandXmlExporter
{
    [CommandMethod("ExportTINtoLandXML")]
    public void ExportTINtoLandXML()
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
            string filePath = UtilDebug.IntPath + Path.DirectorySeparatorChar + surface.Name +".xml";
            ed.WriteMessage(filePath+"\n");
            myLandXML myLandXML = new myLandXML(filePath, surface);
            tr.Dispose();
        }
    }
   
}
