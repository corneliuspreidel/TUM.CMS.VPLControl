using System;
using System.Windows.Media.Media3D;

namespace TUM.CMS.VplControl.BimPlus.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class MeshIdandGeometry
    {
        public Guid Id { get; set; }
        public GeometryModel3D Model3D { get; set; }
        public Material Material { get; set; }
    }
}
