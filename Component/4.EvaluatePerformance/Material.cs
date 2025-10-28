using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PointCloudDiffusion.Component.EvaluatePerformance
{
	public class Material : GH_Component
	{
		public Material()
		  : base("Material", "Mat",
			  "Evaluate material properties for generated form",
			  "EcoLogic", "4.EvaluatePerformance")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("MaterialType", "MT", "Material type identifier", GH_ParamAccess.item);
			pManager.AddNumberParameter("Density", "D", "Material density", GH_ParamAccess.item);
			pManager.AddNumberParameter("CostPerUnit", "C", "Material cost per unit", GH_ParamAccess.item);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddNumberParameter("MaterialScore", "MS", "Aggregate material score", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA) { }

		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				var asm = System.Reflection.Assembly.GetExecutingAssembly();
				var resourceName = "EcoLogic.IconResource.Material_64.png";
				var stream = asm.GetManifestResourceStream(resourceName);
				if (stream != null)
					return new System.Drawing.Bitmap(stream);

				var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
				var file = System.IO.Path.Combine(folder, "Material_64.png");
				if (System.IO.File.Exists(file))
					return new System.Drawing.Bitmap(file);

				return null;
			}
		}

		public override Guid ComponentGuid => new Guid("EFA5A5A7-5A63-4E81-992E-3F0B2A84EE0A");
	}
}
