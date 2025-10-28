using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Visualization
{
    public class ReverseDiffusionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ReverseDiffusionComponent()
          : base("ReverseDiffusion", "RD",
            "Description",
            "BinaryNature", "Visualization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("OriginalModel", "OM", "Original Model", GH_ParamAccess.list);
            pManager.AddPointParameter("NoiseCloud", "NC", "Noised Point Cloud", GH_ParamAccess.list);
            pManager.AddIntegerParameter("MaxStep", "MxS", "MaxiumStep", GH_ParamAccess.item);
            pManager.AddBooleanParameter("reset", "r", "reset", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("ReverseDiffusion", "RD", "ReverseDiffusion", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        /// 
        int tick;
        List<Point3d> NoiseModel;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> OriginalModel = new List<Point3d>();
            List<Point3d> PointCloud = new List<Point3d>();
            int MaxStep = new int();
            bool reset = new bool();

            if (!DA.GetDataList(0, OriginalModel)) { return; }
            if (!DA.GetDataList(1, PointCloud)) { return; }
            if (!DA.GetData(2, ref MaxStep)) { return; }
            if (!DA.GetData(3, ref reset)) { return; }

            if(NoiseModel == null || reset == true)
            {
                tick = 0;
                NoiseModel = new List<Point3d>(PointCloud);   
            }

            for (int i = 0; i < NoiseModel.Count; i++)
            {
                NoiseModel[i] = ReverseStep(NoiseModel[i], OriginalModel[i], (double)tick/MaxStep);
            }

            tick++;

            DA.SetDataList(0, NoiseModel);
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
            get { return new Guid("AC8BC418-6A66-47C1-8A40-2BED85F63497"); }
        }

        private Point3d ReverseStep(Point3d noisyPt, Point3d originalPt, double t)
        {
            return new Point3d(
                noisyPt.X * (1 - t) + originalPt.X * t,
                noisyPt.Y * (1 - t) + originalPt.Y * t,
                noisyPt.Z * (1 - t) + originalPt.Z * t
            );
        }
    }
}