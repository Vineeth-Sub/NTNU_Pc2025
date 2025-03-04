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
    internal class ComputeUtilization
    {
        public static List<double> Utilization(List<FEM_Element> elements, List<double> forces)
        {
            List<double> utilizations = new List<double>();

            for (int i = 0; i < elements.Count; i++)
            {
                // Compute utilization for each element using the correct force index
                double utilization = ComputeElementUtilization(elements[i], forces[i]);
                utilizations.Add(utilization);
            }

            return utilizations; //Vector<double>.Build.DenseOfEnumerable(utilizations);
        }

        private static double ComputeElementUtilization(FEM_Element element, double force)
        {
            // Compute the utilization for a single element
            double sigma = 355; // Yield strength of steel
            double A = element.Area; // Cross-sectional area

            return (force / A) / sigma;
        }
    
    }
}
