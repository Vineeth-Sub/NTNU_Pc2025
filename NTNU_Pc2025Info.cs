using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace NTNU_Pc2025
{
    public class NTNU_Pc2025Info : GH_AssemblyInfo
    {
        public override string Name => "NTNU_Pc2025";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("88e17f99-666d-4ade-b66e-713e140fdcf9");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}