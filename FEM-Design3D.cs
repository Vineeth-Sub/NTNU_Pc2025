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
            pManager[1].Optional = true;
            pManager.AddVectorParameter("Load", "L", "LoadVectors to be analyzed", GH_ParamAccess.list); // Changed to list
            pManager[2].Optional = true;
            //pManager.AddIntegerParameter("LoadNodeGroups", "LNG", "Group indices for LoadNodes (if some share the same values).", GH_ParamAccess.list);
            pManager.AddPointParameter("SupportsNodes", "SN", "Supports to be analyzed", GH_ParamAccess.list);
            pManager.AddVectorParameter("SupportConditions", "SC", "Vector3d defining DOFs constraints per node (1=Fixed, 0=Free)", GH_ParamAccess.list, new Vector3d(1,1,1));
            pManager.AddNumberParameter("ScaleFactor", "Scale", "ScaleFactor for displacement", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Apply Gravity", "Gravity", "True for gravity false without gravity", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("E", "E", "Young's Modulus", GH_ParamAccess.list, 21);
            pManager.AddNumberParameter("A", "A", "Area", GH_ParamAccess.list, 0.002390);
            pManager.AddNumberParameter("Density", "Density", "Density", GH_ParamAccess.list, 7850);
            pManager.AddCurveParameter("Different properties lines", "DpL", "Apply different properties for different lines.", GH_ParamAccess.list);
            pManager[10].Optional = true;
            pManager.AddNumberParameter("DefaultE", "DefE", "Default E for unspecified lines", GH_ParamAccess.item, 21);
            pManager.AddNumberParameter("DefaultA", "DefA", "Default A for unspecified lines", GH_ParamAccess.item, 0.002390);
            pManager.AddNumberParameter("DefaultDensity", "DefD", "Default density for unspecified lines", GH_ParamAccess.item, 7850);
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
            List<List<int>> LoadNodeGroups = new List<List<int>>();
            List<Vector3d> loads = new List<Vector3d>();
            List<Point3d> supportNodes = new List<Point3d>();
            List<Vector3d> SupportConditions = new List<Vector3d>();
            double Sfactor = new double();
            Boolean Switch = new Boolean();
            List<double> E = new List<double>();
            List<double> A = new List<double>();
            List<double> Density = new List<double>();
            List<Curve> DpL = new List<Curve>();
            double DefaultE = new double();
            double DefaultA = new double();
            double DefaultDensity = new double();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, loadNodes);
            DA.GetDataList(2, loads);
            //DA.GetDataList(3, LoadNodeGroups);
            DA.GetDataList(3, supportNodes);
            DA.GetDataList(4, SupportConditions);
            DA.GetData(5, ref Sfactor);
            DA.GetData(6, ref Switch);
            DA.GetDataList(7, E);
            DA.GetDataList(8, A);
            DA.GetDataList(9, Density);
            DA.GetDataList(10, DpL);
            DA.GetData(11, ref DefaultE);
            DA.GetData(12, ref DefaultA);
            DA.GetData(13, ref DefaultDensity);

            if (DpL == null || DpL.Count == 0)
            {
                DpL = new List<Curve>();
            }
            



            List<Point3d> Nodes = createNodesFromListLines(lines);

            List<FEM_Element> elements = CreateElements(lines, Nodes, DpL, E, A, Density, DefaultE, DefaultA, DefaultDensity);

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[0].ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[1].ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[2].ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "-------");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[elements[0].StartNode].ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[elements[0].EndNode].ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[0].LocalStiffnessMatrix.ToString());

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Local Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[0].LocalStiffnessMatrix.ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Rotate_Matrix3D.RotateMatrix3D(Nodes[0], Nodes[1]).ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[1].LocalStiffnessMatrix.ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[0].DistanceTo(Nodes[1]).ToString());

            

            Dictionary<int, int> nodeConnectivity = new Dictionary<int, int>();

            foreach (var element in elements)
            {
                if (!nodeConnectivity.ContainsKey(element.StartNode)) nodeConnectivity[element.StartNode] = 0;
                if (!nodeConnectivity.ContainsKey(element.EndNode)) nodeConnectivity[element.EndNode] = 0;

                nodeConnectivity[element.StartNode]++;
                nodeConnectivity[element.EndNode]++;
            }

            // Print nodes with low connectivity
            foreach (var node in nodeConnectivity)
            {
                if (node.Value < 2) // Each node should have at least 2 connections
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $" Warning: Node {node.Key} has only {node.Value} connections. May cause instability.");
                }
            }

            




            // CREATE STRUCTURE
            FEM_Structure3D structure = new FEM_Structure3D(elements, Nodes);
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
                G = Vector<double>.Build.Dense(Nodes.Count * 3);

            // GET FIXED NODE INDICES
            List<int> fixedNodeIndices = GetFixedNodeIndices(Nodes, supportNodes, SupportConditions);

            // Debug: Print Fixed Node Indices
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Fixed Node Indices:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Join(", ", fixedNodeIndices));



            Vector<double> displacements = FEM_Solver.SolveSystem(this, structure.GlobalStiffnessMatrix, forceVector, fixedNodeIndices, G);
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
            DA.SetDataList(5, xxx);


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

        List<FEM_Element> CreateElements(List<Curve> lines, List<Point3d> nodes, List<Curve> DpL, List<double> E, List<double> A, List<double> Density, double DefaultE, double DefaultA, double DefaultDensity)
        {
            List<FEM_Element> elements = new List<FEM_Element>();
            double tolerance = 1e-6;

            for (int i = 0; i < lines.Count; i++)
            {
                Curve line = lines[i];
                //int startIndex = nodes.IndexOf(line.PointAtStart); //PROBLEM
                //int endIndex = nodes.IndexOf(line.PointAtEnd); //PROBLEM


                Point3d startPoint = line.PointAtStart;
                Point3d endPoint = line.PointAtEnd;

                int startIndex = nodes.FindIndex(node => node.DistanceTo(startPoint) < tolerance);
                int endIndex = nodes.FindIndex(node => node.DistanceTo(endPoint) < tolerance);





                if (startIndex < 0 || endIndex < 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid node index: startIndex={startIndex}, endIndex={endIndex} for line {i}");
                    continue; // Skip this element
                }


                // Check if the line has a specified property set
                int propertyIndex = DpL.FindIndex(dp => dp.PointAtStart == line.PointAtStart && dp.PointAtEnd == line.PointAtEnd);

                double elementE;
                if (propertyIndex >= 0 && propertyIndex < E.Count)
                {
                    elementE = E[propertyIndex];
                }
                else
                {
                    elementE = DefaultE;
                }

                double elementA;
                if (propertyIndex >= 0 && propertyIndex < A.Count)
                {
                    elementA = A[propertyIndex];
                }
                else
                {
                    elementA = DefaultA;
                }

                double elementDensity;
                if (propertyIndex >= 0 && propertyIndex < Density.Count)
                {
                    elementDensity = Density[propertyIndex];
                }
                else
                {
                    elementDensity = DefaultDensity;
                }
                elements.Add(new FEM_Element(nodes, startIndex, endIndex, elementE, elementA, elementDensity));
            }

            return elements;
        }



        
        List<int> GetFixedNodeIndices(List<Point3d> allNodes, List<Point3d> supportNodes, List<Vector3d> supportConditions)
        {
            List<int> fixedNodeIndices = new List<int>();
            foreach (Point3d supportNode in supportNodes)
            {
                int i = 0;
                int index = allNodes.FindIndex(node => node.Equals(supportNode));

                if (index >= 0 && i < supportConditions.Count)
                {
                    Vector3d constraints = supportConditions[i];

                    if (constraints.X != 0) fixedNodeIndices.Add(index * 3);     // Fix X
                    if (constraints.Y != 0) fixedNodeIndices.Add(index * 3 + 1); // Fix Y
                    if (constraints.Z != 0) fixedNodeIndices.Add(index * 3 + 2); // Fix Z
                }

                i += 1;
            }

                return fixedNodeIndices;
        }




        Vector<double> CreateForceVector(List<Point3d> allNodes, List<Point3d> loadNodes, List<Vector3d> loadForces)
        {
            Vector<double> forceVector = Vector<double>.Build.Dense(allNodes.Count * 3);

            for (int i = 0; i < loadNodes.Count; i++)
            {
                int index = allNodes.FindIndex(node => node.Equals(loadNodes[i]));
                if (index >= 0 && loadForces.Count > 1)
                {
                    forceVector[index * 3] += loadForces[i].X;
                    forceVector[index * 3 + 1] += loadForces[i].Y;
                    forceVector[index * 3 + 2] += loadForces[i].Z;
                }

                else if (index >= 0 && loadForces.Count == 1)
                {
                    forceVector[index * 3] += loadForces[0].X;
                    forceVector[index * 3 + 1] += loadForces[0].Y;
                    forceVector[index * 3 + 2] += loadForces[0].Z;
                }
            }

            return forceVector;
        }


        /*
        Vector<double> CreateForceVector(List<Point3d> allNodes, List<List<int>> LoadNodeGroups, List<Vector3d> LoadVectors)
        {
            Vector<double> forceVector = Vector<double>.Build.Dense(allNodes.Count * 3);

            for (int i = 0; i < LoadNodeGroups.Count; i++)
            {
                List<int> group = LoadNodeGroups[i];
                Vector3d loadForce = LoadVectors[i];

                foreach (int nodeIndex in group)
                {
                    if (nodeIndex >= 0 && nodeIndex < allNodes.Count)
                    {
                        forceVector[nodeIndex * 3] += loadForce.X;
                        forceVector[nodeIndex * 3 + 1] += loadForce.Y;
                        forceVector[nodeIndex * 3 + 2] += loadForce.Z;
                    }
                }
            }

            return forceVector;
        }
        */

        Vector<double> Gravity(List<FEM_Element> ELEMENTS, List<Point3d> globalNodes)
        {
            double g = 9.82;
            Vector<double> gravity = Vector<double>.Build.Dense(globalNodes.Count * 3);

            for (int i =0; i<globalNodes.Count; i++)
            {
                FEM_Element correspondingElement = ELEMENTS.FirstOrDefault(e => e.StartNode == i || e.EndNode == i);
                if (correspondingElement != null)
                {
                    double mass = correspondingElement.M; // Accessing the mass property
                    gravity[i * 3 + 2] = -mass * g / 2; // Apply gravity in the Z direction
                }

            }

            return gravity;

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
          List<FEM_Element> CreateElements(List<Curve> lines, List<Point3d> nodes)
        {
            List<FEM_Element> elements = new List<FEM_Element>();

            foreach (Curve line in lines)
            {
                int startIndex = nodes.IndexOf(line.PointAtStart);
                int endIndex = nodes.IndexOf(line.PointAtEnd);




                elements.Add(new FEM_Element(nodes, startIndex, endIndex, 21, 0.002390));
            }

            return elements;
        }
         */


        /*
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