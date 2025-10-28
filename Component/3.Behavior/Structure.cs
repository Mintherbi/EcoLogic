using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Behavior
{
    public class Structure : GH_Component
    {
        public Structure()
          : base("Structure", "Struct",
              "Apply structural constraints to biomimetic form",
              "EcoLogic", "3.Behavior")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("SpanLimit", "SL", "Maximum unsupported span", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialStrength", "MS", "Material allowable strength", GH_ParamAccess.item);
            pManager.AddNumberParameter("SafetyFactor", "SF", "Safety factor", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("StructureGeometry", "SG", "Generated structural elements", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Structure_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Structure_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("9FC3D5EB-7A8B-42D2-AEDB-BE40C4AF21F9");
    }
}
