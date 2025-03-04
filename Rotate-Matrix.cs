using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal class Rotate_Matrix
    {
        public static Matrix<double> RotateMatrix(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            Matrix<double> rotateMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
        {
        {  c*c,  c*s,  0,  0 },
        { c*s,   s*s,  0,  0 },
        { 0, 0,   c*c,   c*s },
        { 0, 0,   c*s,   s*s }
        }                                                                   );

            return rotateMatrix;
        }
    }
}
