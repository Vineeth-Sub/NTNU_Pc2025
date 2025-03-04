using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace NTNU_Pc2025
{
    internal class CalculateUtilization_BEAM
    {
        public static List<BeamUtilization> ComputeUtilization(List<FEM_Element_BEAM> elements, List<Point3d> globalNodes,Vector<double> displacements, double E, double G, double fy)
        {
            List<BeamUtilization> results = new List<BeamUtilization>();

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                // Get nodal displacements
                int startIndex = element.StartNode * 6;
                int endIndex = element.EndNode * 6;

                Point3d start = globalNodes[element.StartNode];
                Point3d end = globalNodes[element.EndNode];

                double L = start.DistanceTo(end);  // Compute length of bar

                // Compute axial strain
                double u1 = displacements[startIndex];
                double u2 = displacements[endIndex];
                double epsilon = (u2 - u1) / L;

                // Compute axial stress using the given Young's modulus E
                double sigma_axial = E * epsilon;

                // Compute shear strain (approximated using displacement differences)
                double v1 = displacements[startIndex + 1];
                double v2 = displacements[endIndex + 1];
                double gamma = (v2 - v1) / L;  // Approximate shear strain

                // Compute shear stress using the given shear modulus G
                double tau = G * gamma;

                // Utilization ratio
                double utilization = Math.Abs(sigma_axial) / fy;

                results.Add(new BeamUtilization
                {
                    AxialStress = sigma_axial,
                    ShearStress = tau,
                    Utilization = utilization
                });
            }

            return results;
        }

    }

    public class BeamUtilization
    {
        public double AxialStress { get; set; }
        public double ShearStress { get; set; }
        public double Utilization { get; set; }
    }
}
