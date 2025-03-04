using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel;


namespace NTNU_Pc2025
{
    internal class FEM_Structure3D
    {
        public List<FEM_Element> Elements;   // List of all bar elements
        public List<Point3d> Nodes;          // List of all unique nodes
        public Matrix<double> GlobalStiffnessMatrix;

        public FEM_Structure3D(List<FEM_Element> elements, List<Point3d> nodes)
        {
            Elements = elements;
            Nodes = nodes;
            int totalDOF = nodes.Count * 3;  // 3 DOF per node (X, Y)
            GlobalStiffnessMatrix = DenseMatrix.OfArray(new double[totalDOF, totalDOF]);

            AssembleGlobalMatrix();
        }

        private void AssembleGlobalMatrix()
        {
            foreach (var element in Elements)
            {
                Matrix<double> kLocal = element.LocalStiffnessMatrix;
                int startIndex = element.StartNode * 3;
                int endIndex = element.EndNode * 3;

                int[] dofMapping = { startIndex, startIndex + 1, startIndex + 2, endIndex, endIndex + 1, endIndex + 2 };

                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        GlobalStiffnessMatrix[dofMapping[i], dofMapping[j]] += kLocal[i, j];
                    }
                }
            }

        }
    }
}
