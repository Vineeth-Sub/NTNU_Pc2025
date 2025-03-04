using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel;


namespace NTNU_Pc2025
{
    internal class FEM_Element
    {
        public int ID;           // Unique element ID
        public int StartNode;    // Index of start node
        public int EndNode;      // Index of end node
        public double YoungsModulus; // E
        public double Area;          // A
        public Matrix<double> LocalStiffnessMatrix { get; private set; }
        public Point3d StartPoint;
        public Point3d EndPoint;
        public double Angle;



        public FEM_Element(List<Point3d> globalNodes, int startNode, int endNode, double E, double A)
        {
            StartNode = startNode;
            EndNode = endNode;
            YoungsModulus = E;
            Area = A;
            globalNodes = globalNodes;
            LocalStiffnessMatrix = ComputeLocalStiffness3D(globalNodes ,startNode, endNode, E, A);
        }

        public FEM_Element(int startNode, int endNode, double E, double A, List<Point3d> globalNodes, double angle)
        {
            StartNode = startNode;
            EndNode = endNode;
            YoungsModulus = E;
            Area = A;
            Angle = angle;
            LocalStiffnessMatrix = ComputeLocalStiffness(globalNodes, angle);

        }

        public FEM_Element(int startNode, int endNode, Matrix<double> localStiffnessMatrix, double angle)
        {
            StartNode = startNode;
            EndNode = endNode;
            Angle = angle;
            LocalStiffnessMatrix = ComputeLocalStiffness2(localStiffnessMatrix, angle);

        }

        private Matrix<double> ComputeLocalStiffness(List<Point3d> globalNodes, double angle)
        {
            Point3d start = globalNodes[StartNode];
            Point3d end = globalNodes[EndNode];

            double L = start.DistanceTo(end);  // Compute length of bar

            double EA_L = (YoungsModulus * Area) / L;

            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
                {  EA_L, 0, -EA_L, 0 },
               {  0, 0, 0, 0 },
               {  -EA_L, 0, EA_L, 0 },
               {  0, 0, 0, 0 },
                                                                                       });
            Matrix<double> rotationMatrix = Rotate_Matrix.RotateMatrix(angle);
            return rotationMatrix * kLocal * rotationMatrix.Transpose();
        }

        private Matrix<double> ComputeLocalStiffness2(Matrix<double> kmatrix, double angle)
        {
            Matrix<double> rotationMatrix = Rotate_Matrix.RotateMatrix(angle);
            return rotationMatrix.Transpose() * kmatrix * rotationMatrix;
        }

        private Matrix<double> ComputeLocalStiffness3D(List<Point3d> globalNodes, int startNode, int endNode, double E, double A)
        {
            Point3d start = globalNodes[StartNode];
            Point3d end = globalNodes[EndNode];

            double L = start.DistanceTo(end);  // Compute length of bar

            double EA_L = (E * A) / L;

            /*
            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            { EA_L, 0, 0, -EA_L, 0, 0 },
            { 0, EA_L, 0, 0, -EA_L, 0 },
            { 0, 0, EA_L, 0, 0, -EA_L },
            { -EA_L, 0, 0, EA_L, 0, 0 },
            { 0, -EA_L, 0, 0, EA_L, 0 },
            { 0, 0, -EA_L, 0, 0, EA_L }
            */


            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            { EA_L, 0, 0, -EA_L, 0, 0 },
            { 0, EA_L, 0, 0, -EA_L, 0 },
            { 0, 0, EA_L, 0, 0, -EA_L },
            { -EA_L, 0, 0, EA_L, 0, 0 },
            { 0, -EA_L, 0, 0, EA_L, 0 },
            { 0, 0, -EA_L, 0, 0, EA_L }
                                                                                       });
            Matrix<double> rotationMatrix3D = Rotate_Matrix3D.RotateMatrix3D(start, end);
            Matrix<double> kGlobal = rotationMatrix3D.Transpose() * kLocal * rotationMatrix3D;

            return kGlobal;

        }

    }
}