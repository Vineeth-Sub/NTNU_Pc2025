using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal class FEM_Solver
    {
        public static Vector<double> SolveSystem(GH_Component component, Matrix<double> KMat, Vector<double> FVec, List<int> fixedNodes, Vector<double> G)
        {
            int n = KMat.RowCount; // Matrix size remains the same
            double softStiffness = 1e5; // Very large but not infinite stiffness

            //KMat = KMat.Map(Math.Ceiling);

            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "KMat before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());

            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "   G :");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, G.ToString());

            FVec += G;
            //FVec += BLL;
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec after adding G :");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());

            // Apply boundary conditions
            foreach (int dof in fixedNodes)
            {
                if (dof >= 0 && dof < KMat.RowCount)
                {
                    KMat.SetRow(dof, Vector<double>.Build.Dense(n, 0)); // Zero out row
                    KMat.SetColumn(dof, Vector<double>.Build.Dense(n, 0)); // Zero out column
                    KMat[dof, dof] = softStiffness; // High stiffness prevents movement
                    FVec[dof] = 0; // Force should be zero at fixed DOFs


                    /*
                    if (dof % 6 >= 3)  // Only apply soft constraints to rotational DOFs
                    {
                        KMat[dof, dof] = softStiffness;
                    }
                    else
                    {
                        KMat.SetRow(dof, Vector<double>.Build.Dense(n, 0));
                        KMat.SetColumn(dof, Vector<double>.Build.Dense(n, 0));
                        KMat[dof, dof] = 1;
                        FVec[dof] = 0;
                    }
                    */




                }
            }

            
            double check = KMat.ConditionNumber();//3.57e19 for my high voltage tower OH NOOO!!   1.39e10 for BEAM column with extra support. not good.
            // Debug: Print KMat and FVec after applying boundary conditions
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "KMat after applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec after applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ConditionNumber().ToString());

            //KMat[4, 4] = 1;
            //KMat[5, 5] = 1;
            //KMat[11, 11] = 1;  //WHY DOES THIS HELP COLUMN?



            // Compute determinant of final global stiffness matrix
            double determinant = KMat.Determinant(); // VERY HIGH for BEAM column with extra support. //Is infinity for high voltage tower model problem OH NOOO!!
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Determinant calculated.");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Math.Abs(determinant).ToString());

            // Check if determinant is close to zero (indicating instability)
            if (Math.Abs(determinant) < 1e-10)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: Global stiffness matrix is singular! The structure has an instability.");
            }
        
          
            // Alternative: Check rank (if determinant is unreliable) => RIGID BODY MOVEMENTS 
            var rank = KMat.Rank();
            if (rank < KMat.ColumnCount)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: Stiffness matrix rank deficiency detected! The model may be unstable.");
            }
            
            Vector<double> result = KMat.Solve(FVec);

            // Set values close to zero as exactly zero
            
            double tolerance = 1e-10;
            for (int i = 0; i < result.Count; i++)
            {
                if (Math.Abs(result[i]) < tolerance)
                {
                    result[i] = 0.0;
                }
            }
            
            return result;
        }


    }
}
