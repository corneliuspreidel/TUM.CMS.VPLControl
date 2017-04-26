using System.ComponentModel.Composition;
using TUM.CMS.VplControl.BimPlus.Geometry;

namespace TUM.CMS.VplControl.BimPlus.GeometryUtilities
{
    public class BoxIntersectionTester : IIntersectionTester<Box>
    {
        private readonly IIntersectionTester<Interval> intervalIntersectionTester;

        [ImportingConstructor]
        public BoxIntersectionTester(IIntersectionTester<Interval> intervalTester)
        {
            intervalIntersectionTester = intervalTester;
        }

        public bool TestStrict(Box first, Box second)
        {
            return (intervalIntersectionTester.TestStrict(first.X, second.X))
                && (intervalIntersectionTester.TestStrict(first.Y, second.Y))
                && (intervalIntersectionTester.TestStrict(first.Z, second.Z));
        }


        public bool Test(Box first, Box second)
        {
            return (intervalIntersectionTester.Test(first.X, second.X))
                && (intervalIntersectionTester.Test(first.Y, second.Y))
                && (intervalIntersectionTester.Test(first.Z, second.Z));
        }
    }
}
