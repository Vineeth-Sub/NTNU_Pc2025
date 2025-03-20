using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NTNU_Pc2025
{
    internal class ComputeKLocal
    {
        public static Matrix<double> CalculateKLocal(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double Iy, double Iz, double G, double J)
        {
            Point3d start = globalNodes[startNode];
            Point3d end = globalNodes[endNode];

            double L = start.DistanceTo(end);  // Compute length of bar

            double L2 = Math.Pow(L, 2);
            double L3 = Math.Pow(L, 3);

            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,]
            {
        { E*A/L, 0, 0, 0, 0, 0, -E*A/L, 0, 0, 0, 0, 0 },
        { 0, 12*E*Iz/L3, 0, 0, 0, 6*E*Iz/L2, 0, -12*E*Iz/L3, 0, 0, 0, 6*E*Iz/L2 },
        { 0, 0, 12*E*Iy/L3, 0, -6*E*Iy/L2, 0, 0, 0, -12*E*Iy/L3, 0, -6*E*Iy/L2, 0 },
        { 0, 0, 0, G*J/L, 0, 0, 0, 0, 0, -G*J/L, 0, 0 },
        { 0, 0, -6*E*Iy/L2, 0, 4*E*Iy/L, 0, 0, 0, 6*E*Iy/L2, 0, 2*E*Iy/L, 0 },
        { 0, 6*E*Iz/L2, 0, 0, 0, 4*E*Iz/L, 0, -6*E*Iz/L2, 0, 0, 0, 2*E*Iz/L },
        { -E*A/L, 0, 0, 0, 0, 0, E*A/L, 0, 0, 0, 0, 0 },
        { 0, -12*E*Iz/L3, 0, 0, 0, -6*E*Iz/L2, 0, 12*E*Iz/L3, 0, 0, 0, -6*E*Iz/L2 },
        { 0, 0, -12*E*Iy/L3, 0, 6*E*Iy/L2, 0, 0, 0, 12*E*Iy/L3, 0, 6*E*Iy/L2, 0 },
        { 0, 0, 0, -G*J/L, 0, 0, 0, 0, 0, G*J/L, 0, 0 },
        { 0, 0, -6*E*Iy/L2, 0, 2*E*Iy/L, 0, 0, 0, 6*E*Iy/L2, 0, 4*E*Iy/L, 0 },
        { 0, 6*E*Iz/L2, 0, 0, 0, 2*E*Iz/L, 0, -6*E*Iz/L2, 0, 0, 0, 4*E*Iz/L }
            });

            return kLocal;

        }
    }
}
