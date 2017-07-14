using System.Collections.Generic;
using Microsoft.Practices.Unity;
using QL4BIMspatial;

namespace TUM.CMS.VplControl.Utilities.Geometry
{
    public static class SpatialOperators
    {
        // QL4BIM Inits
        private static UnityContainer container = new UnityContainer();
        // private static MainInterface ql4Spatial = new MainInterface(container);

        public static Dictionary<string, string> SpatialOperator(List<GeometryObject> objects1, List<GeometryObject> objects2)
        {
            var list_1 = CreateTriangleMeshes(objects1);
            var list_2 = CreateTriangleMeshes(objects2);

            // Init the Settings for the operators
            var settings = container.Resolve<ISettings>();
            settings.Direction.RaysPerSquareMeter = 100;
            settings.Direction.PositiveOffset = 100;
            // The mapping of the operators is because of the transformation of the model

            var resCollection = new Dictionary<string, string>();

            // Init the Operator 
            var op = container.Resolve<IOverlapOperator>();
            foreach (var item1 in list_1)
            {
                foreach (var item2 in list_2)
                {
                    var abs = op.Overlap(item1, item2);
                    if (abs)
                        resCollection.Add(item1.Name, item2.Name);
                }
            }

            return resCollection;
        }

        private static TriangleMesh CreateTriangleMeshes(string id, double[] vertices, int[] indices)
        {
            var faceSet = new IndexedFaceSet(id, indices, vertices);
            return faceSet.CreateMesh();
        }

        private static List<TriangleMesh> CreateTriangleMeshes(List<GeometryObject> objects)
        {
            var resList = new List<TriangleMesh>();
            foreach (var item in objects)
            {
                resList.Add(CreateTriangleMeshes(item.id, item.vertices, item.indices));
            }
            return resList;
        }

    }

    public class GeometryObject
    {
        public string id;
        public double[] vertices;
        public int[] indices;
    }
}
