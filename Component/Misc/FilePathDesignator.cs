using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using PointCloudDiffusion.Component.Train;
using Rhino.Geometry;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Grasshopper.Kernel.Parameters;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using PointCloudDiffusion.Utils;
using static PointCloudDiffusion.Utils.Utils;

namespace PointCloudDiffusion.Component.Misc
{
    public class FilePathDesignator : GH_Component
    {
        public string SelectedFilePath = "";
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FilePathDesignator()
          : base("FilePathDesignator", "FP",
              "Designate File Path by double click!",
              "EcoLogic", "Misc.")
        {
        }

        public void AllocateFilePath(string filePath)
        {
            if (!File.Exists(filePath)) return;

            SelectedFilePath = ConvertWindowsPathToLinux(filePath);

            this.Params.OnParametersChanged(); 
            this.ExpireSolution(true);        
        }

        public override void CreateAttributes()
        {
            m_attributes = new DoubleClickComponentAttributes(this);
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Designated Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, SelectedFilePath);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1FB8B200-90A1-42DB-B6B9-F4BEB8886C11"); }
        }

        public class DoubleClickComponentAttributes : GH_ComponentAttributes
        {
            public DoubleClickComponentAttributes(IGH_Component component) : base(component) { }
            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "All Files (*.*)|*.*";
                dialog.Title = "Select a Python script";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Owner is FilePathDesignator comp)
                    {
                        comp.AllocateFilePath(dialog.FileName);
                    }
                }

                return GH_ObjectResponse.Handled;
            }
        }
    }
}