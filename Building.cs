using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NTNU_Pc2025
{
    internal class Building
    {
        public int id;
        public string name;
        public List<Curve> columns;
        public List<Curve> beams;
        public List<Brep> slabs;
        public List<Point3d> nodes;
        public List<Beam> buildingBeams;


        public Building()
        {
            //empty constructor
        }
        public Building(string _name)
        {
            name = _name;
        }

        public Building(string _name,int _id)
        {
            name = _name;
            id = _id;
        }
    }
}
