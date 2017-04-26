using System.ComponentModel.Composition;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using TUM.CMS.VplControl.BimPlus.Geometry;

namespace TUM.CMS.VplControl.BimPlus.GeometryUtilities
{
    public class TriangleIntersectionTester : IIntersectionTester<Triangle>
    {
        private readonly IIntersectionTester<Interval> _intervalIntersectionTester;

        [ImportingConstructor]
        public TriangleIntersectionTester(IIntersectionTester<Interval> intervalTester)
        {
            _intervalIntersectionTester = intervalTester;
        }

        public bool TestStrict(Triangle first, Triangle second)
        {
            var axes = new[] { first.Normal, second.Normal }.AsEnumerable();

            var secondStage = false;

            while (!secondStage)
            {
                if ((from axis in axes let triProj1 = Interval.Union(first.Points.Select(p => p.Vector * axis)) let triProj2 = Interval.Union(second.Points.Select(p => p.Vector * axis)) where !_intervalIntersectionTester.Test(triProj1, triProj2) select triProj1).Any())
                {
                    return false;
                }

                if (secondStage)
                    break;
                else
                    secondStage = true;

                // since the normals are no separating axes two possible cases remain depending on whether the triangles are coplanar

                // test whether the normal vectors are parallel
                if (Triangle.CrossProduct(first.Normal as Vector, second.Normal as Vector).AbsoluteMaximum() == 0)
                {
                    // if so, the triangles are coplanar since neither of the normal vectors is separating axis
                    // therefore only the edge normals in the triangles' plane are possible separating axes
                    axes = first.Edges.Select(e => Triangle.CrossProduct(e as Vector, first.Normal as Vector)).Concat(second.Edges.Select(e => Triangle.CrossProduct(e as Vector, second.Normal as Vector)));
                }
                else
                {
                    // if the triangles are not coplanar, the cross products between the triangles' edges are possible separating axes
                    axes = first.Edges.SelectMany(e1 => second.Edges.Select(e2 => Triangle.CrossProduct(e1 as Vector, e2 as Vector)));
                }
            }

            // no separating axis found
            return true;
        }


        public bool Test(Triangle first, Triangle second)
        {
            var axes = new[] { first.Normal, second.Normal }.AsEnumerable();

            bool secondStage = false;

            while (!secondStage)
            {
                foreach (var axis in axes)
                {
                    var triProj1 = Interval.Union(first.Points.Select(p => p.Vector * axis));
                    var triProj2 = Interval.Union(second.Points.Select(p => p.Vector * axis));

                    if (!_intervalIntersectionTester.TestStrict(triProj1, triProj2))
                        return false;
                }

                if (secondStage)
                    break;
                else
                    secondStage = true;

                // test whether the normal vectors are parallel
                if (Triangle.CrossProduct(first.Normal as Vector, second.Normal as Vector).AbsoluteMaximum() == 0)
                {
                    axes = first.Edges.Select(e => Triangle.CrossProduct(e as Vector, first.Normal as Vector)).Concat(second.Edges.Select(e => Triangle.CrossProduct(e as Vector, second.Normal as Vector)));
                }
                else
                {
                    // if the triangles are not coplanar, the cross products between the triangles' edges are possible separating axes
                    axes = first.Edges.SelectMany(e1 => second.Edges.Select(e2 => Triangle.CrossProduct(e1 as Vector, e2 as Vector)));
                }
            }
            return true;
        }
    }
}
