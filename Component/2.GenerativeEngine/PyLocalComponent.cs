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
              "EcoLogic", "3.ExternalProgram")
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
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.PyLocal_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "PyLocal_64.png");
                if (System.IO.File.Exists(file))
                    return new System.Drawing.Bitmap(file);

                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0AF09C9E-7588-44B7-A3A9-8B035A6B9657"); }
        }
    }
}