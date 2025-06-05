using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using static Rhino.UI.Controls.CollapsibleSectionImpl;

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
            pManager.AddBooleanParameter("Apply Gravity", "Gravity", "True for gravity false without gravity", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("E", "E", "Young's Modulus", GH_ParamAccess.item, 2.1E10);
            pManager.AddNumberParameter("A", "A", "Area", GH_ParamAccess.item, 0.002390);
            pManager.AddNumberParameter("Density", "Density", "Density", GH_ParamAccess.item, 7850);
            pManager.AddNumberParameter("Sfactor", "S", "Scale factor for the displacement", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FEM", "FEM", "FEM analysis that gives U vector", GH_ParamAccess.item);
            pManager.AddPointParameter("Moved Nodes", "Points", "Nodes moved because of the forces", GH_ParamAccess.list);
            pManager.AddLineParameter("Moved Elements", "Lines", "Elements moved because of the forces", GH_ParamAccess.list);
            pManager.AddGenericParameter("Matrix", "M", "Stiffnessmatrix", GH_ParamAccess.list);
            pManager.AddNumberParameter("Max displacement", "MaxU", "Max displacement", GH_ParamAccess.item);
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
            double E = new double();
            double A = new double();
            double rho = new double();
            double Sfactor = new double();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, loadNodes);
            DA.GetData(2,ref loads);
            DA.GetDataList(3, supportNodes);
            DA.GetData(4, ref Switch);
            DA.GetData(5, ref E);
            DA.GetData(6, ref A);
            DA.GetData(7, ref rho);
            DA.GetData(8, ref Sfactor);



            List<Point3d> Nodes = createNodesFromListLines(lines);

            List<FEM_Element> elements = CreateElements(lines, Nodes, E, A, rho);

            // CREATE STRUCTURE
            FEM_Structure structure = new FEM_Structure(elements, Nodes);
            var StifMat = structure.GlobalStiffnessMatrix.ToArray();

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



           Vector<double> displacements = FEM_Solver_2D.SolveSystem(this, structure.GlobalStiffnessMatrix, forceVector, fixedNodeIndices, G);
           //double maxDisplacment = ve
           List<Vector3d> grasshopperVectors = ConvertToVector3DList(displacements);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Displacements:");
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, displacements.ToString());


            // DISPLACED NODES
            List<Point3d> MovedNodes = ComputeDisplacedNodes(Nodes, displacements, Sfactor);

            // DISPLACED TRUSS
            List<Line> MovedElements = ComputeDisplacedTruss(MovedNodes, elements);


            List<double> displacementMagnitudes = new List<double>();
            for (int i = 0; i < displacements.Count; i += 2)
            {
                double ux = displacements[i];
                double uz = displacements[i + 1];
                double mag = Math.Sqrt(ux * ux + uz * uz);
                displacementMagnitudes.Add(mag);
            }

            double maxDisplacement = displacementMagnitudes.Max();




            DA.SetDataList(0, grasshopperVectors);
            DA.SetDataList(1, MovedNodes);
            DA.SetDataList(2, MovedElements);
            DA.SetDataList(3, StifMat);
            DA.SetData(4, maxDisplacement);



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

        List<FEM_Element> CreateElements(List<Curve> lines, List<Point3d> nodes, double E, double A, double rho)
        {
            List<FEM_Element> elements = new List<FEM_Element>();

            foreach (Curve line in lines)
            {
                int startIndex = nodes.IndexOf(line.PointAtStart);
                int endIndex = nodes.IndexOf(line.PointAtEnd);
                double angle = Math.Atan2(line.PointAtEnd.Z - line.PointAtStart.Z, line.PointAtEnd.X - line.PointAtStart.X);

                Matrix<double> kLocal = Matrix<double>.Build.DenseOfArray(new double[,] {
            {  1, 0, -1, 0 },
            {  0, 0, 0, 0 },
            {  -1, 0, 1, 0 },
            {  0, 0, 0, 0 },
        });

                elements.Add(new FEM_Element(startIndex, endIndex, E,A, rho, nodes, angle));
            }

            return elements;
        }


        List<int> GetFixedNodeIndices(List<Point3d> allNodes, List<Point3d> supportNodes)
        {
            List<int> fixedNodeIndices = new List<int>();

            // Find the indices of fixed nodes in the allNodes list
            foreach (Point3d supportNode in supportNodes) 
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

                if (index != -1)
                {
                    forceVector[index * 2] = loadForces.X;
                    forceVector[index * 2 + 1] = loadForces.Z;
                }
                else
                {
                    // Handle the case where the node is not found
                    // For example, log an error or throw an exception
                }
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
                vectors.Add(new Vector3d(displacements[i], 0, displacements[i + 1]));
            }
            return vectors;
        }


        List<Point3d> ComputeDisplacedNodes(List<Point3d> originalNodes, Vector<double> displacements, double scaleFactor)
        {
            List<Point3d> displacedNodes = new List<Point3d>();

            for (int i = 0; i < originalNodes.Count; i++)
            {
                int index = i * 2; // Since it's 2D, each node has 2 displacement values (Ux, Uy)
                Point3d displacedPoint = new Point3d(
                    originalNodes[i].X + displacements[index] * scaleFactor,
                    originalNodes[i].Y, //+ displacements[index + 1] * scaleFactor, 
                    originalNodes[i].Z + displacements[index + 1] * scaleFactor
                );
                displacedNodes.Add(displacedPoint);
            }

            return displacedNodes;
        }



        List<Line> ComputeDisplacedTruss(List<Point3d> displacedNodes, List<FEM_Element> elements)
        {
            List<Line> displacedLines = new List<Line>();

            foreach (var element in elements)
            {
                Point3d start = displacedNodes[element.StartNode];
                Point3d end = displacedNodes[element.EndNode];
                displacedLines.Add(new Line(start, end));
            }

            return displacedLines;
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