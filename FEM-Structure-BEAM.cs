using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NTNU_Pc2025
{
    internal class FEM_Structure_BEAM
    {
        public List<FEM_Element_BEAM> Elements;   // List of all bar elements
        public List<Point3d> Nodes;          // List of all unique nodes
        public Matrix<double> GlobalStiffnessMatrix;

        public FEM_Structure_BEAM(List<FEM_Element_BEAM> elements, List<Point3d> nodes)
        {
            Elements = elements;
            Nodes = nodes;
            int totalDOF = nodes.Count * 6;  // 6 DOF per node
            GlobalStiffnessMatrix = DenseMatrix.OfArray(new double[totalDOF, totalDOF]);

            AssembleGlobalMatrix();
        }

        private void AssembleGlobalMatrix()
        {
            foreach (var element in Elements)
            {
                Matrix<double> kLocal = element.LocalStiffnessMatrix;
                int startIndex = element.StartNode * 6;
                int endIndex = element.EndNode * 6;

                int[] dofMapping = {
            startIndex, startIndex + 1, startIndex + 2, startIndex + 3, startIndex + 4, startIndex + 5,
            endIndex, endIndex + 1, endIndex + 2, endIndex + 3, endIndex + 4, endIndex + 5
                                    };

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        GlobalStiffnessMatrix[dofMapping[i], dofMapping[j]] += kLocal[i, j];
                    }
                }
            }
        }

    }
}
