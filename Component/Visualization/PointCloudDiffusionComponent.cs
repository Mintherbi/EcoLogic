using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Visualization
{
    public class PointCloudDiffusionComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PointCloudDiffusionComponent()
          : base("PointCloudDiffusion", "PD",
            "Description",
            "BinaryNature", "Visualization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointCloud", "PC", "Model Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("NoiseStrength", "NS", "NoiseStrength, Insert double 0 to 1", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MaxStep", "MxS", "MaxiumStep to perform", GH_ParamAccess.item);
            pManager.AddBooleanParameter("reset", "r", "reset", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("NoisedPointCloud", "NP", "PointCloud with noise", GH_ParamAccess.list);
        }

        int tick;
        List<Point3d> OriginalModel;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> PointCloud = new List<Point3d>();
            double NoiseStrength = new double();
            int MaxStep = new int();
            bool reset = new bool();

            if (!DA.GetDataList(0, PointCloud)) { return; }
            if (!DA.GetData(1, ref NoiseStrength)) { return; }
            if (!DA.GetData(2, ref MaxStep)) { return; }
            if (!DA.GetData(3, ref reset)) { return; }

            if (OriginalModel == null || reset == true)
            {
                tick = 0;
                OriginalModel = new List<Point3d>(PointCloud);
            }

            for (int i = 0; i < OriginalModel.Count; i++)
            {
                OriginalModel[i] = AddGaussianNoise(OriginalModel[i], (double)tick/MaxStep, NoiseStrength);
            }

            tick++;

            DA.SetDataList(0, OriginalModel);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4b3f0afe-1c16-49ea-8165-461af1809e8e");

        private Point3d AddGaussianNoise(Point3d pt, double t, double noiseStrength)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode()); 
            double stdDev = noiseStrength * Math.Sqrt(t); 

            double dx = stdDev * NextGaussian(rand);
            double dy = stdDev * NextGaussian(rand);
            double dz = stdDev * NextGaussian(rand);

            return new Point3d(pt.X + dx, pt.Y + dy, pt.Z + dz);
        }

        private double NextGaussian(Random r)
        {
            double u1 = 1.0 - r.NextDouble(); // avoid 0
            double u2 = 1.0 - r.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
    }
}