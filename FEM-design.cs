using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;

namespace NTNU_Pc2025
{
    public class FEM_design : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FEM_design class.
        /// </summary>
        public FEM_design()
          : base("FEM-design-truss2D", "Nickname",
              "Description",
              "Master", "FEM-truss")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Line", "L", "Line to be analyzed", GH_ParamAccess.list);
            pManager.AddPointParameter("LoadNodes", "LN", "LoadNodes to be analyzed", GH_ParamAccess.list);
            pManager.AddVectorParameter("Load", "L", "LoadVector to be analyzed", GH_ParamAccess.item);
            pManager.AddPointParameter("SupportsNodes", "SN", "Supports to be analyzed", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Apply Gravity", "Gravity", "True for gravity false without gravity", GH_ParamAccess.item, true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FEM", "FEM", "FEM analysis that gives U vector", GH_ParamAccess.item);
            pManager.AddGenericParameter("Matrix", "M", "Stiffnessmatrix", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> lines = new List<Curve>();
            List<Point3d> loadNodes = new List<Point3d>();
            Vector3d loads = new Vector3d();
            List<Point3d> supportNodes = new List<Point3d>();
            Boolean Switch = new Boolean();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, loadNodes);
            DA.GetData(2,ref loads);
            DA.GetDataList(3, supportNodes);
            DA.GetData(4, ref Switch);

            List<Point3d> Nodes = createNodesFromListLines(lines);

            List<FEM_Element> elements = CreateElements(lines, Nodes);

            // CREATE STRUCTURE
            FEM_Structure structure = new FEM_Structure(elements, Nodes);
            var xxx = structure.GlobalStiffnessMatrix.ToArray();

            // Debug: Print Global Stiffness Matrix
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Global Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, structure.GlobalStiffnessMatrix.ToString());


            // CREATE FORCE VECTOR
            Vector<double> forceVector = CreateForceVector(Nodes, loadNodes, loads);

            // Debug: Print Force Vector
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Force Vector:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, forceVector.ToString());


            Vector<double> G;
            if (Switch == true)
                G = Gravity(elements, Nodes);
            else
                G = Vector<double>.Build.Dense(Nodes.Count * 2);



            // GET FIXED NODE INDICES
            List<int> fixedNodeIndices = GetFixedNodeIndices(Nodes, supportNodes);

            // Debug: Print Fixed Node Indices
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Fixed Node Indices:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Join(", ", fixedNodeIndices));



           Vector<double> displacements = FEM_Solver.SolveSystem(this, structure.GlobalStiffnessMatrix, forceVector, fixedNodeIndices, G);
           List<Vector3d> grasshopperVectors = ConvertToVector3DList(displacements);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Displacements:");
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, displacements.ToString());


            DA.SetDataList(0, grasshopperVectors);
            DA.SetDataList(1, xxx);



        }

        List<Point3d> createNodesFromListLines(List<Curve> lines)
        {
            List<Point3d> nodes = new List<Point3d>();

                foreach (Curve line in lines)
                {
                    nodes.Add(line.PointAtStart);
                    nodes.Add(line.PointAtEnd);
                  


                }
            nodes = Point3d.CullDuplicates(nodes, 1.0).ToList();
            return nodes;
        }

        List<FEM_Element> CreateElements(List<Curve> lines, List<Point3d> nodes)
        {
            List<FEM_Element> elements = new List<FEM_Element>();

            foreach (Curve line in lines)
            {
                int startIndex = nodes.IndexOf(line.PointAtStart);
                int endIndex = nodes.IndexOf(line.PointAtEnd);
                double angle = Math.Atan2(line.PointAtEnd.Y - line.PointAtStart.Y, line.PointAtEnd.X - line.PointAtStart.X);

                Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            {  1, 0, -1, 0 },
            {  0, 0, 0, 0 },
            {  -1, 0, 1, 0 },
            {  0, 0, 0, 0 },
        });

                elements.Add(new FEM_Element(startIndex, endIndex, 5,5, nodes, angle));
            }

            return elements;
        }


        List<int> GetFixedNodeIndices(List<Point3d> allNodes, List<Point3d> supportNodes)
        {
            List<int> fixedNodeIndices = new List<int>();

            // Find the indices of fixed nodes in the allNodes list
            foreach (Point3d supportNode in supportNodes)  //HERE MIGHT THERE BE A PROBLEM
            {
                int index = allNodes.FindIndex(node => node.Equals(supportNode));

                    fixedNodeIndices.Add(index * 2);
                    fixedNodeIndices.Add(index * 2 + 1);
                

            }

            return fixedNodeIndices;
        }

        Vector<double> CreateForceVector(List<Point3d> allNodes, List<Point3d> loadNodes, Vector3d loadForces)
        {
            Vector<double> forceVector = Vector<double>.Build.Dense(allNodes.Count * 2);

            for (int i = 0; i < loadNodes.Count; i++)
            {
                int index = allNodes.FindIndex(node => node.Equals(loadNodes[i]));

                forceVector[index * 2] = loadForces.X;
                forceVector[index * 2 + 1] = loadForces.Y;
                
            }

            return forceVector;
        }


        Vector<double> Gravity(List<FEM_Element> ELEMENTS, List<Point3d> globalNodes)
        {
            double g = 9.82;
            Vector<double> gravity = Vector<double>.Build.Dense(globalNodes.Count * 2);

            for (int i = 0; i < globalNodes.Count; i++)
            {
                FEM_Element correspondingElement = ELEMENTS.FirstOrDefault(e => e.StartNode == i || e.EndNode == i);
                if (correspondingElement != null)
                {
                    double mass = correspondingElement.M; // Accessing the mass property
                    gravity[i * 2 + 1] = -mass * g / 2; // Apply gravity in the Z direction
                }

            }

            return gravity;

        }


        List<Vector3d> ConvertToVector3DList(Vector<double> displacements)
        {
            List<Vector3d> vectors = new List<Vector3d>();
            for (int i = 0; i < displacements.Count; i += 2)
            {
                vectors.Add(new Vector3d(displacements[i], displacements[i + 1], 0));
            }
            return vectors;
        }




        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("56D8BCAD-26AC-42D9-A1B3-6B3DA9C0F236"); }
        }
    }
}