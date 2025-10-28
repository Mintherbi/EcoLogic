using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Behavior
{
    public class Floor : GH_Component
    {
        public Floor()
          : base("Floor", "Floor",
              "Apply architectural floor constraints to biomimetic form",
              "EcoLogic", "3.Behavior")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("FloorHeight", "FH", "Target floor height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "Height tolerance", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Flatten", "F", "Flatten floor plan (true/false)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Th", "Floor thickness", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("FloorGeometry", "FG", "Generated floor geometry", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.Floor_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "Floor_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        public override Guid ComponentGuid => new Guid("7CA81E08-54C4-4AD7-AB97-CC00EDADD64E");
    }
}
