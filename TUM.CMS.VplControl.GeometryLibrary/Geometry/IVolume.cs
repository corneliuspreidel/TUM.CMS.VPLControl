namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public interface IVolume : IHasBounds
    {
        double Volume { get; }
        double Surface { get; }
    }
}