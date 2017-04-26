using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public interface IPolygon : IHasBounds
    {
        IEnumerable<Point> Points { get; }
        IEnumerable<Vector<double>> Edges { get; }
        Vector<double> Normal { get; }
        double Area { get; }
    }
}