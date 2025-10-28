using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.EvaluatePerformance
{
    public class Solar : GH_Component
    {
        public Solar()
          : base("Solar", "Solar",
              "Evaluate solar performance for a given form",
              "EcoLogic", "4.EvaluatePerformance")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Form", "F", "Input form geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("Latitude", "Lat", "Site latitude", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitude", "Lon", "Site longitude", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time", "T", "Time (hour)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Irradiance", "I", "Estimated irradiance", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Solar_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Solar_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("48702109-3CBB-4C03-848D-50A0F8A5C483");
    }
}
