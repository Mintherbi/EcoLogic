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
    public class Embryo : GH_Component
    {
        public Embryo()
          : base("Reaction-Diffusion", "Reaction-Diffusion",
              "Biomimicry: Embryo growth parameters",
              "EcoLogic", "1.BiomimicrySimulation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N0", "N0", "Number of Initial Cells", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Steps", "Steps", "Number of Steps", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("dt", "dt", "dt", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Lx", "Lx", "Bounding Box X", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Ly", "Ly", "Bounding Box Y", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Lz", "Lz", "Bounding Box Z", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("speed", "speed", "Movement Coefficient Length per dt", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("noise", "noise", "LinearNoise", GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("repulsion_strength", "repulsion_strength", "Repulsion Strength", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("division_radius", "division_radius", "Division Radius", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("division_rate", "division_rate", "Divison Rate", GH_ParamAccess.item, 0.01);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("FinalPoints", "F", "Generated form geometry", GH_ParamAccess.list);
            pManager.AddCurveParameter("Trajectories", "Traj", "Trajectories of Cells", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Set Parameters
            int N0 = new int();
            int Steps = new int();
            double dt = new double();
            double Lx = new double();
            double Ly = new double();
            double Lz = new double();
            double speed = new double();
            double noise = new double();
            double repulsion_strength = new double();
            double division_radius = new double();
            double division_rate = new double();

            if (!DA.GetData(0, ref N0)) return;
            if (!DA.GetData(1, ref Steps)) return;
            if (!DA.GetData(2, ref dt)) return;
            if (!DA.GetData(3, ref Lx)) return;
            if (!DA.GetData(4, ref Ly)) return;
            if (!DA.GetData(5, ref Lz)) return;
            if (!DA.GetData(6, ref speed)) return;
            if (!DA.GetData(7, ref noise)) return;
            if (!DA.GetData(8, ref repulsion_strength)) return;
            if (!DA.GetData(9, ref division_radius)) return;
            if (!DA.GetData(10, ref division_rate)) return;
            #endregion

            string scriptPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                "PythonFiles", "Reaction-Diffusion", "01_Static_Reaction-Diffusion.py");
            scriptPath = Path.GetFullPath(scriptPath);
            string scriptBody = File.ReadAllText(scriptPath);

            var py = PythonScript.Create();

            py.SetVariable("N0", N0);
            py.SetVariable("Steps", Steps);
            py.SetVariable("dt", dt);
            py.SetVariable("Lx", Lx);
            py.SetVariable("Ly", Ly);
            py.SetVariable("Lz", Lz);
            py.SetVariable("speed", speed);
            py.SetVariable("noise", noise);
            py.SetVariable("repulsion_strength", repulsion_strength);
            py.SetVariable("division_radius", division_radius);
            py.SetVariable("division_rate", division_rate);

            py.ExecuteScript(scriptBody);

            var FinalPoints = new List<Point3d>();
            var Trajectories = new List<Curve>();
            string info = "Calculation completed";

            object pyFinalPointsObj = py.GetVariable("FinalPoints");
            object pyTrajectoriesObj = py.GetVariable("Trajectories");
            object infoObj = py.GetVariable("Info");

            if (pyFinalPointsObj != null && pyFinalPointsObj is IEnumerable finalPoints)
            {
                foreach (var obj in finalPoints)
                {
                    if (obj is Point3d point)
                        FinalPoints.Add(point);
                }
            }

            if (pyTrajectoriesObj != null && pyTrajectoriesObj is IEnumerable trajectories)
            {
                foreach (var obj in trajectories)
                {
                    if (obj is Curve curve)
                        Trajectories.Add(curve);
                }
            }

            if (infoObj != null)
                info = infoObj.ToString();
                
            DA.SetDataList(0, FinalPoints);
            DA.SetDataList(1, Trajectories);
            DA.SetData(2, info);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Embryo_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Embryo_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("179BD18B-BE1B-4BD3-B516-740C1E32ACB6");
    }
}
