using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public class Ray
    {
        public Ray(Point start, Vector<double> direction)
        {
            Start = start;
            Direction = direction;
        }
        public Ray(Point start, Axis axis, AxisDirection dir)
        {
            Start = start;

            var val = dir == AxisDirection.Positive ? 1 : -1;
            Direction = DenseVector.Create(3, i => (Axis) i == axis ? val : 0);
        }
        public Point Start { get; private set; }
        public Vector<double> Direction { get; private set; }

    }
}