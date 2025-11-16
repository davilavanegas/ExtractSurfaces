//
//

// PolygonOverlay.cs

using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.Topology;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ExtractSurfaces.Extensions;

namespace TopologySampleCS
{
    /// <summary>
    /// PolygonOverlay
    /// </summary>
    public sealed class PolygonOverlay
    {
        public Entity oEntity;
        /// <summary>
        /// Combines polygons with polygons and keeps all geometry. Union acts
        /// like the Boolean OR operation and can be used only with polygons.
        /// </summary>
        /// <param name="sourceTopologyName">[in] The source topology name.</param>
        /// <param name="overlayTopologyName">[in] The overlay topology name.</param>
        /// <returns>  
        /// Returns true if successful.
        /// </returns>
        private bool Union(string sourceTopologyName, string overlayTopologyName)
        {
            // Create the Source AcMapTopology object
            TopologyModel sourceTopology = null;

            // Create the Source AcMapOverlayDataArray object
            OverlayDataCollection sourceDataCollection = null;

            MapApplication mapApp = HostMapApplicationServices.Application;
            Topologies topos = mapApp.ActiveProject.Topologies;

            // Does the Source Topology exist to get information from
            if (topos.Exists(sourceTopologyName))
            {
                sourceTopology = topos[sourceTopologyName];
            }
            else
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nERROR: The topology {0} doesn't exist.", sourceTopologyName));
                return false;
            }

            // Create the Overlay AcMapTopology object
            TopologyModel overlayTopology = null;

            // Create the Overlay AcMapOverlayDataArray object
            OverlayDataCollection overlayDataCollection = new OverlayDataCollection();
            string expression = "";
            expression = string.Format(":AREA@TPMCNTR_{0}", overlayTopologyName);
            OverlayData data = new OverlayData(expression, "PolygonArea", Autodesk.Gis.Map.Constants.DataType.Real);
            overlayDataCollection.Add(data);

            // Does the Overlay Topology exist to get information from
            if (topos.Exists(overlayTopologyName))
            {
                overlayTopology = topos[overlayTopologyName];
            }
            else
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nERROR: The topology {0} doesn't exist.", overlayTopologyName));
                return false;
            }

            try
            {
                // Open the source topology for read
                sourceTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);

