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
    internal class RotateMatrix3D_BEAM
    {
        public int Start;    // Index of start node
        public int End;      // Index of end node
        public Matrix<double> LocalStiffnessMatrix { get; private set; }
        public Point3d StartPoint;
        public Point3d EndPoint;
        public double Angle;


        public static Matrix<double> RotateMatrix3DBEAM(Point3d StartPoint, Point3d EndPoint)
        {
            
             double L = StartPoint.DistanceTo(EndPoint);
             double lx = (EndPoint.X - StartPoint.X) / L;
             double ly = (EndPoint.Y - StartPoint.Y) / L;
             double lz = (EndPoint.Z - StartPoint.Z) / L;

            
             Vector<double> v = Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 1 });
             if (Math.Abs(lx) < 1e-6 && Math.Abs(ly) < 1e-6)
                 v = Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0 });
            

            /*
            Vector<double> v = Math.Abs(lx) < 1e-6 && Math.Abs(lz) < 1e-6 ?
                Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0 }) :
                Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 1 });
            */


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
            //Matrix<double> RR = DenseMatrix.OfArray(new double[12, 12]);
            Matrix<double> RR = DenseMatrix.CreateIdentity(12);

             // Fill the diagonal blocks with R
             RR.SetSubMatrix(0, 0, R);
             RR.SetSubMatrix(3, 3, R);
             RR.SetSubMatrix(6, 6, R);
             RR.SetSubMatrix(9, 9, R);
            

            /*
            double L = StartPoint.DistanceTo(EndPoint);
            double cosx = (EndPoint.X - StartPoint.X) / L;
            double cosy = (EndPoint.Y - StartPoint.Y) / L;
            double cosz = (EndPoint.Z - StartPoint.Z) / L;

            double cosxz = Math.Sqrt(Math.Pow(cosx, 2) + Math.Pow(cosz, 2));
            double c = Math.Cos(0);
            double s = Math.Sin(0);

            Matrix<double> R;

            if ((EndPoint.X - StartPoint.X) == 0 && (EndPoint.Z - StartPoint.Z) == 0)
            {
                R = DenseMatrix.OfArray(new double[,]
                {
        {0, cosy, 0 },
        {cosy * c, 0, s },
        {cosy * s, 0, -c }
                });
            }
            else
            {
                R = DenseMatrix.OfArray(new double[,]
                {
        {cosx, cosy, cosz },
        {(cosx * cosy * c - cosz * s) / cosxz, -cosxz * c, (cosy * cosz * c + cosx * s) / cosxz},
        {(cosx * cosy * s + cosz * c) / cosxz, cosxz * s, (cosy * cosz * s - cosx * c) / cosxz}
                });
            }

            // Create a 12x12 matrix by stacking R on the diagonal
            Matrix<double> RR = DenseMatrix.OfArray(new double[12, 12]);

            // Fill the diagonal blocks with R
            RR.SetSubMatrix(0, 0, R);
            RR.SetSubMatrix(3, 3, R);
            RR.SetSubMatrix(6, 6, R);
            RR.SetSubMatrix(9, 9, R);

            // Return the 12x12 matrix
            */



            return RR;

        }

    }
}
