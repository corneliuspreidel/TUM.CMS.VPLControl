using System;
using System.Collections.Generic;
using System.Linq;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{
    public class Interval
    {
        public Interval(double val1, double val2)
        {
            Min = Math.Min(val1, val2);
            Max = Math.Max(val1, val2);
            Length = Max - Min;
        }
        public double Min { get; }
        public double Max { get; }
        public double Length { get; private set; }
        public static Interval Union(IEnumerable<double> values)
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            foreach (var val in values)
            {
                min = Math.Min(min, val);
                max = Math.Max(max, val);
            }

            return new Interval(min, max);
        }
        public static Interval Union(params double[] values)
        {
            return Union(values.AsEnumerable());
        }
        public static Interval Union(IEnumerable<Interval> intervals)
        {
            var enumerable = intervals as Interval[] ?? intervals.ToArray();
            var first = enumerable.First();
            var min = first.Min;
            var max = first.Max;

            foreach (var inter in enumerable.Skip(1))
            {
                min = Math.Min(min, inter.Min);
                max = Math.Max(max, inter.Max);
            }

            return new Interval(min, max);
        }
        public static Interval Union(params Interval[] intervals)
        {
            return Union(intervals.AsEnumerable());
        }
        public static Interval Intersection(IEnumerable<Interval> intervals)
        {
            var enumerable = intervals as Interval[] ?? intervals.ToArray();
            var first = enumerable.First();
            var min = first.Min;
            var max = first.Max;

            foreach (var inter in enumerable.Skip(1))
            {
                min = Math.Max(min, inter.Min);
                max = Math.Min(max, inter.Max);

                // if max had become smaller than min, there was an interval not intersecting the others:
                if (max < min)
                    return new Interval(0, 0);
            }

            return new Interval(min, max);
        }
        public static Interval Intersection(params Interval[] intervals)
        {
            return Intersection(intervals.AsEnumerable());
        }
    }
}