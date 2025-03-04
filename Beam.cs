using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal class Beam
    {
        public int id;
        public string name;
        public Curve axis;
        public string section;
        public string materials;
        public double maxDisplacement;
        public Beam() { }

        public Beam(int id, Curve axis)
        {
            this.id = id;
            this.axis = axis;

        }
        public Beam(Curve axis, string section, string materials)
        {
            this.axis = axis;
            this.section = section;
            this.materials = materials;
        }
    }
}
