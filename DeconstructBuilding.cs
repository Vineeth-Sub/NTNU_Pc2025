using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class DeconstructBuilding : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBuilding class.
        /// </summary>
        public DeconstructBuilding()
          : base("DeconstructBuilding", "Nickname",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("info", "", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("columns", "cs", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("beams", "cs", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("slabs", "sbs", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Building building = new Building();
            DA.GetData(0, ref building);

            List<string> list = new List<string>();
            list.Add("this is Building class");
            list.Add("name is:" + building.name);
            list.Add("id is:" + building.name);

            List<Curve> cols = new List<Curve>();
            List<Curve> bms = new List<Curve>();
            if(building.columns != null)
                cols.AddRange(building.columns);
            if (building.beams != null)
                bms.AddRange(building.beams);


            List<Brep> brs = new List<Brep>();
            if (building.slabs != null)
                brs.AddRange(building.slabs);
            DA.SetDataList(0, list);
            DA.SetDataList(1, cols);
            DA.SetDataList(2, bms);
            DA.SetDataList(3, brs);



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
            get { return new Guid("4846DC59-DB1C-4FD0-8DF3-4C1E39D75D6C"); }
        }
    }
}