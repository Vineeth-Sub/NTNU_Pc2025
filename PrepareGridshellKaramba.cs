using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Net;
using System.IO;

namespace NTNU_Pc2025
{
    public class PrepareGridshellKaramba : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PrepareGridshellKaramba class.
        /// </summary>
        public PrepareGridshellKaramba()
          : base("PrepareGridshellKaramba", "Nickname",
              "Description",
              "NTNUPC2025", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("building", "b", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("zones", "zs", "load zones", GH_ParamAccess.list);
            pManager.AddNumberParameter("loadValues", "lvs", "load values zone", GH_ParamAccess.list);
            pManager.AddCurveParameter("sectionzones", "szs", "section zones", GH_ParamAccess.list);
            pManager.AddTextParameter("sectionValues", "svls", "section values zone", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes","ns","list of all nodes",GH_ParamAccess.list);
            pManager.AddGenericParameter("Beams", "bs", "list of all beams", GH_ParamAccess.list);
            pManager.AddPointParameter("SupportNodes", "sn", "list of all supportnodes", GH_ParamAccess.list);
            pManager.AddPointParameter("LoadNodes", "ls", "list of all loadnodes", GH_ParamAccess.tree);
            pManager.AddVectorParameter("LoadValues", "lvs", "list of load values", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Building building = new Building();
            List<Curve> zns = new List<Curve>();
            List<double> vls = new List<double>();
            List<Curve> szns = new List<Curve>();
            List<String> svls = new List<String>();

            DA.GetData(0, ref building);
            DA.GetDataList(1, zns);
            DA.GetDataList(2, vls);
            DA.GetDataList(3, szns);
            DA.GetDataList(4, svls);

            var nodes = building.nodes;
            var beams = new List<Beam>();

            foreach (var b in building.beams)
                beams.Add(new Beam(b, "IPE100", "S355"));


            for(int i = 0; i< beams.Count; i++)
            {
                int index = 0;
                foreach (var sectionZone in szns)
                {
                    Point3d mp = beams[i].axis.PointAt(beams[i].axis.Domain.ParameterAt(0.5));
                    var pointCont = sectionZone.Contains(mp);//SHIT THIS DOES NOT WORK
                    if (pointCont == PointContainment.Inside)
                    {
                        string sectionName = svls[index];
                        beams.Add(new Beam(beams[i].axis, sectionName, "S355"));
                    }
                }
            }

            var sups = new List<Point3d>();
            var loads = new List<Point3d>();


            foreach (var n in nodes) //Support
            {
                double z = n.Z;
                if (z == 0)
                    sups.Add(n);
            }



            List<Vector3d> loadVectors = new List<Vector3d>();

            GH_Structure<GH_Point> dataTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> dataTreeVector = new GH_Structure<GH_Vector>();

            //int ixOfZone = 0;
            //Curve crvZone = zns[0];
            double loadeValue = -5.0;

            for (int i = 0; i < zns.Count; i++)
            {
                Curve crvZone = zns[i];
                var path = new GH_Path(i);

                foreach (var n in nodes)
                {
                    var pc = crvZone.Contains(new Point3d(n.X, n.Y, 0));
                    if (pc == PointContainment.Inside)
                    {
                        if (vls.Count < i)
                            loadeValue = vls[i];
                        dataTreeVector.Append(new GH_Vector(new Vector3d(0, 0, vls[i])), path);
                        dataTree.Append(new GH_Point(n), path);
                    }
                }
            }
            




            DA.SetDataList(0, nodes);
            DA.SetDataList(1, beams);
            DA.SetDataList(2, sups);
            DA.SetDataTree(3, dataTree);
            DA.SetDataTree(4, dataTreeVector);
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
            get { return new Guid("B77A4E61-D157-4E99-90E7-D9FA2FDB2698"); }
        }
    }
}