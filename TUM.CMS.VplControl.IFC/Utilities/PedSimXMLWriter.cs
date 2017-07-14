using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public static class PedSimXMLWriter
    {
        public enum areaType
        {
            Intermediate,
            Origin
        }

        public static void Serialize(simulator sim, string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(simulator));

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }

                using (TextWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, sim);
                }
            }
            catch (Exception)
            {
            }
            
        }

        public static simulator Deserialize(string filePath)
        {
            try
            {
                simulator myObject;
                // Construct an instance of the XmlSerializer with the type  
                // of object that is being deserialized.  
                XmlSerializer mySerializer =
                new XmlSerializer(typeof(simulator));
                // To read the file, create a FileStream.  
                FileStream myFileStream = new FileStream(filePath, FileMode.Open);
                // Call the Deserialize method and cast to the object type.  
                return (simulator)mySerializer.Deserialize(myFileStream);
            }
            catch (Exception e)
            {
            }

            return null;
        }

    }

    // Classes
    [XmlRoot("simulator")]
    public class simulator
    {
        // version="0.9.0" simulationName="SCC_2016_Validation_IKOM_Final" simEnd="2200.0" timeStepDuration="0.1">
        [XmlAttribute("version")]
        public string version;
        [XmlAttribute("simulationName")]
        public string simulationName;
        [XmlAttribute("simEnd")]
        public double simEnd;
        [XmlAttribute("timeStepDuration")]
        public double timeStepDuration;

        [XmlElement("layouts")]
        public List<layouts> layouts;

        public simulator()
        {
            version= @"0.9.0";
            simEnd = 1.0;
            timeStepDuration = 1.0;
            layouts = new List<layouts>();
        }
    }

    public class layouts
    {
        [XmlElement("scenario")]
        public scenario scenario;
    }

    public class scenario
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("maxX")]
        public double maxX;
        [XmlAttribute("maxY")]
        public double maxY;
        [XmlAttribute("minX")]
        public double minX;
        [XmlAttribute("minY")]
        public double minY;
        [XmlElement("area")]
        public List<area> areas;
        [XmlElement("obstacle")]
        public List<obstacle> obstacles;

        [XmlElement("graph")]
        public List<graph> graphs;

        public scenario()
        {
            areas = new List<area>();
            obstacles = new List<obstacle>();
        }

        public void AddArea(string name, List<point> points, PedSimXMLWriter.areaType areaType)
        {
            var a = new area
            {
                points = points,
                name = name,
                id = areas.Count,
                // category = "close",
                type = areaType.ToString()
            };
            areas.Add(a);
        }

        public void AddSolidObstacle(string name, List<point> points)
        {
            var o = new obstacle()
            {
                points = points,
                name = name,
                id = obstacles.Count,
                type = "Solid"
            };
            obstacles.Add(o);
        }

        public void AddWallObstacle(string name, point point1, point point2)
        {
            var o = new obstacle()
            {
                points = new List<point>{point1, point2},
                name = name,
                id = obstacles.Count - 1,
                type = "Wall"
            };
            obstacles.Add(o);
        }
    }

    public class graph
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("name")]
        public string name;

        [XmlElement("vertex")]
        public List<vertex> vertices;

        [XmlElement("edge")]
        public List<edge> edges;
    }

    public class vertex
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("name")]
        public string name;
        [XmlElement("center")]
        public center center;
    }

    public class center
    {
        [XmlAttribute("x")]
        public double x;
        [XmlAttribute("y")]
        public double y;
    }

    public class edge
    {
        [XmlAttribute("idLeft")]
        public int idLeft;
        [XmlAttribute("idRight")]
        public int idRight;
    }

    public class area
    {
        [XmlAttribute("id")]
        public int id; // integer
        [XmlAttribute("name")]
        public string name; //name="2_Company (6)"
        [XmlAttribute("category")]
        public string category; //  category="close"
        [XmlAttribute("type")]
        public string type; // type="Intermediate"
        [XmlElement("point")]
        public List<point> points;

        public area()
        {
            points = new List<point>();
        }
    }

    public class obstacle
    {
        [XmlAttribute("id")]
        public int id; // integer
        [XmlAttribute("name")]
        public string name; //name="Wall0" 
        [XmlAttribute("type")]
        public string type; // type="Wall" 2 points // Solid multiple points
        [XmlElement("point")]
        public List<point> points;

        public obstacle()
        {
            points = new List<point>();
        }
    }

    public class point
    {
        [XmlAttribute("x")]
        public double x;
        [XmlAttribute("y")]
        public double y;
    }
}

