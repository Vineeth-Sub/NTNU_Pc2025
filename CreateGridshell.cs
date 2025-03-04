using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class createGridshell : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the createGridshell class.
        /// </summary>
        public createGridshell()
          : base("createGridshell", "Nickname",
              "Description",
              "NTNUPC2025", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("points", "pts", "support clockwise", GH_ParamAccess.list, new List<Point3d>() { new Point3d(0,0,0), new Point3d(10, 0, 0) , new Point3d(10, 10, 0) , new Point3d(0, 10, 0) });
            pManager.AddIntegerParameter("div1", "d1", "divide in 1 direct", GH_ParamAccess.item,10);
            pManager.AddIntegerParameter("div2", "d2", "divide in 2 direc", GH_ParamAccess.item,12);
            pManager.AddNumberParameter("edgeheight", "eh", "height of the edge", GH_ParamAccess.item,5);
            pManager.AddNumberParameter("centerheight", "ch", "height of the center", GH_ParamAccess.item,10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("buildig", "b", "building, a gridshell", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();
            int div1 = 0;
            int div2 = 0;
            double eh = 0;
            double ch = 0;
            DA.GetDataList(0, pts);
            DA.GetData(1, ref div1);
            DA.GetData(2, ref div2);
            DA.GetData(3, ref eh);
            DA.GetData(4, ref ch);

            Building building = new Building();
            Line l0 = new Line(pts[0], pts[1]);
            Line l1 = new Line(pts[3], pts[2]);


            Line a1 = new Line(l0.PointAt(0), l1.PointAt(0));
            Line a2 = new Line(l0.PointAt(0.5), l1.PointAt(0.5));
            Line a3 = new Line(l0.PointAt(1.0), l1.PointAt(1.0));


            Point3d p11 = a1.PointAt(0);
            Point3d p12 = a1.PointAt(0.5);
            Point3d p13 = a1.PointAt(1);

            Point3d p21 = a2.PointAt(0);
            Point3d p22 = a2.PointAt(0.5);
            Point3d p23 = a2.PointAt(1);

            Point3d p31 = a3.PointAt(0);
            Point3d p32 = a3.PointAt(0.5);
            Point3d p33 = a3.PointAt(1);


            List<Point3d> pts1 = new List<Point3d>() { p11, new Point3d(p12.X, p12.Y, p12.Z + eh), p13 };
            List<Point3d> pts2 = new List<Point3d>() {new Point3d(p21.X, p21.Y, p21.Z + eh), new Point3d(p22.X, p22.Y, p22.Z + ch), new Point3d(p23.X, p23.Y, p23.Z + eh) };
            List<Point3d> pts3 = new List<Point3d>() { p31, new Point3d(p32.X, p32.Y, p32.Z + eh), p33 };

            Curve c1 = Curve.CreateInterpolatedCurve(pts1, 2);
            Curve c2 = Curve.CreateInterpolatedCurve(pts2, 2);
            Curve c3 = Curve.CreateInterpolatedCurve(pts3, 2);
            List<Curve> crvs = new List<Curve>() { c1, c2, c3 };
            building.columns = crvs;

            Brep brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            building.slabs = new List<Brep>() { brep };

            Surface srf = brep.Surfaces[0];

            Interval i1 = srf.Domain(0);
            Interval i2 = srf.Domain(1);

            List<Curve> bms1 = new List<Curve>();
            List<Curve> bms2 = new List<Curve>();

            List<Point3d> nodes = new List<Point3d>();

            for (int i = 0; i < div1 + 1; i++)
            {
                for (int j = 0; j < div2 +1 ; j++)
                {
                    double p1 = 1.0 / Convert.ToDouble(div1) * Convert.ToDouble(i);
                    double p2 = 1.0 / Convert.ToDouble(div2) * Convert.ToDouble(j);
                    Point3d node = srf.PointAt(i1.ParameterAt(p1), i2.ParameterAt(p2));
                    nodes.Add(node);
                }

            }
            building.nodes = nodes;


                    for (int i=0; i <div1+1; i++)
            {
                for (int j=0; j<div2; j++)
                {
                    double p1 = 1.0 / Convert.ToDouble(div1) * Convert.ToDouble(i);
                    double p2 = 1.0 / Convert.ToDouble(div2) * Convert.ToDouble(j);
                    //double p3 = 1.0 / Convert.ToDouble(div1) * Convert.ToDouble(i+1);
                    double p4 = 1.0 / Convert.ToDouble(div2) * Convert.ToDouble(j+1);
                    Point3d n1 = srf.PointAt(i1.ParameterAt(p1),i2.ParameterAt(p2));
                    Point3d n2 = srf.PointAt(i1.ParameterAt(p1), i2.ParameterAt(p4));
                    bms1.Add(new Line(n1,n2).ToNurbsCurve());

                }
            }
            //////
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2+1; j++)
                {
                    double p1 = 1.0 / Convert.ToDouble(div1) * Convert.ToDouble(i);
                    double p2 = 1.0 / Convert.ToDouble(div2) * Convert.ToDouble(j);
                    double p3 = 1.0 / Convert.ToDouble(div1) * Convert.ToDouble(i+1);
                    //double p4 = 1.0 / Convert.ToDouble(div2) * Convert.ToDouble(j + 1);
                    Point3d n1 = srf.PointAt(i1.ParameterAt(p1), i2.ParameterAt(p2));
                    Point3d n2 = srf.PointAt(i1.ParameterAt(p3), i2.ParameterAt(p2));
                    bms2.Add(new Line(n1, n2).ToNurbsCurve());

                }
            }
            building.beams = bms1;
            building.beams.AddRange(bms2);




            DA.SetData(0, building);
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
            get { return new Guid("51CDB415-5709-4278-B027-3545CB068F3F"); }
        }
    }
}