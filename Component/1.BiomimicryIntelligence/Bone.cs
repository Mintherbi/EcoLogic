using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.Biomimicry
{
	public class Bone : GH_Component
	{
		public Bone()
		  : base("Bone", "Bone",
			  "Biomimicry: Bone growth parameters",
			  "EcoLogic", "1.Biomimicry")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			// Parameters that influence how the 'bone' grows
			pManager.AddNumberParameter("GrowthRate", "GR", "Growth rate (0..1)", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Iterations", "It", "Number of growth iterations", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Seed", "S", "Random seed", GH_ParamAccess.item);
			pManager.AddNumberParameter("EnvironmentFactor", "EF", "Environmental factor affecting growth", GH_ParamAccess.item);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			// Single output: generated geometry points (or any representation)
			pManager.AddPointParameter("Skeleton", "Sk", "Generated skeleton points", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			// Intentionally empty: user requested only IO skeleton (no main implementation)
		}

		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				var asm = System.Reflection.Assembly.GetExecutingAssembly();
				var resourceName = "EcoLogic.IconResource.Bone_64.png";
				var stream = asm.GetManifestResourceStream(resourceName);
				if (stream != null)
					return new System.Drawing.Bitmap(stream);

				// fallback: load from output folder if PNG exists
				var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
				var file = System.IO.Path.Combine(folder, "Bone_64.png");
				if (System.IO.File.Exists(file))
					return new System.Drawing.Bitmap(file);

				return null;
			}
		}

		public override Guid ComponentGuid => new Guid("C2E91F1D-E397-498D-9809-D602F4F9108C");
	}
}
