using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class component2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the component2 class.
        /// </summary>
        public component2()
          : base("NTNUseries", "Nickname",
              "Description",
              "NTNUPC2025", "Constructor")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("start", "st", "", GH_ParamAccess.item,0); //0
            pManager.AddNumberParameter("step", "s", "", GH_ParamAccess.item,1); //1
            pManager.AddIntegerParameter("count", "c", "", GH_ParamAccess.item,10); //2
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("series", "srs", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double start = 0;
            double step = 0;
            int count = 0;

            DA.GetData(0, ref start);
            DA.GetData(1, ref step);
            DA.GetData(2, ref count);
            List<double> series = new List<double>();

            for (int i = 0; i < count; i++)
            {
                series.Add(step * Convert.ToDouble(i));
            }


            DA.SetDataList(0, series);
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
            get { return new Guid("7FE9F5A5-EA55-4BDB-9F3A-8F0C24639982"); }
        }
    }
}