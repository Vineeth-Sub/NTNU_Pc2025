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
    internal class FEM_Element_BEAM
    {
        public int StartNode;    // Index of start node
        public int EndNode;      // Index of end node
        public double E; // E
        public double A;          // A
        public double Iy;
        public double Iz;
        public double G;
        public double J;

        public Matrix<double> LocalStiffnessMatrix { get; private set; }
        public Point3d StartPoint;
        public Point3d EndPoint;


        public FEM_Element_BEAM(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double Iy, double Iz, double G, double J)
        {
            StartNode = startNode;
            EndNode = endNode;
            this.E = E;
            this.A = A;
            this.Iy = Iy;
            this.Iz = Iz;
            this.G = G;
            this.J = J;
            globalNodes = globalNodes;
            LocalStiffnessMatrix = ComputeLocalStiffness3DBEAM(globalNodes, startNode, endNode, E, A, Iy, Iz, G, J);
        }

        private Matrix<double> ComputeLocalStiffness3DBEAM(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double Iy, double Iz, double G, double J)
        {
            Point3d start = globalNodes[StartNode];
            Point3d end = globalNodes[EndNode];

            double L = start.DistanceTo(end);  // Compute length of bar

            double L2 = Math.Pow(L,2);
            double L3 = Math.Pow(L,3);

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

            Matrix<double> rotationMatrix3DBEAM = RotateMatrix3D_BEAM.RotateMatrix3DBEAM(start, end);
            return rotationMatrix3DBEAM.Transpose() * kLocal * rotationMatrix3DBEAM;

        }

    }
}
