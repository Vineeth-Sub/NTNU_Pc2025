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
        public static Vector<double> SolveSystem(GH_Component component,Matrix<double> KMat, Vector<double> FVec, List<int> fixedNodes)
        {
            int n = KMat.RowCount; // Matrix size remains the same
            double softStiffness = 1e8; // Very large but not infinite stiffness

            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "KMat before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec before applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());


            // Apply boundary conditions
            foreach (int dof in fixedNodes)
            {
                /*
                KMat.SetRow(dof, Vector<double>.Build.Dense(n, 0));
                KMat.SetColumn(dof, Vector<double>.Build.Dense(n, 0));
                KMat[dof, dof] = 1;
                FVec[dof] = 0;
                */


                
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

                


                /*
                if (dof >= 0 && dof < KMat.RowCount)
                {
                    KMat = KMat.RemoveRow(dof).RemoveColumn(dof);
                    FVec = Vector<double>.Build.DenseOfEnumerable(FVec.Where((_, index) => index != dof));
                }
                else
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid index: {dof}");
                }
                */

            }

            // Debug: Print KMat and FVec after applying boundary conditions
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "KMat after applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FVec after applying boundary conditions:");
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, FVec.ToString());
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, KMat.ConditionNumber().ToString());


            return KMat.Solve(FVec);
        }


    }
}
