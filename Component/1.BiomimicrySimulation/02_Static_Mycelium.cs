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
    public class Mycelium : GH_Component
    {
        public Mycelium()
          : base("Mycelium", "Mycelium",
              "Biomimicry: 3D mycelium network growth simulation",
              "EcoLogic", "1.BiomimicrySimulation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartPts", "StartPts", "Starting seed points for mycelium growth", GH_ParamAccess.list);
            pManager.AddPointParameter("FoodPts", "FoodPts", "Food/nutrient points that attract growth", GH_ParamAccess.list);
            pManager.AddNumberParameter("Step", "Step", "Growth step size", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("SenseR", "SenseR", "Sensing radius for detecting food", GH_ParamAccess.item, 30.0);
            pManager.AddNumberParameter("KillR", "KillR", "Kill radius - distance at which food is consumed", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("MaxIter", "MaxIter", "Maximum number of growth iterations", GH_ParamAccess.item, 200);
            pManager.AddNumberParameter("GrowR", "GrowR", "Growth radius - limits growth area around start points", GH_ParamAccess.item, 178.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Segments", "Segments", "Generated mycelium growth segments", GH_ParamAccess.list);
            pManager.AddPointParameter("Tips", "Tips", "Growth tip points", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Set Parameters
            var StartPts = new List<Point3d>();
            var FoodPts = new List<Point3d>();
            double Step = 1.0;
            double SenseR = 5.0;
            double KillR = 1.0;
            int MaxIter = 100;
            double GrowR = 50.0;

            if (!DA.GetDataList(0, StartPts)) return;
            if (!DA.GetDataList(1, FoodPts)) return;
            if (!DA.GetData(2, ref Step)) return;
            if (!DA.GetData(3, ref SenseR)) return;
            if (!DA.GetData(4, ref KillR)) return;
            if (!DA.GetData(5, ref MaxIter)) return;
            if (!DA.GetData(6, ref GrowR)) return;
            #endregion

            string scriptPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                "..", "..", "..", "PythonFiles", "Mycelium", "01_Static_Mycelium.py");
            scriptPath = Path.GetFullPath(scriptPath);
            string scriptBody = File.ReadAllText(scriptPath);

            var py = PythonScript.Create();

            py.SetVariable("StartPts", StartPts);
            py.SetVariable("FoodPts", FoodPts);
            py.SetVariable("Step", Step);
            py.SetVariable("SenseR", SenseR);
            py.SetVariable("KillR", KillR);
            py.SetVariable("MaxIter", MaxIter);
            py.SetVariable("GrowR", GrowR);

            py.ExecuteScript(scriptBody);

            var Segments = new List<Curve>();
            var Tips = new List<Point3d>();

            object pySegmentsObj = py.GetVariable("Segments");
            object pyTipsObj = py.GetVariable("Tips");

            if (pySegmentsObj != null && pySegmentsObj is IEnumerable segments)
            {
                foreach (var obj in segments)
                {
                    if (obj is Curve curve)
                        Segments.Add(curve);
                }
            }

            if (pyTipsObj != null && pyTipsObj is IEnumerable tips)
            {
                foreach (var obj in tips)
                {
                    if (obj is Point3d point)
                        Tips.Add(point);
                }
            }

            DA.SetDataList(0, Segments);
            DA.SetDataList(1, Tips);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Mycelium_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Mycelium_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("A8B3C4D5-E6F7-8901-2345-6789ABCDEF01");
    }
}
