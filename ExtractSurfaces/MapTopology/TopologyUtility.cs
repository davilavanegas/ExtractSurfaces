
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

// TopologySampleCommands.cs

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Gis.Map.Topology;
using Autodesk.Gis.Map;
using System.Collections.Generic;

namespace TopologySampleCS
{
    internal class Utility
    {
        private Utility()
        {
        }

        internal static Autodesk.AutoCAD.EditorInput.Editor AcadEditor
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }
        /// <summary>
        /// delete topology, keep object less dbpoint
        /// </summary>
        /// <param name="name"></param>
        public static void Delete(string name)
        {
            MapApplication mapApp = HostMapApplicationServices.Application;
            Topologies topos = mapApp.ActiveProject.Topologies;

            // Does the Source Topology exist to get information from
            if (topos.Exists(name))
            {
                TopologyModel newTopology = topos[name];
                newTopology.Open(Autodesk.Gis.Map.Topology.OpenMode.ForRead);
                ObjectIdCollection ocol = newTopology.GetEntityIds();
                newTopology.Close();
                topos.Delete(name, false);
                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId i in ocol)
                    {
                        var o = (Entity)tr.GetObject(i, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        if (o.GetType() == typeof(DBPoint))
                        {
                            o.UpgradeOpen();
                            o.Erase();
                        }
                    }
                    tr.Commit();
                }
            }
            else
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nERROR: The topology {0} doesn't exist.", name));
                return;
            }
        }
        public static Entity Join(List<Entity> col)
        {
            Entity[] entities = new Entity[col.Count-1];
            var o = col[0];
            col.RemoveAt(0);

            for (int i = 0; i < col.Count; i++)
            {
                entities[i] = col[i];
            }

            o.JoinEntities(entities);
            foreach (Entity e in entities)
            {
                e.Erase(); 
            }
            return o;

        }
        /// <summary>
        /// Sends a command string to the current document of AutoCAD Map 3D to execute.
        /// </summary>
        /// <returns>Returns true if successfully.</returns>
        public static bool SendCommand(string cmd)
        {
            try
            {
                Document doc = null;
                doc = Application.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute(cmd, true, false, true);
                return true;
            }
            catch (System.Exception e)
            {
                AcadEditor.WriteMessage(e.Message);
                return false;
            }
        }
    }
}
