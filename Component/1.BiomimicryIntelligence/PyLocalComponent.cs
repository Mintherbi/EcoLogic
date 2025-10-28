using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using System.IO;

using PointCloudDiffusion.Client;

namespace PointCloudDiffusion.Component.ExternalProcess
{
    public class PyLocalComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PyLocalComponent()
          : base("PythonInLocal", "PyLo",
              "Python in Local Environment",
              "EcoLogic", "Execution")
        {
        }

        string PythonPath;
        string ScriptPath;
        string args;

        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "RUN!", RunPython, "RunPythonScript");
        }

        public void RunPython()
        {
            PyLocal pylocal = new PyLocal(PATH.HelloWorld);
            pylocal.Run();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            /*
            pManager.AddTextParameter("PythonPath", "PP", "Path of Python", GH_ParamAccess.item, PATH.pythonPath);
            pManager.AddTextParameter("ScriptPath", "SP", "Path of Script", GH_ParamAccess.item, PATH.HelloWorld);
            pManager.AddTextParameter("ArgumentPath", "AP", "Path of Argument", GH_ParamAccess.item, "");
            */
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Process", "P", "Process of Program", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
            if(!DA.GetData(0, ref PythonPath)) { return; }
            if(!DA.GetData(1, ref ScriptPath)) { return; }
            if(!DA.GetData(2, ref args)) { return; }
            */

            DA.SetData(0, null);
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
            get { return new Guid("59980291-EB78-470C-8B5D-110ECC6D4A8F"); }
        }
    }
}