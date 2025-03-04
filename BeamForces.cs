using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace NTNU_Pc2025
{
    internal class BEAMForces
    {
        public class BeamForces
        {
            public double AxialForce { get; set; }       // Fx
            public double ShearForceY { get; set; }      // Fy
            public double ShearForceZ { get; set; }      // Fz
            public double TorsionalMoment { get; set; }  // Mx
            public double BendingMomentY { get; set; }   // My
            public double BendingMomentZ { get; set; }   // Mz
        }


        public static List<BeamForces> ExtractForcesAndMoments(List<Vector<double>> elementForces)
        {
            List<BeamForces> forcesList = new List<BeamForces>();

            foreach (var fLocal in elementForces)
            {
                BeamForces forces = new BeamForces
                {
                    AxialForce = fLocal[0],   // Fx
                    ShearForceY = fLocal[1],  // Fy
                    ShearForceZ = fLocal[2],  // Fz
                    TorsionalMoment = fLocal[3],  // Mx
                    BendingMomentY = fLocal[4],  // My
                    BendingMomentZ = fLocal[5]   // Mz
                };

                forcesList.Add(forces);
            }

            return forcesList;
        }

    }
}
