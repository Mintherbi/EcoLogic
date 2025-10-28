using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Biomimicry
{
    public class Embryo : GH_Component
    {
        public Embryo()
          : base("Embryo", "Emb",
              "Biomimicry: Embryo growth parameters",
              "EcoLogic", "1.Biomimicry")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("GrowthFactor", "GF", "Primary growth factor", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Stages", "St", "Number of developmental stages", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Form", "F", "Generated form geometry", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
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

        public override Guid ComponentGuid => new Guid("19C11D4E-26EC-4A4D-AA67-CDA0B6260719");
    }
}
