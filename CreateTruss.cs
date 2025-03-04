using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class CreateTruss : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateTruss class.
        /// </summary>
        public CreateTruss()
          : base("CreateTruss", "Nickname",
              "Description",
               "NTNUPC2025", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("bottomAxis","bA","",GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "h", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("divisions", "d", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("type", "t", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("building", "b", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Line axis = new Line();
            double height = 0.0;
            int div = 0;
            int type = 0;

            DA.GetData(0, ref axis);
            DA.GetData(1, ref height);
            DA.GetData(2, ref div);
            DA.GetData(3, ref type);

            Building building = new Building();
            building.name = "truss system";
            building.id = 1;

            List<Point3d> bottomNodes = createNodesFromLine(axis, div);

            var t = Transform.Translation(new Vector3d(0, 0, height));
            axis.Transform(t);


            Curve crvAxisT = axis.ToNurbsCurve();
            double[] pmsT = crvAxisT.DivideByCount(div, true);

            List<Point3d> topNodes = createNodesFromLine(axis, div);

            List<Curve> columns = createColumnsFromPoints(bottomNodes,topNodes);
            building.columns = columns;

            List <Curve> tbeams = createBeamsFromPoints(topNodes);
            List<Curve> bbeams = createBeamsFromPoints(bottomNodes);
            building.beams = bbeams;
            building.beams.AddRange(tbeams);
            building.beams.AddRange(createBarsFromPoints(type, bottomNodes, topNodes));

            DA.SetData(0, building);

        }

        List<Curve> createBarsFromPoints(int type, List<Point3d> bottomPoints, List<Point3d> topPoints)
        {
            var bars = new List<Curve>();

            if (type == 0)
            {

            }
            else if (type == 1)
            {
                for (int i = 0; i < bottomPoints.Count - 1; i++)
                {
                    Line ba = new Line(bottomPoints[i], topPoints[i + 1]);
                    bars.Add(ba.ToNurbsCurve());
                }
            }
            else if (type == 2)
            {
                for (int i = 0; i < bottomPoints.Count - 1; i++)
                {
                    Line ba = new Line(bottomPoints[i+1], topPoints[i]);
                    bars.Add(ba.ToNurbsCurve());
                }
            }

            else if (type == 3)
            {
                for (int i = 0; i < bottomPoints.Count - 1; i = i+2)
                {
                    Line ba1 = new Line(bottomPoints[i + 1], topPoints[i]);
                    bars.Add(ba1.ToNurbsCurve());
                }
                for (int i = 1; i < bottomPoints.Count - 1; i = i+2)
                {
                    Line ba2 = new Line(bottomPoints[i], topPoints[i + 1]);
                    bars.Add(ba2.ToNurbsCurve());

                    
                }
            }

            return bars;
        }



        List<Curve> createColumnsFromPoints(List<Point3d> bottomPoints, List<Point3d> topPoints)
        {
            var cls = new List<Curve>();
                for (int i = 0; i < bottomPoints.Count; i++)
            {
                Line cl = new Line(bottomPoints[i], topPoints[i]);
                cls.Add(cl.ToNurbsCurve());
            }
            return cls;
        }

        List<Curve> createBeamsFromPoints(List<Point3d> points)
        {
            var bms = new List<Curve>();
            for (int i = 0; i < points.Count-1; i++)
            {
                Line bm= new Line(points[i], points[i+1]);
                bms.Add(bm.ToNurbsCurve());
            }
            return bms;
        }


        List<Point3d> createNodesFromLine(Line line, int div)
        {
            List<Point3d> nodes = new List<Point3d>();
            Curve crv = line.ToNurbsCurve();
            double [] pms = crv.DivideByCount(div, true);

            foreach (var p in pms)
                nodes.Add(crv.PointAt(p));

            return nodes;
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
            get { return new Guid("DA02A2DE-218A-463E-95E7-9E8067E94EAA"); }
        }
    }
}