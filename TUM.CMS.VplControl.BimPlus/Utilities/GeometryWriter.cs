
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BimPlus.Sdk.Data.DbCore;
using BimPlus.Sdk.Data.Geometry;
using Color = System.Drawing.Color;

namespace TUM.CMS.VplControl.BimPlus.Utilities
{
    public static class GeometryWriter
    {
        /// <summary>
        ///     Parsing the THREEJS Geometry of an Object ... (including the color ...)
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="model"></param>
        /// <param name="color"></param>
        public static void EvaluateAndSaveGeometry(DbGeometry geo, DtObject model, Color color)
        {
            // Create 3DPoints
            var splitpointlist = geo.Vertices
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 3)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            var points = splitpointlist.Select(item => new Point3D(item[0], item[1], item[2])).ToList();

            // Add the information as AttributeGroup
            model.AttributeGroups["geometry"].AddProperty("threejspoints", points);
            model.AttributeGroups["geometry"].AddProperty("geometryindices", geo.Faces);
            model.AttributeGroups["geometry"].AddProperty("color", color);
        }
    }
}
