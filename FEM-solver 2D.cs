using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;

namespace NTNU_Pc2025
{
    internal class FEM_Solver_2D
    {
        public static Vector<double> SolveSystem(GH_Component component, Matrix<double> KMat, Vector<double> FVec, List<int> fixedNodes, Vector<double> G)
        {

            int n = KMat.RowCount;


            double softStiffness = 1e5; // Previously used, but now replaced with identity fix

            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "KMat before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());

            // Apply external force correction
            FVec += G;
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec after adding G :");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());

            // Apply boundary conditions
            foreach (int dof in fixedNodes)
            {
                if (dof >= 0 && dof < KMat.RowCount)
                {
                    KMat.ClearRow(dof);
                    KMat.ClearColumn(dof);
                    KMat[dof, dof] = 1.0; // Identity fix (instead of soft stiffness)
                    FVec[dof] = 0.0;
                }
            }

            // Debug: Check condition number
            double condNum = KMat.ConditionNumber();
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Condition Number: " + condNum.ToString());

            // Compute determinant for debugging
            double determinant = KMat.Determinant();
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Determinant calculated: " + determinant.ToString());

            if (Math.Abs(determinant) < 1e-10)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: Global stiffness matrix is singular! The structure has an instability.");
                
            }

            // Alternative check: Rank deficiency detection
            var rank = KMat.Rank();
            if (rank < KMat.ColumnCount)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: Stiffness matrix rank deficiency detected! The model may be unstable.");
               
            }


            Vector<double> result = KMat.Solve(FVec);

            // Set values close to zero as exactly zero

            /*

            double tolerance = 1e-10;
            for (int i = 0; i < result.Count; i++)
            {
                if (Math.Abs(result[i]) < tolerance)
                {
                    result[i] = 0.0;
                }
            }
            */

            return result;

        }
    }

}
