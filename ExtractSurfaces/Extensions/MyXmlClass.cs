using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExtractSurfaces.Extensions
{
    class myLandXML
    {
        //Variagles
        private static string Txtschema = "http://www.landxml.org/schema/LandXML-1.2";
        //var Units
        private static string AreaUnits = "squareMeter";
        private static string LinearUnit = "meter";
        private static string VolumeUnit = "cubicMeter";
        private static string TemperatureUnit = "celsius";
        private static string PressureUnit = "milliBars";
        private static string DiameterUnit = "millimeter";
        private static string AngularUnit = "decimal degrees";
        private static string DirectionUnit = "decimal degrees";
        //var Project
        private static string DwgName = "nombre de proyecto";
        //var Application
        private static string SoftwareName = "Autodesk Civil 3D";
        private static string Description = "Description";
        private static string Language = "en";

        private static Document Doc = Application.DocumentManager.MdiActiveDocument;
        private static XmlWriter writer;
        public myLandXML(string filePath, TinSurface tinSurface)
        {
            GetAppName();
            writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 });
            Header();
            xmlSurface(tinSurface);

            // Write the close tag for the root element.
            writer.WriteEndElement();

            // Write the XML to file and close the writer.
            writer.Flush();
            writer.Close();
        }
        private void GetAppName()
        {
            string root = HostApplicationServices.Current.MachineRegistryProductRootKey;
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(root, false))
            {
                SoftwareName = rk.GetValue("ProductNameGlob") as string;
                Language = rk.GetValue("Language") as string;
            }
            DwgName = Doc.Name;
            SettingsSurface settingsSurface = CivilApplication.ActiveDocument.Settings.GetSettings<SettingsSurface>();
            AreaUnits = settingsSurface.Area.Unit.Value.ToString();
            LinearUnit = settingsSurface.Dimension.Unit.Value.ToString();
            VolumeUnit = settingsSurface.Volume.Unit.Value.ToString();
        }
        private static void Header()
        {
            writer.WriteStartDocument();

            // Write the root element.
            writer.WriteStartElement("LandXML", Txtschema);

            // Write the root element.
            writer.WriteAttributeString("xmlns", "", Txtschema);

            // Agrega xmlns:xsi
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

            // Agrega xsi:schemaLocation
            writer.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance",
                "http://www.landxml.org/schema/LandXML-1.2 http://www.landxml.org/schema/LandXML-1.2/LandXML-1.2.xsd");

            // Atributos adicionales
            writer.WriteAttributeString("date", DateTime.Now.Date.ToShortDateString());
            writer.WriteAttributeString("time", DateTime.Now.ToLongTimeString());
            writer.WriteAttributeString("version", "1.2");
            writer.WriteAttributeString("language", Language);
            writer.WriteAttributeString("readOnly", "false");

            // Units
            writer.WriteStartElement("Units");
            //Metric
            writer.WriteStartElement("Metric");
            writer.WriteAttributeString("areaUnit", AreaUnits);
            writer.WriteAttributeString("linearUnit", LinearUnit);
            writer.WriteAttributeString("volumeUnit", VolumeUnit);
            writer.WriteAttributeString("temperatureUnit", TemperatureUnit);
            writer.WriteAttributeString("pressureUnit", PressureUnit);
            writer.WriteAttributeString("diameterUnit", DiameterUnit);
            writer.WriteAttributeString("angularUnit", AngularUnit);
            writer.WriteAttributeString("directionUnit", DirectionUnit);
            //end Metric
            writer.WriteEndElement();
            //end Units
            writer.WriteEndElement();

            //Project
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("name", DwgName);
            //end //Project
            writer.WriteEndElement();

            //Application
            writer.WriteStartElement("Application");
            writer.WriteAttributeString("name", DwgName);
            writer.WriteAttributeString("desc", Description);
            writer.WriteAttributeString("manufacturer", Description);
            //end Application
            writer.WriteEndElement();
        }


        private static void xmlSurface(TinSurface tinSurface)
        {
            //Surface
            writer.WriteStartElement("Surfaces");
            writer.WriteStartElement("Surface");
            writer.WriteAttributeString("name", tinSurface.Name);
            writer.WriteAttributeString("desc", tinSurface.Description);

            writer.WriteStartElement("SourceData");
            writer.WriteEndElement();

            //Definition
            writer.WriteStartElement("Definition");
            writer.WriteAttributeString("surfType", "TIN");
            writer.WriteAttributeString("area2DSurf", tinSurface.GetTerrainProperties().SurfaceArea2D.ToString());
            writer.WriteAttributeString("area3DSurf", tinSurface.GetTerrainProperties().SurfaceArea3D.ToString());
            writer.WriteAttributeString("elevMax", tinSurface.GetGeneralProperties().MaximumElevation.ToString());
            writer.WriteAttributeString("elevMin", tinSurface.GetGeneralProperties().MinimumElevation.ToString());
            // Puntos (Pnts)
            writer.WriteStartElement("Pnts");
            int i = 5;
            foreach (TinSurfaceVertex vertex in tinSurface.Vertices)
            {
                writer.WriteStartElement("P");
                writer.WriteAttributeString("id", vertex.GetHashCode().ToString());
                writer.WriteString($"{vertex.Location.Y} {vertex.Location.X} {vertex.Location.Z} ");
                writer.WriteEndElement(); // P
                i++;
            }
            writer.WriteEndElement(); // Pnts

            // Triángulos (Faces)
            writer.WriteStartElement("Faces");
            foreach (TinSurfaceTriangle triangle in tinSurface.Triangles)
            {
                writer.WriteStartElement("F");
                writer.WriteString($"{triangle.Vertex1.GetHashCode()} {triangle.Vertex2.GetHashCode()} {triangle.Vertex3.GetHashCode()}");
                writer.WriteEndElement(); // F
            }
            writer.WriteEndElement(); // Faces

            //end Definition
            writer.WriteEndElement();

            //end Surface 
            writer.WriteEndElement();
        }
    }
}