                // Open the Overlay topology for read
                overlayTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);

                // Set the Node create properties
                PointCreationSettings nodeCreationSettings = new PointCreationSettings(
                    "result",           // layer name
                    1,                  // color, by layer
                    true,               // create new node
                    "ACAD_POINT");      // block name

                // Set the Node creation settings
                sourceTopology.SetNodeCreationSettings(nodeCreationSettings);

                // New topology name
                string newTopologyName = "Result";
                // New topology description
                string newTopologyDesc = "Result topology of an Union operation";

                // Create the result data table
                ObjectDataTable resultDataTable = new ObjectDataTable();
                resultDataTable.ODTableName = "UnionResult";
                resultDataTable.ODTableDescription = "The result of the Union Overlay";

                // Union
                sourceTopology.Union(overlayTopology,
                    newTopologyName,
                    newTopologyDesc,
                    resultDataTable,
                    sourceDataCollection,
                    overlayDataCollection);

                TopologyModel newTopology = topos[newTopologyName];
                newTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);
                newTopology.ShowGeometry(5);
                newTopology.Close();

                return true;
            }
            catch (MapException e)
            {
                if (2001 == e.ErrorCode)
                {
                    Utility.AcadEditor.WriteMessage("\nERROR: Topology Result already exists.");
                }
                else
                {
                    Utility.AcadEditor.WriteMessage(string.Format("\nERROR: Operation failed with error code: {0}.", e.ErrorCode));
                }
                return false;
            }
            finally
            {
                // Close the topologys
                sourceTopology.Close();
                overlayTopology.Close();
            }
        }
       
        /// <summary>
        /// Intersect and get entity
        /// </summary>
        /// <param name="sourceTopologyName">[in] The source topology name.</param>
        /// <param name="overlayTopologyName">[in] The overlay topology name.</param>
        /// <returns>  
        /// Returns true if successful.
        /// </returns>
        private Entity Intersect(string sourceTopologyName, string overlayTopologyName, bool t)
        {
            // Create the Source AcMapTopology object
            TopologyModel sourceTopology = null;

            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.MdiActiveDocument;

            // Create the Source AcMapOverlayDataArray object
            //OverlayDataCollection sourceDataCollection = null;
            MapApplication mapApp = HostMapApplicationServices.Application;
            Topologies topos = mapApp.ActiveProject.Topologies;
            // Does the Source Topology exist to get information from
            if (topos.Exists(sourceTopologyName))
            {
                sourceTopology = topos[sourceTopologyName];
            }
            else
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nERROR: The topology {0} doesn't exist.", sourceTopologyName));
                return null;
            }

            // Create the Overlay AcMapTopology object
            TopologyModel overlayTopology = null;

            // Does the Overlay Topology exist to get information from
            if (topos.Exists(overlayTopologyName))
            {
                overlayTopology = topos[overlayTopologyName];
            }
            else
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nERROR: The topology {0} doesn't exist.", overlayTopologyName));
                return null;
            }

            try
            {
                // Open the source topology for read
                sourceTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);

                // Open the Overlay topology for read
                overlayTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);

                // Set the Node create properties
                PointCreationSettings nodeCreationSettings = new PointCreationSettings(
                    "result",           // layer name
                    1,                  // color, by layer
                    true,               // create new node
                    "ACAD_POINT");      // block name

                // Set the Node creation settings
                sourceTopology.SetNodeCreationSettings(nodeCreationSettings);

                // New topology name
                string newTopologyName = "Result";
                // New topology description
                string newTopologyDesc = "Result topology of an Union operation";

                // Union
                sourceTopology.Intersect(overlayTopology,
                    newTopologyName,
                    newTopologyDesc);

                TopologyModel newTopology = topos[newTopologyName];
                newTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);

                //Method GetEntityIds return Polylines and DBPoint
                ObjectIdCollection ocol = newTopology.GetEntityIds();
                newTopology.Close();
                topos.Delete(newTopologyName, t);
                List<Entity> entities = new List<Entity>();
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId i in ocol)
                    {
                        var o = (Entity)tr.GetObject(i, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        if (o.GetType() == typeof(DBPoint))
                        {
                            o.UpgradeOpen();
                            o.Erase();
                        }
                        else
                        {
                            o.UpgradeOpen();
                            entities.Add(o);
                        }
                        //doc.Editor.WriteMessage("\n" + o.GetType().ToString());
                    }
                    oEntity = Utility.Join(entities);
                    tr.Commit();
                }

                return oEntity;
            }
            catch (MapException e)
            {
                if (2001 == e.ErrorCode)
                {
                    Utility.AcadEditor.WriteMessage("\nERROR: Topology Result already exists.");
                }
                else
                {
                    Utility.AcadEditor.WriteMessage(string.Format("\nERROR: Operation failed with error code: {0} {1}.", e.ErrorCode, e.StackTrace));
                }
                return null;
            }
            finally
            {
                // Close the topologys
                sourceTopology.Close();
                overlayTopology.Close();
            }
        }
        /// <summary>
        /// Combines polygons with polygons and keeps all geometry. Union acts
        /// like the Boolean OR operation and can be used only with polygons.
        /// </summary>
        public void Union()
        {
            string sourceTopologyName = "";
            string overlayTopologyName = "";
            Editor editor = Utility.AcadEditor;

            PromptResult promptResult = editor.GetString("\nEnter the Source Topology name:");
            if (promptResult.Status == PromptStatus.Cancel)
            {
                return;
            }
            sourceTopologyName = promptResult.StringResult;

            promptResult = editor.GetString("\nEnter the Overlay Topology name:");
            if (promptResult.Status == PromptStatus.Cancel)
            {
                return;
            }
            overlayTopologyName = promptResult.StringResult;

            Union(sourceTopologyName, overlayTopologyName);
        }
        public void Intersect(string sourceTopologyName, string overlayTopologyName)
        {
            Intersect(sourceTopologyName, overlayTopologyName, false);
        }
        public PolygonOverlay()
        {
        }
    }
}
