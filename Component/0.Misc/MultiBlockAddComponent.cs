using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using static PointCloudDiffusion.Utils.Utils;

namespace PointCloudDiffusion.Component.Misc
{
    public class MultiBlockAddComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MultiBlockAddComponent()
          : base("MultiBlockParalellMove", "MBPM",
              "Utilizing Multiple Block for Point Movement Employing CUDA",
              "EcoLogic", "Misc.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointList", "PL", "Point to Move", GH_ParamAccess.list);
            pManager.AddPointParameter("Point2Add", "PA", "Point to Move", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Result", "R", "Result", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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

            int len = Point1.Count;
            double[,] result = new double[len, 3];

            BlockVectorAdd(Point2Array(Point1), Point2Array(Point2), len, result);

            DA.SetDataList(0, Array2Point(result, len));

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("C:\\Users\\jord9\\source\\repos\\Mintherbi\\PointCloudDiffusion\\PointCloudDiffusion\\src\\ReverseDiffusion.png");
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EFA5A5A7-5A63-4E81-992E-3F0B2A84EE0A"); }
        }
    }
}