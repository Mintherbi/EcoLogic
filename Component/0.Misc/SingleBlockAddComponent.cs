using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

using static PointCloudDiffusion.Utils.Utils;

namespace PointCloudDiffusion.Component.Misc
{
    public class SingleBlockAddComponent : GH_Component
    {
        public SingleBlockAddComponent()
          : base("SingleBlockPointMove", "SBPM",
              "Untilizing Single Block for Point Movement Employing CUDA",
              "EcoLogic", "0.Misc.")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointList", "PL", "Point to Move", GH_ParamAccess.list);
            pManager.AddPointParameter("Point2Add", "PA", "Point to Add", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Result", "R", "Result", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> Point1 = new List<Point3d>();
            List<Point3d> Point2 = new List<Point3d>();

            if (!DA.GetDataList(0, Point1)) { return; }
            if (!DA.GetDataList(1, Point2)) { return; }
            
            ///Exception Alert for size of List
            ///
            if (Point1.Count != Point2.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Size of two vector list is not identical");
            }
            if (Point1.Count > 1024 / 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Size of Vector is not computable in Single Block");
            }
            int len = Point1.Count;
            double[,] result = new double[len, 3];

            BlockVectorAdd(Point2Array(Point1), Point2Array(Point2), len, result);

            DA.SetDataList(0, Array2Point(result, len));
            /*
            Task.Run(() =>
            {
                VectorAdd(Point2Array(Point1), Point2Array(Point2), len, result);
                Rhino.RhinoApp.InvokeOnUiThread(() =>
                {
                    DA.SetDataList(0, Array2Point(result, len));
                    this.ExpireSolution(true);
                });
            });
            */
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("BF6143C8-8395-4B39-8EA3-FCCE32BD3B53"); }
        }
    }
}