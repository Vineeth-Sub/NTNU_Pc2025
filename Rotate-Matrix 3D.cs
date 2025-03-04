using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Rhino.Geometry;

namespace NTNU_Pc2025
{



    /*
    internal static class VectorExtensions
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
    */
    internal class Rotate_Matrix3D
    {

        public int Start;    // Index of start node
        public int End;      // Index of end node
        public Matrix<double> LocalStiffnessMatrix { get; private set; }
        public Point3d StartPoint;
        public Point3d EndPoint;
        public double Angle;



        public static Matrix<double> RotateMatrix3D(Point3d StartPoint, Point3d EndPoint)
        {
            /*
            double L = StartPoint.DistanceTo(EndPoint);
            double cosx = (EndPoint.X - StartPoint.X) / L;
            double cosy = (EndPoint.Y - StartPoint.Y) / L;
            double cosz = (EndPoint.Z - StartPoint.Z) / L;
            
            Matrix<double> RR = Matrix<double>.Build.DenseOfArray(new double[,]
       {
           { cosx, cosy, cosz, 0, 0, 0 },
           { cosx, cosy, cosz, 0, 0, 0 },
            { cosx, cosy, cosz, 0, 0, 0 },
           { 0, 0, 0, cosx, cosy, cosz },
            { 0, 0, 0, cosx, cosy, cosz },
            { 0, 0, 0, cosx, cosy, cosz }
       });
            */

            
            double L = StartPoint.DistanceTo(EndPoint);
            double lx = (EndPoint.X - StartPoint.X) / L;
            double ly = (EndPoint.Y - StartPoint.Y) / L;
            double lz = (EndPoint.Z - StartPoint.Z) / L;

            Vector<double> v = Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 1 });
            if (Math.Abs(lx) < 1e-6 && Math.Abs(ly) < 1e-6) 
                v = Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0 });

            // Compute local y-axis (perpendicular to both v and element axis)
            Vector<double> ex = Vector<double>.Build.DenseOfArray(new double[] { lx, ly, lz });
            Vector<double> ey = v.CrossProducts(ex).Normalize(2);
            Vector<double> ez = ex.CrossProducts(ey); // Ensure right-hand rule

            // Create 3×3 rotation matrix
            Matrix<double> R = DenseMatrix.OfArray(new double[,]
            {
                { ex[0], ex[1], ex[2] },
                { ey[0], ey[1], ey[2] },
                { ez[0], ez[1], ez[2] }
            });

            // Construct 6×6 transformation matrix
            Matrix<double> RR = DenseMatrix.OfArray(new double[6, 6]);

            // Fill the diagonal blocks with R
            RR.SetSubMatrix(0, 0, R);
            RR.SetSubMatrix(3, 3, R);

            




            return RR;///UNSURE ABOUT THIS ROTATION MATRIX

        }
    }
}



/*
 double cosxz = Math.Sqrt(Math.Pow(cosx, 2) + Math.Pow(cosz, 2));


            if (cosxz < 1e-10) // Avoid division by zero
                cosxz = 1e-10;

            Matrix<double> RR;

            if (Math.Abs(cosx) < 1e-10 && Math.Abs(cosz) < 1e-10)
            {
                RR = DenseMatrix.OfArray(new double[,] {
                    { 0, cosy, 0 },
                    { cosy, 0, 1 },
                    { cosy, 0, -1 }
                });
            }
            else
            {
                RR = DenseMatrix.OfArray(new double[,] {
                    { cosx, cosy, cosz },
                    { (cosx * cosy - cosz) / cosxz, -cosxz, (cosy * cosz + cosx) / cosxz },
                    { (cosx * cosy + cosz) / cosxz, cosxz, (cosy * cosz - cosx) / cosxz }
                });
            }
 */