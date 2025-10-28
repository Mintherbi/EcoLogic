using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.EvaluatePerformance
{
    public class StructuralStrength : GH_Component
    {
        public StructuralStrength()
          : base("StructuralStrength", "SS",
              "Evaluate structural strength of generated elements",
              "EcoLogic", "4.EvaluatePerformance")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Structure", "S", "Input structural geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("Load", "L", "Applied load", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaterialStrength", "MS", "Material strength", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Utilization", "U", "Structural utilization ratio", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.StructuralStrength_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "StructuralStrength_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("F9D7AEBE-0A6D-4F7D-9D0B-AEFC7B8DDBD0");
    }
}
