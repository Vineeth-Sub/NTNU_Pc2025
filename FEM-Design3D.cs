using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace NTNU_Pc2025
{
    public class FEM_design3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FEM_design class.
        /// </summary>
        public FEM_design3D()
          : base("FEM-design-truss3D", "Nickname",
              "Description",
              "Master", "FEM-truss-3D")
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
            pManager.AddNumberParameter("ScaleFactor", "Scale", "ScaleFactor for displacement", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FEM-displacement", "u", "FEM analysis that gives U vector", GH_ParamAccess.list);
            pManager.AddPointParameter("Moved Nodes","Points", "Nodes moved because of the forces", GH_ParamAccess.list);
            pManager.AddLineParameter("Moved Elements", "Lines", "Elements moved because of the forces", GH_ParamAccess.list);
            pManager.AddGenericParameter("FEM-force", "F", "FEM analysis that gives F vector", GH_ParamAccess.list);
            pManager.AddGenericParameter("FEM-Steel usage", "Sigma", "FEM analysis that gives sigma vector", GH_ParamAccess.list);
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
            double Sfactor = new double();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, loadNodes);
            DA.GetData(2, ref loads);
            DA.GetDataList(3, supportNodes);
            DA.GetData(4, ref Sfactor);

            List<Point3d> Nodes = createNodesFromListLines(lines);

            List<FEM_Element> elements = CreateElements(lines, Nodes);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Local Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[0].LocalStiffnessMatrix.ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[1].LocalStiffnessMatrix.ToString());

            // CREATE STRUCTURE
            FEM_Structure3D structure = new FEM_Structure3D(elements, Nodes);

            // Debug: Print Global Stiffness Matrix
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Global Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, structure.GlobalStiffnessMatrix.ToString());


            // CREATE FORCE VECTOR
            Vector<double> forceVector = CreateForceVector(Nodes, loadNodes, loads);

            // Debug: Print Force Vector
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Force Vector:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, forceVector.ToString());

            // GET FIXED NODE INDICES
            List<int> fixedNodeIndices = GetFixedNodeIndices(Nodes, supportNodes);

            // Debug: Print Fixed Node Indices
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Fixed Node Indices:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Join(", ", fixedNodeIndices));



            Vector<double> displacements = FEM_Solver.SolveSystem(this, structure.GlobalStiffnessMatrix, forceVector, fixedNodeIndices);
            List<Vector3d> grasshopperVectors = ConvertToVector3DList(displacements);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Displacements:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, displacements.ToString());


            // CREATE FORCE VECTOR
            List<double> NodalForce = CalculateForce.ComputeElementForces(elements, displacements, Nodes);
            //List<Vector3d> grasshopperVectorsF = ConvertVectorToList(NodalForce);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "F:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, NodalForce.ToString());


            // CREATE SIGMA VECTOR
            List<double> Sigma = ComputeUtilization.Utilization(elements, NodalForce);
            //List<Vector3d> grasshopperVectorsSig = ConvertVectorToList(Sigma);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Sigma:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Sigma.ToString());


            // DISPLACED NODES
            List<Point3d> MovedNodes = ComputeDisplacedNodes(Nodes, displacements, Sfactor);

            // DISPLACED TRUSS
            List<Line> MovedElements = ComputeDisplacedTruss(MovedNodes, elements);


            DA.SetDataList(0, grasshopperVectors);
            DA.SetDataList(1, MovedNodes);
            DA.SetDataList(2, MovedElements);
            DA.SetDataList(3, NodalForce);
            DA.SetDataList(4, Sigma);



            //List<Point3d> startNodes = new List<Point3d>();
            //List<Point3d> endNodes = new List<Point3d>();
            //List<int> nodeNumber = new List<int>();

            /* 
             for (int i = 0; i < lines.Count; i++)
             {
                 foreach (Curve line in lines)
                 {
                     startNodes.Add(line.PointAtStart);
                     endNodes.Add(line.PointAtEnd);
                     nodeNumber.Add(i);
                 }
             }

             */

            /*

            for (int i = 0; i < lines.Count; i++)
            {
                nodeNumber.Add(i);
            }

            */






            //DA.SetDataList(0, Nodes);


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



                elements.Add(new FEM_Element(nodes, startIndex, endIndex, 21000, 23.90));
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

                if (index >= 0)
                {
                    fixedNodeIndices.Add(index * 3);
                    fixedNodeIndices.Add(index * 3 + 1);
                    fixedNodeIndices.Add(index * 3 + 2);
                }



            }

            return fixedNodeIndices;
        }

        Vector<double> CreateForceVector(List<Point3d> allNodes, List<Point3d> loadNodes, Vector3d loadForces)
        {
            Vector<double> forceVector = Vector<double>.Build.Dense(allNodes.Count * 3);

            for (int i = 0; i < loadNodes.Count; i++)
            {
                int index = allNodes.FindIndex(node => node.Equals(loadNodes[i]));
                if (index >= 0)
                {
                    forceVector[index * 3] = loadForces.X;
                    forceVector[index * 3 + 1] = loadForces.Y;
                    forceVector[index * 3 + 2] = loadForces.Z;
                }
            }

            return forceVector;
        }

        List<Vector3d> ConvertToVector3DList(Vector<double> displacements)
        {
            List<Vector3d> vectors = new List<Vector3d>();
            for (int i = 0; i < displacements.Count; i += 3)
            {
                vectors.Add(new Vector3d(displacements[i], displacements[i + 1], displacements[i + 2]));
            }
            return vectors;
        }


        List<double> ConvertVectorToList(Vector<double> inputVector)
        {
            return inputVector.ToList();
        }





        List<Point3d> ComputeDisplacedNodes(List<Point3d> originalNodes, Vector<double> displacements, double scaleFactor)
        {
            List<Point3d> displacedNodes = new List<Point3d>();

            for (int i = 0; i < originalNodes.Count; i++)
            {
                int index = i * 3; // Since it's 3D, each node has 3 displacement values (Ux, Uy, Uz)
                Point3d displacedPoint = new Point3d(
                    originalNodes[i].X + displacements[index] * scaleFactor,
                    originalNodes[i].Y + displacements[index + 1] * scaleFactor,
                    originalNodes[i].Z + displacements[index + 2] * scaleFactor
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





        /*
        List<int> GetLoadNodeIndices(List<Point3d> allNodes, List<Point3d> loadNodes)
        {
            List<int> loadNodeIndices = new List<int>();
            // Find the indices of load nodes in the allNodes list
            foreach (Point3d loadNode in loadNodes)
            {
                int index = allNodes.IndexOf(loadNode);
                if (index >= 0) // Only add valid indices
                {
                    loadNodeIndices.Add(index);
                }
            }
            return loadNodeIndices;
        }
        */


        /*
        List<Point3d> createStartNodesFromListLines(List<Line> lines)
        {
            List<Point3d> nodes = new List<Point3d>();
            foreach (Line line in lines)
            {
                nodes.Add(line.From);
            }
            return nodes;
        }

        List<Point3d> createEndNodesFromListLines(List<Line> lines)
        {
            List<Point3d> nodes = new List<Point3d>();
            foreach (Line line in lines)
            {
                nodes.Add(line.To);
            }
            return nodes;
        }
        */



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
            get { return new Guid("65031F50-30AA-49EB-807E-288C9F086A83"); }
        }
    }
}