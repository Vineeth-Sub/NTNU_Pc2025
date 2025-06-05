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
        public double M;
        public double rho;


        public FEM_Element(List<Point3d> globalNodes, int startNode, int endNode, double E, double A, double rho)
        {

            if (globalNodes == null || globalNodes.Count == 0 ||
    startNode < 0 || endNode < 0 ||
    startNode >= globalNodes.Count || endNode >= globalNodes.Count)
            {
                // Don't throw, just exit cleanly
                LocalStiffnessMatrix = null;
                M = 0;
                return;
            }

            StartNode = startNode;
            EndNode = endNode;
            YoungsModulus = E;
            Area = A;
            globalNodes = globalNodes;
            LocalStiffnessMatrix = ComputeLocalStiffness3D(globalNodes ,startNode, endNode, E, A);
            rho = rho;
            double L = globalNodes[startNode].DistanceTo(globalNodes[endNode]);

            if (L < 1e-6)  // avoid divide-by-zero
            {
                LocalStiffnessMatrix = null;
                M = 0;
                return;
            }

            double m = rho * A * L;
            M = m;
        }

        public FEM_Element(int startNode, int endNode, double E, double A,double rho, List<Point3d> globalNodes, double angle)
        {

            if (globalNodes == null || globalNodes.Count == 0 ||
startNode < 0 || endNode < 0 ||
startNode >= globalNodes.Count || endNode >= globalNodes.Count)
            {
                // Don't throw, just exit cleanly
                LocalStiffnessMatrix = null;
                M = 0;
                return;
            }

            StartNode = startNode;
            EndNode = endNode;
            YoungsModulus = E;
            Area = A;
            Angle = angle;
            LocalStiffnessMatrix = ComputeLocalStiffness(globalNodes, angle);
            double L = globalNodes[startNode].DistanceTo(globalNodes[endNode]);

            if (L < 1e-6)  // avoid divide-by-zero
            {
                LocalStiffnessMatrix = null;
                M = 0;
                return;
            }

            double m = rho * A * L;
            M = m;

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
            double k = EA_L; //WHYYY
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);

            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
                {  EA_L, 0, -EA_L, 0 },
               {  0, 0, 0, 0 },
               {  -EA_L, 0, EA_L, 0 },
               {  0, 0, 0, 0 },
                                                                                       });

            Matrix<double> K_global = DenseMatrix.OfArray(new double[,]
        {
            {  k * c * c,  k * c * s, -k * c * c, -k * c * s },
            {  k * c * s,  k * s * s, -k * c * s, -k * s * s },
            { -k * c * c, -k * c * s,  k * c * c,  k * c * s },
            { -k * c * s, -k * s * s,  k * c * s,  k * s * s }
                    });

            Matrix<double> rotationMatrix = Rotate_Matrix.RotateMatrix(angle);
            //return rotationMatrix * kLocal * rotationMatrix.Transpose();
            return K_global;
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

            /*
            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            { EA_L, 0, 0, -EA_L, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { -EA_L, 0, 0, EA_L, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
              */

            Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            { EA_L, 0, 0, -EA_L, 0, 0 },
            { 0, EA_L, 0, 0, -EA_L, 0 },
            { 0, 0, EA_L, 0, 0, -EA_L },
            { -EA_L, 0, 0, EA_L, 0, 0 },
            { 0, -EA_L, 0, 0, EA_L, 0 },
            { 0, 0, -EA_L, 0, 0, EA_L }
                                                                                     });

            var l = (end.X - start.X) / L;
            var m = (end.Y - start.Y) / L;
            var n = (end.Z - start.Z) / L;


            Matrix<double> kg1 = Matrix<double>.Build.DenseOfArray(new double[,] {
            {  l*l,  l*m,  l*n, -l*l, -l*m, -l*n },
            {  l*m,  m*m,  m*n, -l*m, -m*m, -m*n },
            {  l*n,  m*n,  n*n, -l*n, -m*n, -n*n },
            { -l*l, -l*m, -l*n,  l*l,  l*m,  l*n },
            { -l*m, -m*m, -m*n,  l*m,  m*m,  m*n },
            { -l*n, -m*n, -n*n,  l*n,  m*n,  n*n }
            });

            kg1 = kg1 * EA_L;

            Matrix<double> T1 = Rotate_Matrix3D.RotateMatrix3D(start, end);
            Matrix<double> T2 = Rotate_Matrix3D.RotateMatrix3D(start, end);
            T2 = T2.Transpose();
            Matrix<double> kGlobal = T2.Multiply(kLocal);
            var kg = kGlobal.Multiply(T1);
            return kg1;

        }

    }
}