using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal static class Vector_crossproduct
    {
        public static Vector<double> CrossProducts(this Vector<double> a, Vector<double> b)
        {
            if (a.Count != 3 || b.Count != 3)
                throw new ArgumentException("Cross product is only defined for 3D vectors.");

            return Vector<double>.Build.DenseOfArray(new double[]
            {
                a[1] * b[2] - a[2] * b[1], // X
                a[2] * b[0] - a[0] * b[2], // Y
                a[0] * b[1] - a[1] * b[0]  // Z
            });
        }
    }
}
