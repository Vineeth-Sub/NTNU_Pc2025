using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class Sorting : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Sorting()
          : base("MyComponent1", "Nickname",
              "Description",
              "NTNUPC2025", "Algorithims")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("pointlist","pls","list og points", GH_ParamAccess.list);
            //pManager.AddPointParameter("refPt", "rpt", "refernce point", GH_ParamAccess.item);
            pManager.AddNumberParameter("zs", "z", "list of doubles as parameter for chopping", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("spointlist", "spls", "list og points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();
            DA.GetDataList(0,points);

            //Point3d rpt = new Point3d();
            //DA.GetData(1, ref rpt);


            List<Point3d> sortedPointsZ = points.OrderBy(z => z.Z).ToList();
            List<double> zs = new List<double>();
            DA.GetDataList(0,sortedPointsZ);
            DA.GetDataList(1, zs);
            zs.Sort();

            GH_Structure<GH_Point> sortedPoints = new GH_Structure<GH_Point>();


            for (int j = 0; j < sortedPointsZ.Count; j++)
            {
                Point3d pt = sortedPointsZ[j];
                for (int i = 0; i < sortedPointsZ.Count; i++)
                {
                    var p = new GH_Path(i);
                    if (pt.Z > zs[i] && pt.Z < zs[i + 1])
                        sortedPoints.Append(new GH_Point(pt), p);
                }
            }


            DA.SetDataTree(0, sortedPoints);


            /*
            List<Point3d> sortedPoints = new List<Point3d>();
            Dictionary<Point3d,double > pointsDistances = new Dictionary<Point3d, double>();
            foreach (var pt in points)
            {
                double distance = pt.DistanceTo(rpt);
                pointsDistances.Add(pt, distance);
            }
            var sorteddist = pointsDistances.OrderBy(v => v.Value);


            foreach (var pd in sorteddist)
            {
                sortedPoints.Add(pd.Key);
            }
            DA.SetDataList(0, sortedPoints);
            */


            //List<Point3d> sortedPointsX = points.OrderBy(x => x.X).ToList();
            //List<Point3d> sortedPointsY = sortedPointsX.OrderBy(y => y.Y).ToList();
            //DA.SetDataList(0,sortedPointsY);
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
            get { return new Guid("F9448534-476D-41D6-BABB-D35C93CB4259"); }
        }
    }
}