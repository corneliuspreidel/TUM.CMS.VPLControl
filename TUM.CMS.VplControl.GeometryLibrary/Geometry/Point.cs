using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public class Point
    {
        public Point(double x, double y, double z)
        {
            Vector = new DenseVector(new[] {x, y, z});
        }
        public Vector<double> Vector { get; }
        public double X => Vector[0];
        public double Y => Vector[1];
        public double Z => Vector[2];
    }
}