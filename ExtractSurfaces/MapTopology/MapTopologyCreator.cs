//
// (C) Copyright 2004-2009 by Autodesk, Inc.

// CreateMapTopology.cs

using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.Topology;
using ExtractSurfaces.Extensions;

namespace TopologySampleCS
{
    /// <summary>
    /// MapTopologyCreator
    /// </summary>
    public class MapTopologyCreator
    {
        /// <summary>
        /// Creates a new Topology from users input of the name, description, type
        /// and selected entities in the drawing file
        /// </summary>
        public void CreateMapTopology(string name, ObjectIdCollection linkCollection)
        {
            string description = "";

            TopologyTypes topoType = TopologyTypes.Polygon;
            
            CreateMapTopology(name, description, topoType, linkCollection);
        }

        /// <summary>
        /// Creates a new Topology from users input of the name, description, type
        /// and selected entities in the drawing file
        /// </summary>
        /// <param name="name">[in] Topology name.</param>
        /// <param name="description">[in] Topology description.</param>
        /// <param name="topologyType">[in] Topology type.</param>
        /// <param name="linkCollection">[in] Collection of Links' Object ID.</param>

        private void CreateMapTopology(string name,
            string description,
            TopologyTypes topologyType,
            ObjectIdCollection linkCollection)
        {
            ObjectIdCollection polygonCentroidCollection = new ObjectIdCollection();
            ObjectIdCollection nodeCollection = new ObjectIdCollection();

            TopologyModel topo = null;
            MapApplication mapApp = HostMapApplicationServices.Application;
            Topologies topos = mapApp.ActiveProject.Topologies;
            if(topos.Exists(name))
                topos.Delete(name,true);
            try
            {
                topos.Create(name, linkCollection, nodeCollection, polygonCentroidCollection, topologyType);
                topo = topos[name];
                topo.Open(Autodesk.Gis.Map.Topology.OpenMode.ForWrite);
                topo.Description = description;
                //topo.Close();
            }
            catch (MapException expt)
            {
                Utility.AcadEditor.WriteMessage(string.Format("\nException throwed containing the error code: {0}", expt.ErrorCode));
            }
            Utility.AcadEditor.Regen();
        }


        public MapTopologyCreator()
        {
        }
    }
}
