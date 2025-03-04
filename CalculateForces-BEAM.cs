using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace NTNU_Pc2025
{
    internal class CalculateForces_BEAM
    {



        public static List<Vector<double>> ComputeElementForces(List<FEM_Element_BEAM> elements, Vector<double> globalDisplacements, List<Point3d> globalnodes)
        {
            List<Vector<double>> elementForces = new List<Vector<double>>();

            foreach (var element in elements)
            {
                Point3d start = globalnodes[element.StartNode];
                Point3d end = globalnodes[element.EndNode];
                // Extract Local Stiffness Matrix (12x12 for 3D beam)
                Matrix<double> kLocal = element.LocalStiffnessMatrix;
                Matrix<double> rotationMatrix3DBEAM = RotateMatrix3D_BEAM.RotateMatrix3DBEAM(start, end);

                // Get DOF mapping for this element (6 DOFs per node)
                int startIndex = element.StartNode * 6;
                int endIndex = element.EndNode * 6;

                int[] dofMapping =
                {
            startIndex, startIndex + 1, startIndex + 2, startIndex + 3, startIndex + 4, startIndex + 5,
            endIndex, endIndex + 1, endIndex + 2, endIndex + 3, endIndex + 4, endIndex + 5
                        };

                // Extract Local Displacements (u_local)
                Vector<double> uLocal = Vector<double>.Build.Dense(12);
                for (int i = 0; i < 12; i++)
                {
                    uLocal[i] = globalDisplacements[dofMapping[i]];
                }

                // Compute Local Forces: F_local = K_local * u_local
                Vector<double> fLocal = kLocal * uLocal;
                Vector<double> fGlobal = rotationMatrix3DBEAM * fLocal;


                // Store result
                elementForces.Add(fGlobal);
            }

            return elementForces;
        }


    }
}
