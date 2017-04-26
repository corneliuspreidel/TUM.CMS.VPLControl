using System.ComponentModel.Composition;

namespace TUM.CMS.VplControl.BimPlus.GeometryUtilities
{
    [InheritedExport]
    public interface IIntersectionTester<in TFirst, in TSecond>
    {
        bool TestStrict(TFirst first, TSecond second);
        bool Test(TFirst first, TSecond second);
    }

    [InheritedExport]
    public interface IIntersectionTester<in T> : IIntersectionTester<T, T>
    {
    }
}
