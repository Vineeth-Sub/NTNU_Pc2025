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
        public double M;

        public Matrix<double> LocalStiffnessMatrix { get; private set; }
        public Point3d StartPoint;
        public Point3d EndPoint;


        public FEM_Element_BEAM(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double Iy, double Iz, double G, double J, double rho)
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
            double L = globalNodes[startNode].DistanceTo(globalNodes[endNode]);
            double m = rho * A * L;
            M = m;
        }

        private Matrix<double> ComputeLocalStiffness3DBEAM(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double Iy, double Iz, double G, double J)
        {
            Point3d start = globalNodes[StartNode];
            Point3d end = globalNodes[EndNode];

            double L = start.DistanceTo(end);  // Compute length of bar


            double l = (end.X - start.X) / L; //l
            double m = (end.Y - start.Y) / L; //m
            double n = (end.Z - start.Z) / L; //n

            double L2 = Math.Pow(L, 2);
            double L3 = Math.Pow(L, 3);

            // Compute stiffness values
            double K_axial = E * A / L;
            double K_bending_y = 12 * E * Iy / Math.Pow(L, 3);
            double K_bending_z = 12 * E * Iz / Math.Pow(L, 3);
            double K_torsion = G * J / L;


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

            // 12x12 Local Stiffness Matrix
            Matrix<double> k_Local = DenseMatrix.OfArray(new double[,]
            {
            { K_axial, 0, 0, 0, 0, 0, -K_axial, 0, 0, 0, 0, 0 },
            { 0, K_bending_y, 0, 0, 0, K_torsion, 0, -K_bending_y, 0, 0, 0, K_torsion },
            { 0, 0, K_bending_z, 0, -K_torsion, 0, 0, 0, -K_bending_z, 0, -K_torsion, 0 },
            { 0, 0, 0, K_torsion, 0, 0, 0, 0, 0, -K_torsion, 0, 0 },
            { 0, 0, -K_torsion, 0, K_bending_y, 0, 0, 0, K_torsion, 0, -K_bending_y, 0 },
            { 0, K_torsion, 0, 0, 0, K_bending_z, 0, -K_torsion, 0, 0, 0, -K_bending_z },
            { -K_axial, 0, 0, 0, 0, 0, K_axial, 0, 0, 0, 0, 0 },
            { 0, -K_bending_y, 0, 0, 0, -K_torsion, 0, K_bending_y, 0, 0, 0, -K_torsion },
            { 0, 0, -K_bending_z, 0, K_torsion, 0, 0, 0, K_bending_z, 0, K_torsion, 0 },
            { 0, 0, 0, -K_torsion, 0, 0, 0, 0, 0, K_torsion, 0, 0 },
            { 0, 0, K_torsion, 0, -K_bending_y, 0, 0, 0, -K_torsion, 0, K_bending_y, 0 },
            { 0, -K_torsion, 0, 0, 0, -K_bending_z, 0, K_torsion, 0, 0, 0, K_bending_z }
            });


            // Construct the explicit global stiffness matrix (12x12)
            Matrix<double> K_global = DenseMatrix.OfArray(new double[12, 12]);

            // Fill the explicit formula for the stiffness matrix
            K_global[0, 0] = K_axial * l * l + K_bending_y * m * m + K_bending_z * n * n;
            K_global[0, 1] = (K_axial - K_bending_y) * l * m;
            K_global[0, 2] = (K_axial - K_bending_z) * l * n;
            K_global[1, 1] = K_axial * m * m + K_bending_y * l * l + K_bending_z * n * n;
            K_global[1, 2] = (K_axial - K_bending_z) * m * n;
            K_global[2, 2] = K_axial * n * n + K_bending_y * l * l + K_bending_z * m * m;

            // Symmetric part (K_ij = K_ji)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    K_global[j, i] = K_global[i, j];
                }
            }

            // Axial & torsional components
            K_global[3, 3] = K_torsion;
            K_global[4, 4] = K_bending_y;
            K_global[5, 5] = K_bending_z;

            // Mirror for the second node
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    K_global[i + 6, j + 6] = K_global[i, j];
                    K_global[i, j + 6] = -K_global[i, j];
                    K_global[i + 6, j] = -K_global[i, j];
                }
            }

            //return K_global;

            Matrix<double> rotationMatrix3DBEAM = RotateMatrix3D_BEAM.RotateMatrix3DBEAM(start, end);
            return rotationMatrix3DBEAM.Transpose() * kLocal * rotationMatrix3DBEAM;

        }





       
        
        }

    }

