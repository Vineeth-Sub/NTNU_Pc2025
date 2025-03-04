using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace NTNU_Pc2025
{
    public class splitBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the splitBrep class.
        /// </summary>
        public splitBrep()
          : base("splitBrep", "splBr",
              "This algorithm splits brep into curves",
              "NTNUPC2025", "Breps")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("brep","b","",GH_ParamAccess.item);
            pManager.AddIntegerParameter("chops", "d", "", GH_ParamAccess.item,10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("frame","","",GH_ParamAccess.list);
            pManager.AddCurveParameter("curve", "c", "", GH_ParamAccess.list);
            pManager.AddTextParameter("info", "i", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = new Brep();
            int chops = 0;

            DA.GetData(0, ref brep);
            DA.GetData(1, ref chops);
            List<Plane> planes = new List<Plane> ();
            List<Curve> curves = new List<Curve> ();
            List<string> info = new List<string> ();

            var bb = brep.GetBoundingBox(true);
            double minZ = bb.Min.Z;
            double maxZ = bb.Max.Z;
            Interval iZ = new Interval(minZ, maxZ);


            //create the list of parameters for chopping
            //int count = 10;
            double[] prs = new double[chops];
            for (int i = 0; i < chops; i++)
            {
                prs[i] = 1.0/(Convert.ToDouble(chops) -1) * Convert.ToDouble(i);
            }
            info.Add("List of parameters to chop brep");
            foreach (var p in prs)
            {
                info.Add(p.ToString());
                double z = iZ.ParameterAt(p);
                Plane pl = new Plane(new Point3d(0,0,z), new Vector3d(0,0,1));
                planes.Add(pl);

                Curve[] crvs;
                Point3d[] pts;
                Intersection.BrepPlane(brep, pl, 0.00001, out crvs, out pts);
                curves.AddRange(crvs);
            }
            DA.SetDataList(0, planes);
            DA.SetDataList(1, curves);
            DA.SetDataList(2, info);
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
            get { return new Guid("793C9D76-C9FC-4162-AE60-FF0458038F04"); }
        }
    }
}