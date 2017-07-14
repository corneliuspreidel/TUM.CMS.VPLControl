using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using MIConvexHull;
using QL4BIMspatial;
using Xbim.Common.Geometry;
using Xbim.Common.XbimExtensions;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public static class GeometryHandler
    {
        public static MeshGeometry3D WriteTriangles(IIfcProduct ifcElement, Xbim3DModelContext context, XbimMatrix3D wcsTransformation)
        {
            MeshBuilder meshBuilder = new MeshBuilder(false, false);

            // var allTriangles = new List<Triangles>();
            // foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement).Where(x => x.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded))
            foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement).Where(x => x.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded))
            {
                XbimShapeGeometry geometry = context.ShapeGeometry(instance);
                var data = ((IXbimShapeGeometryData)geometry).ShapeData;
                using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        XbimShapeTriangulation mesh = reader.ReadShapeTriangulation();
                        mesh = mesh.Transform(instance.Transformation);
                        // WCS transforms
                        mesh = mesh.Transform(wcsTransformation);

                        foreach (XbimFaceTriangulation face in mesh.Faces)
                        {
                            var j = 0;
                            for (var i = 0; i < face.TriangleCount; i++)
                            {
                                int k = i + j;
                                var point1 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };
                                j++;
                                k = i + j;
                                var point2 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };
                                j++;
                                k = i + j;
                                var point3 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };

                                meshBuilder.AddTriangle(point1, point2, point3);

                            }
                        }
                    }
                }
            }
            return meshBuilder.ToMesh();
            // return allTriangles;
        }

        /// <summary>
        /// Including all Geometry Elements
        /// </summary>
        /// <param name="ifcElement"></param>
        /// <param name="context"></param>
        /// <param name="wcsTransformation"></param>
        /// <returns></returns>
        public static MeshGeometry3D WriteAllTriangles(IIfcProduct ifcElement, Xbim3DModelContext context, XbimMatrix3D wcsTransformation)
        {
            MeshBuilder meshBuilder = new MeshBuilder(false, false);

            // var allTriangles = new List<Triangles>();
            foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement))
            {
                XbimShapeGeometry geometry = context.ShapeGeometry(instance);
                var data = ((IXbimShapeGeometryData)geometry).ShapeData;
                using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        XbimShapeTriangulation mesh = reader.ReadShapeTriangulation();
                        mesh = mesh.Transform(instance.Transformation);
                        // WCS transforms
                        mesh = mesh.Transform(wcsTransformation);

                        foreach (XbimFaceTriangulation face in mesh.Faces)
                        {
                            var j = 0;
                            for (var i = 0; i < face.TriangleCount; i++)
                            {
                                int k = i + j;
                                var point1 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };
                                j++;
                                k = i + j;
                                var point2 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };
                                j++;
                                k = i + j;
                                var point3 = new Point3D { X = mesh.Vertices[face.Indices[k]].X, Y = mesh.Vertices[face.Indices[k]].Y, Z = mesh.Vertices[face.Indices[k]].Z };

                                meshBuilder.AddTriangle(point1, point2, point3);
                            }
                        }
                    }
                }
            }
            return meshBuilder.ToMesh();
            // return allTriangles;
        }



        public static DiffuseMaterial GetStyleFromXbimModel(IIfcProduct item, Xbim3DModelContext context, double opacity = 1)
        {
            // var context = new Xbim3DModelContext(item.Model);
            // context.CreateContext();

            var productShape = context.ShapeInstancesOf(item)
                .Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded)
                .ToList();
            Material wpfMaterial = GetWpfMaterial(item.Model, productShape.Count > 0 ? productShape[0].StyleLabel : 0);

            Material newmaterial = wpfMaterial.Clone();
            ((DiffuseMaterial)newmaterial).Brush.Opacity = opacity;
            return newmaterial as DiffuseMaterial;
        }

        private static Material GetWpfMaterial(Xbim.Common.IModel model, int styleId)
        {
            var sStyle = model.Instances[styleId] as IIfcSurfaceStyle;
            var wpfMaterial = new WpfMaterial();

            if (sStyle != null)
            {
                XbimTexture texture = XbimTexture.Create(sStyle);
                texture.DefinedObjectId = styleId;
                wpfMaterial.CreateMaterial(texture);

                return wpfMaterial;
            }
            MaterialDictionary defaultMaterial = ModelDataProvider.DefaultMaterials;

            Material material;
            if (defaultMaterial.TryGetValue(model.GetType().Name, out material))
            {
                return material;
            }
            var color = new XbimColour("black", 1, 1, 1);
            wpfMaterial.CreateMaterial(color);
            return wpfMaterial;
        }

        public static List<Point3D> GetFootPrintNonBREP(IIfcProduct ifcElement, Xbim3DModelContext context)
        {
            var resultList = new List<Point3D>();

            foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement))
            {
                // Get the IFC Representation of the Footprint of the instance 
                XbimShapeGeometry geometry = context.ShapeGeometry(instance);

                var data = ((IXbimShapeGeometryData)geometry).ShapeData;
                using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        XbimShapeTriangulation mesh = reader.ReadShapeTriangulation();
                        mesh = mesh.Transform(instance.Transformation);

                        // find the minimal z coordinate
                        double minZ = 10000;
                        foreach (var vertex in mesh.Vertices)
                        {
                            if (vertex.Z <= minZ)
                                minZ = vertex.Z;
                        }
                        List<IVertex> points = new List<IVertex>();

                        foreach (var vertex in mesh.Vertices)
                        {
                            if (vertex.Z != minZ)
                                continue;

                            points.Add(new DefaultVertex { Position = new[] { vertex.X, vertex.Y } });
                        }

                        if (points.Count <= 2)
                            return null;

                        // Compute ConvexHull
                        var cH = ConvexHull.Create(points);

                        foreach (var item in cH.Points)
                        {
                            var point = new Point3D() { X = item.Position[0], Y = item.Position[1], Z = minZ };
                            bool duplicate = false;
                            foreach (var result in resultList)
                            {
                                if (result == point)
                                {
                                    duplicate = true;
                                    break;
                                }
                            }
                            if (!duplicate)
                                resultList.Add(point);
                        }

                    }
                }
            }
            return resultList;
        }

        /// <summary>
        /// This is only available for BREP Geometry
        /// </summary>
        /// <param name="ifcElement"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<Point3D> GetFootPrint(IIfcProduct ifcElement, Xbim3DModelContext context)
        {
            var resultList = new List<Point3D>();

            double minZ = 10000;

            if (ifcElement.Representation == null)
                return null;
            if (ifcElement.Representation.Representations.FirstOrDefault().Items.FirstOrDefault().GetType().Name != @"IfcFacetedBrep")
                return null;

            var faces =
                (ifcElement.Representation.Representations.FirstOrDefault().Items.FirstOrDefault() as IIfcFacetedBrep)
                    .Outer.CfsFaces;

            foreach (var face in faces)
            {
                var poly = (face.Bounds.FirstOrDefault().Bound as IIfcPolyLoop).Polygon;

                // Check the normal Vector

                var normalVector = Vector3D.CrossProduct(new Vector3D(poly[1].X - poly[0].X, poly[1].Y - poly[0].Y, poly[1].Z - poly[0].Z), new Vector3D(poly[2].X - poly[0].X, poly[2].Y - poly[0].Y, poly[2].Z - poly[0].Z));
                
                if (normalVector.X == 0 && normalVector.Y == 0 && normalVector.Z != 0)
                    ;
                else
                    continue;

                // Take the minz Coordinates

                foreach (var point in poly)
                {
                    resultList.Add(new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]));

                    if (point.Z <= minZ)
                        minZ = point.Z;
                    resultList.Add(new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]));
                }
            }

            var distinctResult = new List<Point3D>();
            foreach (var item in resultList)
            {
                if(item.Z == minZ)
                    distinctResult.Add(item);
            }

            return distinctResult;

        }


        /// <summary>
        /// QL4BIM GeometryOperation Function
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="xModelContext"></param>
        /// <returns></returns>
        public static List<TriangleMesh> CreateTriangleMeshes(List<IIfcProduct> elements, Xbim3DModelContext xModelContext)
        {
            var resList = new List<TriangleMesh>();
            foreach (var item in elements)
            {
               //  var mesh = GeometryHandler.WriteTriangles(item, xModelContext, new XbimMatrix3D())

                var mesh = GeometryHandler.WriteAllTriangles(item, xModelContext, new XbimMatrix3D());


                // Transformation 
                // var vertices = jObject.SelectToken("vertices").ToList().Select(value => value.Value<double>()).ToArray();
                // var indices = jObject.SelectToken("faces").ToList().Select(value => value.Value<int>()).ToArray();
                // var faceSet = new IndexedFaceSet(item.Id.ToString(), indices, vertices);

                var vertices = new List<Tuple<double, double, double>>();

                // var posList = new List<double>();
                foreach (var pos in mesh.Positions)
                {

                    vertices.Add(new Tuple<double, double, double>(pos.X, pos.Y, pos.Z));
                    // posList.Add(pos.X);
                    // posList.Add(pos.Y);
                    // posList.Add(pos.Z);
                }

                var indices = new List<Tuple<int, int, int>>();
                for (int i = 0; i < mesh.TriangleIndices.Count; i = i + 3)
                {
                    indices.Add(new Tuple<int, int, int>(mesh.TriangleIndices[i], mesh.TriangleIndices[i + 1], mesh.TriangleIndices[i + 2]));
                }

                if (vertices.Count == 0 || indices.Count == 0)
                    continue; 

                var faceSet = new IndexedFaceSet(vertices.ToArray(), indices.ToArray(), item.GlobalId.ToString(), 1);

                // var faceSet = new IndexedFaceSet(item.GlobalId.ToString(), mesh.TriangleIndices.ToArray(), posList.ToArray());
                resList.Add(faceSet.CreateMesh());
            }

            return resList;
        }

    }
}
