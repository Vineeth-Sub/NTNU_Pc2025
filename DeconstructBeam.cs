using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    public class DeconstructBeam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBeam class.
        /// </summary>
        public DeconstructBeam()
          : base("DeconstructBeam", "Nickname",
              "Description",
               "NTNUPC2025", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("beam", "b", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("axis", "a", "", GH_ParamAccess.item);
            pManager.AddTextParameter("section", "s", "", GH_ParamAccess.item);
            pManager.AddTextParameter("material", "m", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Beam beam= new Beam();
            DA.GetData(0, ref beam);

            Curve a = beam.axis;
            string sec = beam.section;
            string mat = beam.materials;

            DA.SetData(0, a);
            DA.SetData(1, sec);
            DA.SetData(2, mat);


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
            get { return new Guid("CC18FFF7-C0C4-4689-9729-7D1261631068"); }
        }
    }
}