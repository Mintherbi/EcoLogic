using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.EvaluatePerformance
{
    public class FAR : GH_Component
    {
        public FAR()
          : base("FAR", "FAR",
              "Evaluate Floor Area Ratio for the form",
              "EcoLogic", "4.EvaluatePerformance")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Footprint", "FP", "Building footprint geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("SiteArea", "SA", "Total site area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Levels", "Lv", "Number of levels", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("FARValue", "FAR", "Calculated FAR", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.FAR_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "FAR_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("1F97240E-5E0C-4D7B-AB79-6F82F4F293C4");
    }
}
