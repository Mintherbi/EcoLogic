using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Runtime;  

using System.IO;
using System.Diagnostics;
using System.Collections;

namespace PointCloudDiffusion.Component.Biomimicry
{
    public class VenusFlowerBasket : GH_Component
    {
        public VenusFlowerBasket()
          : base("Venus Flower Basket", "VFB",
              "Biomimicry: Venus Flower Basket structure generation",
              "EcoLogic", "1.BiomimicrySimulation")
        {
        }

        // Inputs:
        //     surface: Surface - Target surface (if None, creates cylinder)
        //     height: float - Overall height (for default cylinder)
        //     diameter: float - Base diameter (for default cylinder)
        //     taper: float - Taper ratio (0-1, for default cylinder)
        //     fiber_diameter: float - Base thickness of fibers
        //     cell_density_u: int - Number of cells in U direction
        //     cell_density_v: int - Number of cells in V direction
        //     diagonal_guides: int - Number of diagonal guide lines
        //     include_framework: bool - Include diagonal lattice framework
        //     framework_thickness: float - Multiplier for framework thickness
        //     pattern_type: int - 0=Voronoi only, 1=Framework only, 2=Combined
        //     seed: int - Random seed for cell distribution
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Surface", "Surface", "Target surface (if None, creates cylinder)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Overall height (for default cylinder)", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Diameter", "Diameter", "Base diameter (for default cylinder)", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Taper", "Taper", "Taper ratio (0-1, for default cylinder)", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("FiberDiameter", "FiberDiameter", "Base thickness of fibers", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("CellDensityU", "CellDensityU", "Number of cells in U direction", GH_ParamAccess.item, 12);
            pManager.AddIntegerParameter("CellDensityV", "CellDensityV", "Number of cells in V direction", GH_ParamAccess.item, 40);
            pManager.AddIntegerParameter("DiagonalGuides", "DiagonalGuides", "Number of diagonal guide lines", GH_ParamAccess.item, 15);
            pManager.AddBooleanParameter("IncludeFramework", "IncludeFramework", "Include diagonal lattice framework", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("FrameworkThickness", "FrameworkThickness", "Multiplier for framework thickness", GH_ParamAccess.item, 0.3);
            pManager.AddIntegerParameter("PatternType", "PatternType", "0=Voronoi only, 1=Framework only, 2=Combined", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Seed", "Seed", "Random seed for cell distribution", GH_ParamAccess.item, 10);

            // Make surface optional
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("AllFibers", "AllFibers", "All combined geometry", GH_ParamAccess.list);
            pManager.AddGeometryParameter("VoronoiFibers", "VoronoiFibers", "Just Voronoi cells", GH_ParamAccess.list);
            pManager.AddGeometryParameter("FrameworkFibers", "FrameworkFibers", "Just diagonal framework", GH_ParamAccess.list);
            pManager.AddPointParameter("SeedPoints", "SeedPoints", "Seed points for visualization", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Set Parameters
            ///Input Parameter
            GeometryBase surface = null;
            double height = 1.0;
            double diameter = 3.0;
            double taper = 0.1;
            double fiber_diameter = 1.0;
            int cell_density_u = 12;
            int cell_density_v = 40;
            int diagonal_guides = 15;
            bool include_framework = false;
            double framework_thickness = 0.3;
            int pattern_type = 0;
            int seed = 10;

            if (!DA.GetData(0, ref surface)) return;
            if (!DA.GetData(1, ref height)) return;
            if (!DA.GetData(2, ref diameter)) return;
            if (!DA.GetData(3, ref taper)) return;
            if (!DA.GetData(4, ref fiber_diameter)) return;
            if (!DA.GetData(5, ref cell_density_u)) return;
            if (!DA.GetData(6, ref cell_density_v)) return;
            if (!DA.GetData(7, ref diagonal_guides)) return;
            if (!DA.GetData(8, ref include_framework)) return;
            if (!DA.GetData(9, ref framework_thickness)) return;
            if (!DA.GetData(10, ref pattern_type)) return;
            if (!DA.GetData(11, ref seed)) return;
            #endregion

            string scriptPath = @"/Users/minsupchung/Library/Mobile Documents/com~apple~CloudDocs/GitHub/Grasshopper Project/EcoLogic/PythonFiles/Venus Flower Basket/01_Static_Venus-Flower-Basket.py";
            string scriptBody = File.ReadAllText(scriptPath);

            var py = PythonScript.Create();

            py.SetVariable("surface", surface);
            py.SetVariable("height", height);
            py.SetVariable("diameter", diameter);
            py.SetVariable("taper", taper);
            py.SetVariable("fiber_diameter", fiber_diameter);
            py.SetVariable("cell_density_u", cell_density_u);
            py.SetVariable("cell_density_v", cell_density_v);
            py.SetVariable("diagonal_guides", diagonal_guides);
            py.SetVariable("include_framework", include_framework);
            py.SetVariable("framework_thickness", framework_thickness);
            py.SetVariable("pattern_type", pattern_type);
            py.SetVariable("seed", seed);

            py.ExecuteScript(scriptBody);

            var AllFibers = new List<GeometryBase>();
            var VoronoiFibers = new List<GeometryBase>();
            var FrameworkFibers = new List<GeometryBase>();
            var SeedPoints = new List<Point3d>();

            object pyAllFibersObj = py.GetVariable("a");
            object pyVoronoiFibersObj = py.GetVariable("b");
            object pyFrameworkFibersObj = py.GetVariable("c");
            object pySeedPointsObj = py.GetVariable("d");

            if (pyAllFibersObj != null && pyAllFibersObj is IEnumerable allFibers)
            {
                foreach (var obj in allFibers)
                {
                    if (obj is GeometryBase geom)
                        AllFibers.Add(geom);
                }
            }

            if (pyVoronoiFibersObj != null && pyVoronoiFibersObj is IEnumerable voronoiFibers)
            {
                foreach (var obj in voronoiFibers)
                {
                    if (obj is GeometryBase geom)
                        VoronoiFibers.Add(geom);
                }
            }

            if (pyFrameworkFibersObj != null && pyFrameworkFibersObj is IEnumerable frameworkFibers)
            {
                foreach (var obj in frameworkFibers)
                {
                    if (obj is GeometryBase geom)
                        FrameworkFibers.Add(geom);
                }
            }

            if (pySeedPointsObj != null && pySeedPointsObj is IEnumerable seedPoints)
            {
                foreach (var obj in seedPoints)
                {
                    if (obj is Point3d point)
                        SeedPoints.Add(point);
                }
            }

            DA.SetDataList(0, AllFibers);
            DA.SetDataList(1, VoronoiFibers);
            DA.SetDataList(2, FrameworkFibers);
            DA.SetDataList(3, SeedPoints);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.VenusFlowerBasket_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "VenusFlowerBasket_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("53D6C0E5-35BE-4C6B-8A55-AA9A89A38787");
    }
}
