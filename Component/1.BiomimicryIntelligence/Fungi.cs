using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Biomimicry
{
    public class Fungi : GH_Component
    {
        public Fungi()
          : base("Fungi", "Fungi",
              "Biomimicry: Fungi growth parameters",
              "EcoLogic", "1.Biomimicry")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("MyceliumRate", "MR", "Mycelium spread rate", GH_ParamAccess.item);
            pManager.AddNumberParameter("Humidity", "H", "Humidity factor", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "It", "Iterations", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Hyphae", "Hy", "Generated hyphae curves", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Fungi_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Fungi_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("F0596B0D-38EC-45B3-BADB-F8F11A1DE1A1");
    }
}
