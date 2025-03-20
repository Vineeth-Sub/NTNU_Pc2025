using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.ComponentModel;

namespace NTNU_Pc2025
{
    public class FEM_Design3D_BEAM : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FEM_Design3D_BEAM class.
        /// </summary>
        public FEM_Design3D_BEAM()
          : base("FEM-Design3D-BEAM", "Nickname",
              "Description",
              "Master", "FEM - BEAM")
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
            pManager.AddNumberParameter("ScaleFactor", "Scale", "ScaleFactor for displacement", GH_ParamAccess.item,1);
            pManager.AddBooleanParameter("Apply Gravity", "Gravity", "True for gravity flase withou gravity", GH_ParamAccess.item, true);
            pManager.AddCurveParameter("BlockLoads", "BL", "BlockLoads to be analyzed", GH_ParamAccess.list); //PROBLEM BIG PROBLEM
            pManager[6].Optional = true;
            pManager.AddVectorParameter("BlockLoads", "BL", "BlockLoadVector to be analyzed", GH_ParamAccess.item);
            pManager[7].Optional = true;
            pManager.AddNumberParameter("E", "E", "Young's Modulus", GH_ParamAccess.list, 21);
            pManager.AddNumberParameter("A", "A", "Area", GH_ParamAccess.list, 0.002390);
            pManager.AddNumberParameter("Density", "Density", "Density", GH_ParamAccess.list, 7850);
            pManager.AddCurveParameter("Different properties lines", "DpL", "Apply different properties for different lines.", GH_ParamAccess.list);
            pManager[11].Optional = true;
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
            pManager.AddPointParameter("Moved Nodes", "Points", "Nodes moved because of the forces", GH_ParamAccess.list);
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
            Vector3d loads = new Vector3d();
            List<Point3d> supportNodes = new List<Point3d>();
            double Sfactor = new double();
            Boolean Switch = new Boolean();
            List<Curve> blockLoadLine = new List<Curve>();
            Vector3d blockLoad = new Vector3d();
            List<double> E = new List<double>();
            List<double> A = new List<double>();
            List<double> Density = new List<double>();
            List<Curve> DpL = new List<Curve>();
            double DefaultE = new double();
            double DefaultA = new double();
            double DefaultDensity = new double();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, loadNodes);
            DA.GetData(2, ref loads);
            DA.GetDataList(3, supportNodes);
            DA.GetData(4, ref Sfactor);
            DA.GetData(5, ref Switch);
            DA.GetDataList(6, blockLoadLine);
            DA.GetData(7, ref blockLoad);
            DA.GetDataList(8, E);
            DA.GetDataList(9, A);
            DA.GetDataList(10, Density);
            DA.GetDataList(11, DpL);
            DA.GetData(12, ref DefaultE);
            DA.GetData(13, ref DefaultA);
            DA.GetData(14, ref DefaultDensity);

            List<Point3d> Nodes = createNodesFromListLines(lines);
            
            List<FEM_Element_BEAM> elements = CreateElements(lines, Nodes, DpL, E, A, new List<double> { 1.317e-1}, new List<double> { 1.009e-2}, new List<double> { 4.8e-5 }, new List<double> { 8.076 }, Density, DefaultE, DefaultA, 1.317e-1, 1.009e-2, 4.8e-9, 8.076 ,DefaultDensity);
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[0].ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[1].ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[2].ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "-------");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[elements[0].StartNode].ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[elements[0].EndNode].ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, ComputeKLocal.CalculateKLocal(Nodes, 0, 1, 21, 0.002390, 1.317e-5, 1.009e-6, 8.076, 4.8).ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[0].LocalStiffnessMatrix.ToString());

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Local Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[0].LocalStiffnessMatrix.ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, RotateMatrix3D_BEAM.RotateMatrix3DBEAM(Nodes[0], Nodes[1]).ToString());
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, elements[1].LocalStiffnessMatrix.ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Nodes[0].DistanceTo(Nodes[1]).ToString());



            // CREATE STRUCTURE
            FEM_Structure_BEAM structure = new FEM_Structure_BEAM(elements, Nodes);
            var xxx = structure.GlobalStiffnessMatrix.ToArray();

            // Debug: Print Global Stiffness Matrix
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Global Stiffness Matrix:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, structure.GlobalStiffnessMatrix.ToString());


            Matrix<double> rotationMatrix = RotateMatrix3D_BEAM.RotateMatrix3DBEAM(Nodes[0], Nodes[1]);
            Matrix<double> CC = rotationMatrix.Transpose() * rotationMatrix;
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rotation Matrix 3D BEAM:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, rotationMatrix.ToString());
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "I 3D BEAM:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, CC.ToString());


            // CREATE FORCE VECTOR
            Vector<double> forceVector = CreateForceVector(Nodes, loadNodes, loads);

            Vector<double> BlockLoadVector = BlockLoad(blockLoadLine, blockLoad, Nodes);

            forceVector += BlockLoadVector;

            // Debug: Print Force Vector
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Force Vector:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, forceVector.ToString());

            Vector<double> G;
            if (Switch == true)
                G = Gravity(elements, Nodes);
            else
                G = Vector<double>.Build.Dense(Nodes.Count * 6);




            // GET FIXED NODE INDICES
            List<int> fixedNodeIndices = GetFixedNodeIndices(Nodes, supportNodes);

            // Debug: Print Fixed Node Indices
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Fixed Node Indices:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Join(", ", fixedNodeIndices));



            Vector<double> displacements = FEM_Solver.SolveSystem(this, structure.GlobalStiffnessMatrix, forceVector, fixedNodeIndices, G);
            List<Vector3d> grasshopperVectors = ConvertToVector3DList(displacements);

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Displacements:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, displacements.ToString());


            // CREATE FORCE VECTOR
            List<Vector<double>> Forces = CalculateForces_BEAM.ComputeElementForces(elements, displacements, Nodes);
            List<BEAMForces.BeamForces> BForces = BEAMForces.ExtractForcesAndMoments(Forces);
            List<string> readableForces = new List<string>();
            for (int i = 0; i < elements.Count; i++)
            {
                BEAMForces.BeamForces force = BForces[i];  // Get force for element i
                readableForces.Add($"Element {i}: Fx={force.AxialForce:F2}, Fy={force.ShearForceY:F2}, Fz={force.ShearForceZ:F2}, " +
                                   $"Mx={force.TorsionalMoment:F2}, My={force.BendingMomentY:F2}, Mz={force.BendingMomentZ:F2}");
            }


            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "F:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Forces.ToString());


            // CREATE SIGMA VECTOR
            List<BeamUtilization> Uti = CalculateUtilization_BEAM.ComputeUtilization(elements, Nodes, displacements, 210000, 75000, 355);
            List<string> readableUtilization = new List<string>();

            for (int i = 0; i < Uti.Count; i++)
            {
                BeamUtilization result = Uti[i];
                readableUtilization.Add($"Element {i}: sigma = {result.AxialStress:F2} MPa, tau = {result.ShearStress:F2} MPa, Utilization = {result.Utilization},");
            }

            // Debug: Print Displacements
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Uti:");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Uti.ToString());


            // DISPLACED NODES
            List<Point3d> MovedNodes = ComputeDisplacedNodes(Nodes, displacements, Sfactor);

            // DISPLACED TRUSS
            List<Line> MovedElements = ComputeDisplacedTruss(MovedNodes, elements);


            DA.SetDataList(0, grasshopperVectors);
            DA.SetDataList(1, MovedNodes);
            DA.SetDataList(2, MovedElements);
            DA.SetDataList(3, readableForces);
            DA.SetDataList(4, readableUtilization);
            DA.SetDataList(5, xxx);



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
                //nodes.Add(line.PointAtLength(0.5));
                nodes.Add(line.PointAtEnd);



            }
            nodes = Point3d.CullDuplicates(nodes, 1.0).ToList();
            return nodes;
        }


        List<FEM_Element_BEAM> CreateElements(
    List<Curve> lines,
    List<Point3d> nodes,
    List<Curve> DpL,
    List<double> E,
    List<double> A,
    List<double> Iy,
    List<double> Iz,
    List<double> J,
    List<double> G,
    List<double> Density,
    double DefaultE,
    double DefaultA,
    double DefaultIy,
    double DefaultIz,
    double DefaultJ,
    double DefaultG,
    double DefaultDensity)
        {
            List<FEM_Element_BEAM> elements = new List<FEM_Element_BEAM>();

            for (int i = 0; i < lines.Count; i++)
            {
                Curve line = lines[i];
                int startIndex = nodes.IndexOf(line.PointAtStart);
                int endIndex = nodes.IndexOf(line.PointAtEnd);

                // Find the corresponding property index
                int propertyIndex = DpL.FindIndex(dp => dp.PointAtStart == line.PointAtStart && dp.PointAtEnd == line.PointAtEnd);

                // Assign properties or use default values
                double elementE = (propertyIndex >= 0 && propertyIndex < E.Count) ? E[propertyIndex] : DefaultE;
                double elementA = (propertyIndex >= 0 && propertyIndex < A.Count) ? A[propertyIndex] : DefaultA;
                double elementIy = (propertyIndex >= 0 && propertyIndex < Iy.Count) ? Iy[propertyIndex] : DefaultIy;
                double elementIz = (propertyIndex >= 0 && propertyIndex < Iz.Count) ? Iz[propertyIndex] : DefaultIz;
                double elementJ = (propertyIndex >= 0 && propertyIndex < J.Count) ? J[propertyIndex] : DefaultJ;
                double elementG = (propertyIndex >= 0 && propertyIndex < G.Count) ? G[propertyIndex] : DefaultG;
                double elementDensity = (propertyIndex >= 0 && propertyIndex < Density.Count) ? Density[propertyIndex] : DefaultDensity;

                // Create and add the beam element
                elements.Add(new FEM_Element_BEAM(nodes, startIndex, endIndex, elementE, elementA, elementIy, elementIz, elementJ, elementG, elementDensity));
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
                    fixedNodeIndices.Add(index * 6); //u_x
                    fixedNodeIndices.Add(index * 6 + 1); //u_y
                    fixedNodeIndices.Add(index * 6 + 2); //u_z
                    fixedNodeIndices.Add(index * 6 + 3); //theta_x
                    fixedNodeIndices.Add(index * 6 + 4); //theta_y
                    fixedNodeIndices.Add(index * 6 + 5); //theta_z
                }


            }

            return fixedNodeIndices;
        }

        Vector<double> CreateForceVector(List<Point3d> allNodes, List<Point3d> loadNodes, Vector3d loadForces)
        {
            Vector<double> forceVector = Vector<double>.Build.Dense(allNodes.Count * 6);

            for (int i = 0; i < loadNodes.Count; i++)
            {
                int index = allNodes.FindIndex(node => node.Equals(loadNodes[i]));

                if (index >= 0)
                {
                    forceVector[index * 6] = loadForces.X;
                    forceVector[index * 6 + 1] = loadForces.Y;
                    forceVector[index * 6 + 2] = loadForces.Z;
                    forceVector[index * 6 + 3] = 0; //Moment Mx
                    forceVector[index * 6 + 4] = 0; //Moment My
                    forceVector[index * 6 + 5] = 0; //Moment Mz
                }
            }

            return forceVector;
        }

        Vector<double> Gravity(List<FEM_Element_BEAM> ELEMENTS, List<Point3d> globalNodes)
        {
            double g = 10;
            Vector<double> gravity = Vector<double>.Build.Dense(globalNodes.Count * 6);

            for (int i = 0; i < globalNodes.Count; i++)
            {
                FEM_Element_BEAM correspondingElement = ELEMENTS.FirstOrDefault(e => e.StartNode == i || e.EndNode == i);
                if (correspondingElement != null)
                {
                    double mass = correspondingElement.M; // Accessing the mass property
                    gravity[i * 6 + 2] = -mass * g / 2; // Apply gravity in the Z direction
                }
            }

            return gravity;

        }

        Vector<double> BlockLoad(List<Curve> BLLines, Vector3d BLLoad, List<Point3d> globalNodes)
        {
            Vector<double> blockLoad = Vector<double>.Build.Dense(globalNodes.Count * 6);
            for (int i = 0; i < BLLines.Count; i++)
            {
                Curve line = BLLines[i];
                Point3d start = line.PointAtStart;
                Point3d end = line.PointAtEnd;
                int startIndex = globalNodes.IndexOf(start);
                int endIndex = globalNodes.IndexOf(end);
                blockLoad[startIndex * 6] = BLLoad.X/2;
                blockLoad[startIndex * 6 + 1] = BLLoad.Y/2;
                blockLoad[startIndex * 6 + 2] = BLLoad.Z / 2;
                blockLoad[endIndex * 6] = BLLoad.X / 2;
                blockLoad[endIndex * 6 + 1] = BLLoad.Y / 2;
                blockLoad[endIndex * 6 + 2] = BLLoad.Z / 2;
            }
            return blockLoad;
        }

            List<Vector3d> ConvertToVector3DList(Vector<double> displacements)
        {
            List<Vector3d> vectors = new List<Vector3d>();
            for (int i = 0; i < displacements.Count; i += 6)
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
                int index = i * 6; // 6 DOF per node
                Point3d displacedPoint = new Point3d(
                    originalNodes[i].X + displacements[index] * scaleFactor,
                    originalNodes[i].Y + displacements[index + 1] * scaleFactor,
                    originalNodes[i].Z + displacements[index + 2] * scaleFactor
                );
                displacedNodes.Add(displacedPoint);
            }

            return displacedNodes;
        }



        List<Line> ComputeDisplacedTruss(List<Point3d> displacedNodes, List<FEM_Element_BEAM> elements)
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
 *List<FEM_Element_BEAM> CreateElements(List<Curve> lines, List<Point3d> nodes)
        {
            List<FEM_Element_BEAM> elements = new List<FEM_Element_BEAM>();

            foreach (Curve line in lines)
            {
                int startIndex = nodes.IndexOf(line.PointAtStart);
                int endIndex = nodes.IndexOf(line.PointAtEnd);



                elements.Add(new FEM_Element_BEAM(nodes, startIndex, endIndex, 21, 0.002390, 1.317e-5, 1.009e-6, 8.076, 4.8, 7850));
            }

            return elements;
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
            get { return new Guid("DE89BBDB-3A91-4EAB-B61F-66529CB1C08D"); }
        }
    }
}