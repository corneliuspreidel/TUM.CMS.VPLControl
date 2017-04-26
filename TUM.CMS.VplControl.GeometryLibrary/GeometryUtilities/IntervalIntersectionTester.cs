using TUM.CMS.VplControl.BimPlus.Geometry;

namespace TUM.CMS.VplControl.BimPlus.GeometryUtilities
{
    public class IntervalIntersectionTester : IIntersectionTester<Interval>
    {
        public bool TestStrict(Interval first, Interval second)
        {
            return (first.Min < second.Max && second.Min < first.Max);
        }

        public bool Test(Interval first, Interval second)
        {
            return (first.Min <= second.Max && second.Min <= first.Max);
        }
    }
}
