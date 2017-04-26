using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TUM.CMS.VplControl.BimPlus.Geometry
{

    public class Triangle : IPolygon
    {
        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;

            PopulateFields();
        }

        public Point A { get; }
        public Point B { get; }
        public Point C { get; }
        public Vector<double> AB { get; private set; }
        public Vector<double> AC { get; private set; }
        public Vector<double> BC { get; private set; }
        public IEnumerable<Vector<double>> Edges
        {
            get
            {
                PopulateFields();
                yield return AB;
                yield return AC;
                yield return BC;
            }
        }
        public Vector<double> Normal { get; private set; }
        public IEnumerable<Point> Points
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }
        public double Area { get; private set; }
        public Box Bounds { get; private set; }

        private void PopulateFields()
        {
            AB = B.Vector - A.Vector;
            AC = C.Vector - A.Vector;
            BC = C.Vector - B.Vector;

            Normal = CrossProduct(AC as Vector, AB as Vector);
            Area = Normal.Norm(2);
            Normal = Normal.Normalize(2);

            if (Bounds == null)
                Bounds = new Box(Interval.Union(Points.Select(p => p.X)), Interval.Union(Points.Select(p => p.Y)),
                    Interval.Union(Points.Select(p => p.Z)));
            else
            {
                Bounds.X = Interval.Union(Points.Select(p => p.X));
                Bounds.Y = Interval.Union(Points.Select(p => p.Y));
                Bounds.Z = Interval.Union(Points.Select(p => p.Z));
            }
        }

        public static Vector CrossProduct(Vector left, Vector right)
        {
            if ((left.Count != 3 || right.Count != 3))
            {
                var message = "Vectors must have a length of 3.";
                throw new Exception(message);
            }
            Vector result = new DenseVector(3);
            result[0] = left[1] * right[2] - left[2] * right[1];
            result[1] = -left[0] * right[2] + left[2] * right[0];
            result[2] = left[0] * right[1] - left[1] * right[0];

            return result;
        }
    }
}