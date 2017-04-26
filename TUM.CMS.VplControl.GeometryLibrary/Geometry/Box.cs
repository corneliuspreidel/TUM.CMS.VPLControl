using System;
using System.Collections.Generic;
using System.Linq;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public class Box : IVolume
    {
        public Box(Point point1, Point point2)
        {
            X = new Interval(point1.X, point2.X);
            Y = new Interval(point1.Y, point2.Y);
            Z = new Interval(point1.Z, point2.Z);

            PopulateFields();
        }

        public Box(Interval x, Interval y, Interval z)
        {
            X = x;
            Y = y;
            Z = z;

            PopulateFields();
        }

        public Box(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            X = new Interval(minX, maxX);
            Y = new Interval(minY, maxY);
            Z = new Interval(minZ, maxZ);

            PopulateFields();
        }

        public Interval X { get; set; }
        public Interval Y { get; set; }
        public Interval Z { get; set; }
        public IEnumerable<Interval> Extents
        {
            get
            {
                yield return X;
                yield return Y;
                yield return Z;
            }
        }

        public Interval this[Axis dim]
        {
            get
            {
                if (dim == Axis.X)
                    return X;
                if (dim == Axis.Y)
                    return Y;
                if (dim == Axis.Z)
                    return Z;

                throw new IndexOutOfRangeException();
            }
        }

        public Point Center { get; private set; }

        public double Volume { get; private set; }

        public double Surface { get; private set; }

        public Box Bounds => this;

        public static Box Union(IEnumerable<Box> boxes)
        {
            var enumerable = boxes as Box[] ?? boxes.ToArray();
            return new Box(Interval.Union(enumerable.Select(b => b.X)), Interval.Union(enumerable.Select(b => b.Y)),
                Interval.Union(enumerable.Select(b => b.Z)));
        }

        public static Box Union(params Box[] boxes)
        {
            return Union(boxes.AsEnumerable());
        }

        public static Box Intersection(IEnumerable<Box> boxes)
        {
            var enumerable = boxes as Box[] ?? boxes.ToArray();
            var x = Interval.Intersection(enumerable.Select(b => b.X));
            if (!(x.Length > 0)) return new Box(Interval.Union(0), Interval.Union(0), Interval.Union(0));
            {
                var y = Interval.Intersection(enumerable.Select(b => b.Y));
                if (!(y.Length > 0)) return new Box(Interval.Union(0), Interval.Union(0), Interval.Union(0));
                {
                    var z = Interval.Intersection(enumerable.Select(b => b.Z));
                    if (z.Length > 0)
                        return new Box(x, y, z);
                }
            }
            return new Box(Interval.Union(0), Interval.Union(0), Interval.Union(0));
        }

        public static Box Intersection(params Box[] boxes)
        {
            return Intersection(boxes.AsEnumerable());
        }

        private void PopulateFields()
        {
            Volume = X.Length*Y.Length*Z.Length;
            Surface = 2*(X.Length*Y.Length + X.Length*Z.Length + Y.Length*Z.Length);
            Center = new Point(X.Min + X.Length/2, Y.Min + Y.Length/2, Z.Min + Z.Length/2);
        }
    }
}