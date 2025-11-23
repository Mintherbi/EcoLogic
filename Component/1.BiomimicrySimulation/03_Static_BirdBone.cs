using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Runtime;
using System.IO;
using System.Collections;

namespace PointCloudDiffusion.Component.Biomimicry
{
    public class BirdBone : GH_Component
    {
        public BirdBone()
          : base("Bird Bone", "BirdBone",
              "Biomimicry: Bird bone trabecular structure generator with irregular floors",
              "EcoLogic", "1.BiomimicrySimulation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("BoundaryBrep", "Boundary", "Boundary brep for trabecular structure (if None, creates default box)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("FloorCount", "FloorCount", "Number of floors in the structure", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("FloorHeight", "FloorHeight", "Height between floors", GH_ParamAccess.item, 4.0);
            pManager.AddNumberParameter("VoxelSize", "VoxelSize", "Size of each voxel in the grid", GH_ParamAccess.item, 1.5);
            pManager.AddNumberParameter("LatticeThreshold", "Threshold", "Threshold for lattice generation (higher = more porous)", GH_ParamAccess.item, 0.25);
            pManager.AddVectorParameter("WindDirection", "WindDir", "Wind direction vector for lateral loads", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("LatticeLines", "LatticeLines", "Generated lattice structure lines", GH_ParamAccess.list);
            pManager.AddBrepParameter("FloorSlabs", "FloorSlabs", "Irregular floor slab surfaces", GH_ParamAccess.list);
            pManager.AddCurveParameter("Columns", "Columns", "Primary structural columns", GH_ParamAccess.list);
            pManager.AddPointParameter("StressField", "StressField", "Points showing stress distribution", GH_ParamAccess.list);
            pManager.AddPointParameter("DensityMap", "DensityMap", "Points showing material density", GH_ParamAccess.list);
            pManager.AddBrepParameter("ThickLattice", "ThickLattice", "Solid lattice members with thickness", GH_ParamAccess.list);
            pManager.AddBrepParameter("ThickFloors", "ThickFloors", "Thick irregular floor slabs", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Set Parameters
            Brep boundary_brep = null;
            int floor_count = 5;
            double floor_height = 4.0;
            double voxel_size = 1.5;
            double lattice_threshold = 0.25;
            Vector3d wind_direction = Vector3d.Unset;

            DA.GetData(0, ref boundary_brep);
            if (!DA.GetData(1, ref floor_count)) return;
            if (!DA.GetData(2, ref floor_height)) return;
            if (!DA.GetData(3, ref voxel_size)) return;
            if (!DA.GetData(4, ref lattice_threshold)) return;
            DA.GetData(5, ref wind_direction);
            #endregion

            string scriptPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                "..", "..", "..", "PythonFiles", "Birdbond", "01_Static_Birdbone.py");
            scriptPath = Path.GetFullPath(scriptPath);
            string scriptBody = File.ReadAllText(scriptPath);

            var py = PythonScript.Create();

            py.SetVariable("boundary_brep", boundary_brep);
            py.SetVariable("floor_count", floor_count);
            py.SetVariable("floor_height", floor_height);
            py.SetVariable("voxel_size", voxel_size);
            py.SetVariable("lattice_threshold", lattice_threshold);
            
            if (wind_direction != Vector3d.Unset)
            {
                py.SetVariable("wind_direction", wind_direction);
            }

            py.ExecuteScript(scriptBody);

            var LatticeLines = new List<Curve>();
            var FloorSlabs = new List<Brep>();
            var Columns = new List<Curve>();
            var StressField = new List<Point3d>();
            var DensityMap = new List<Point3d>();
            var ThickLattice = new List<Brep>();
            var ThickFloors = new List<Brep>();

            object pyLatticeObj = py.GetVariable("lattice_lines");
            if (pyLatticeObj != null && pyLatticeObj is IEnumerable latticeLines)
            {
                foreach (var obj in latticeLines)
                {
                    if (obj is Curve curve)
                        LatticeLines.Add(curve);
                }
            }

            object pyFloorSlabsObj = py.GetVariable("floor_slabs");
            if (pyFloorSlabsObj != null && pyFloorSlabsObj is IEnumerable floorSlabs)
            {
                foreach (var obj in floorSlabs)
                {
                    if (obj is Brep brep)
                        FloorSlabs.Add(brep);
                }
            }

            object pyColumnsObj = py.GetVariable("columns");
            if (pyColumnsObj != null && pyColumnsObj is IEnumerable columns)
            {
                foreach (var obj in columns)
                {
                    if (obj is Curve curve)
                        Columns.Add(curve);
                }
            }

            object pyStressObj = py.GetVariable("stress_field");
            if (pyStressObj != null && pyStressObj is IEnumerable stressField)
            {
                foreach (var obj in stressField)
                {
                    if (obj is Point3d point)
                        StressField.Add(point);
                }
            }

            object pyDensityObj = py.GetVariable("density_map");
            if (pyDensityObj != null && pyDensityObj is IEnumerable densityMap)
            {
                foreach (var obj in densityMap)
                {
                    if (obj is Point3d point)
                        DensityMap.Add(point);
                }
            }

            object pyThickLatticeObj = py.GetVariable("thick_lattice");
            if (pyThickLatticeObj != null && pyThickLatticeObj is IEnumerable thickLattice)
            {
                foreach (var obj in thickLattice)
                {
                    if (obj is Brep brep)
                        ThickLattice.Add(brep);
                }
            }

            // Get thick floors
            object pyThickFloorsObj = py.GetVariable("thick_floors");
            if (pyThickFloorsObj != null && pyThickFloorsObj is IEnumerable thickFloors)
            {
                foreach (var obj in thickFloors)
                {
                    if (obj is Brep brep)
                        ThickFloors.Add(brep);
                }
            }

            DA.SetDataList(0, LatticeLines);
            DA.SetDataList(1, FloorSlabs);
            DA.SetDataList(2, Columns);
            DA.SetDataList(3, StressField);
            DA.SetDataList(4, DensityMap);
            DA.SetDataList(5, ThickLattice);
            DA.SetDataList(6, ThickFloors);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.BirdBone_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "BirdBone_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("B7D8E9F0-1234-5678-9ABC-DEF012345678");
    }
}
