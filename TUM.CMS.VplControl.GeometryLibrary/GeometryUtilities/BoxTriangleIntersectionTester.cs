using System;
using System.ComponentModel.Composition;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using TUM.CMS.VplControl.BimPlus.Geometry;

namespace TUM.CMS.VplControl.BimPlus.GeometryUtilities
{
    public class BoxTriangleIntersectionTester : IIntersectionTester<Box, Triangle>, IIntersectionTester<Triangle, Box>
    {
        private static readonly Vector<double>[] CartAxes;

        private readonly IIntersectionTester<Box> _boxIntersectionTester;

        static BoxTriangleIntersectionTester()
        {
            CartAxes = new Vector<double>[] { new DenseVector(new double[] { 1, 0, 0 }), new DenseVector(new double[] { 0, 1, 0 }), new DenseVector(new double[] { 0, 0, 1 }) };
        }

        [ImportingConstructor]
        public BoxTriangleIntersectionTester(IIntersectionTester<Box> boxTester)
        {
            _boxIntersectionTester = boxTester;
        }

        public bool TestStrict(Box box, Triangle tri)
        {
            if (!_boxIntersectionTester.TestStrict(box, tri.Bounds))
                return false;

            var boxCenter = box.Center.Vector;

            var triPoints = new[] { tri.A.Vector - boxCenter, tri.B.Vector - boxCenter, tri.C.Vector - boxCenter };
            var crossProducts = CartAxes.SelectMany(c => tri.Edges.Select(e => Triangle.CrossProduct(c as Vector,e as Vector)));
            var axes = (new[] { tri.Normal }).Concat(crossProducts);
            return !(from axis in axes let triProj = Interval.Union(triPoints.Select(p => p*axis)) let boxRadius = (box.X.Length*Math.Abs(axis[0]) + box.Y.Length*Math.Abs(axis[1]) + box.Z.Length*Math.Abs(axis[2]))/2 where triProj.Max <= -boxRadius || boxRadius <= triProj.Min select triProj).Any();
        }

        public bool Test(Box box, Triangle tri)
        {
            if (!_boxIntersectionTester.Test(box, tri.Bounds))
                return false;
            var boxCenter = box.Center.Vector;
            var triPoints = new[] { tri.A.Vector - boxCenter, tri.B.Vector - boxCenter, tri.C.Vector - boxCenter };
            var crossProducts = CartAxes.SelectMany(c => tri.Edges.Select(e => Triangle.CrossProduct(c as Vector, e as Vector)));
            var axes = (new[] { tri.Normal }).Concat(crossProducts);
            return !(from axis in axes let triProj = Interval.Union(triPoints.Select(p => p*axis)) let boxRadius = (box.X.Length*Math.Abs(axis[0]) + box.Y.Length*Math.Abs(axis[1]) + box.Z.Length*Math.Abs(axis[2]))/2 where triProj.Max < -boxRadius || boxRadius < triProj.Min select triProj).Any();
        }

        public bool TestStrict(Triangle first, Box second)
        {
            return TestStrict(second, first);
        }

        public bool Test(Triangle first, Box second)
        {
            return Test(second, first);
        }
    }
}
