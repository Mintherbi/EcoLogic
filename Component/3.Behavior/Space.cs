using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Behavior
{
    public class Space : GH_Component
    {
        public Space()
          : base("Space", "Space",
              "Define usable space properties for biomimetic forms",
              "EcoLogic", "3.Behavior")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("ClearHeight", "CH", "Clear height requirement", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinArea", "MA", "Minimum usable area", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Occupancy", "O", "Estimated occupancy/load", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("SpaceGeometry", "SG", "Generated usable space geometry", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Space_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Space_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("1569FA4C-9D52-4051-8B95-22E301CCF989");
    }
}
