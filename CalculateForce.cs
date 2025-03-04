using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal class CalculateForce
    {
        public int ID;           // Unique element ID
        public int StartNode;    // Index of start node
        public int EndNode;      // Index of end node
        public double YoungsModulus; // E
        public double Area;          // A
        public Point3d StartPoint;
        public Point3d EndPoint;

        public static List<double> ComputeElementForces(List<FEM_Element> elements, Vector<double> displacements, List<Point3d> globalNodes)
        {

            List<double> forces = new List<double>();

            foreach (var element in elements)
            {
                // Get global DOF indices
                int startIndex = element.StartNode * 3; // 3D system
                int endIndex = element.EndNode * 3;
                double L = globalNodes[element.StartNode].DistanceTo(globalNodes[element.EndNode]);

                // Extract displacements
                
                Vector<double> u_global = Vector<double>.Build.DenseOfArray(new double[]
                {
                    (globalNodes[element.StartNode].X + displacements[startIndex]) - globalNodes[element.StartNode].X,     // Ux start (A-node)
                    (globalNodes[element.StartNode].Y + displacements[startIndex + 1]) - globalNodes[element.StartNode].Y, // Uy start (A-node)
                    (globalNodes[element.StartNode].Z + displacements[startIndex + 2]) - globalNodes[element.StartNode].Z, // Uz start (A-node)
                    (globalNodes[element.EndNode].X + displacements[endIndex]) - globalNodes[element.EndNode].X,       // Ux end (B-node)
                    (globalNodes[element.EndNode].Y + displacements[endIndex + 1]) - globalNodes[element.EndNode].Y,   // Uy end (B-node)
                    (globalNodes[element.EndNode].Z + displacements[endIndex + 2]) - globalNodes[element.EndNode].Z   // Uz end (B-node)
                });
                

                Matrix<double> RR = Rotate_Matrix3D.RotateMatrix3D(globalNodes[element.StartNode], globalNodes[element.EndNode]);

                // Convert to local displacements
                Vector<double> u_local = RR * u_global;

                // Compute force: F = (EA/L) * (u_end - u_start)
                double force = (element.YoungsModulus * element.Area / L) * (u_local[3] - u_local[0]);

                forces.Add(force);
            }

            return forces; //Vector<double>.Build.DenseOfEnumerable(forces);
        }

    }
}
