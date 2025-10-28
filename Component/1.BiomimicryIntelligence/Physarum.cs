using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Biomimicry
{
	public class Physarum : GH_Component
	{
		public Physarum()
		  : base("Physarum", "Phys",
			  "Biomimicry: Physarum (slime mold) growth parameters",
			  "EcoLogic", "1.Biomimicry")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("DiffusionRate", "DR", "Diffusion rate", GH_ParamAccess.item);
			pManager.AddNumberParameter("Attraction", "A", "Attraction to nutrient sources", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Iterations", "It", "Number of iterations", GH_ParamAccess.item);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("Network", "N", "Generated network curves", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
		}

		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				var asm = System.Reflection.Assembly.GetExecutingAssembly();
				var resourceName = "EcoLogic.IconResource.Physarum_64.png";
				var stream = asm.GetManifestResourceStream(resourceName);
				if (stream != null)
					return new System.Drawing.Bitmap(stream);

				var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
				var file = System.IO.Path.Combine(folder, "Physarum_64.png");
				if (System.IO.File.Exists(file))
					return new System.Drawing.Bitmap(file);

				return null;
			}
		}

		public override Guid ComponentGuid => new Guid("B0BA3523-3AA7-43F9-B3AA-5CDA3C9138C8");
	}
}
