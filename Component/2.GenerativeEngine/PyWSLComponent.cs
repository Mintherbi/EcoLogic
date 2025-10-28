using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

using PointCloudDiffusion.Client;
using static PointCloudDiffusion.Utils.Utils;

namespace PointCloudDiffusion.Component.ExternalProcess
{
    public class PyWSLComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PyWSLComponent()
          : base("PythonInWsl", "PyWSL",
              "Execute Python in WSl Environment",
              "EcoLogic", "2.GenerativeEngine")
        {
        }

        string scriptPath;
        string scriptArgs;
        string condaEnv;

        List<string> processOutput;
        List<string> processError;

        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "RUN!", AsyncRunWSL, "RunPythonScript");
        }
        public void AsyncRunWSL()
        {
            Task.Run(async () =>
            {
                PyWSL pyWSL = new PyWSL(scriptPath, scriptArgs, condaEnv);

                processOutput = new List<string>();
                processError = new List<string>();

                await pyWSL.AsyncRun(
                    processOutput: line =>
                    {
                        processOutput.Add(line);
                        Grasshopper.Instances.DocumentEditor.Invoke(new Action(() =>
                        {
                            this.OnPingDocument().ScheduleSolution(1, doc =>
                            {
                                this.ExpireSolution(false); // false: 중간 갱신
                            });
                        }));
                    },
                    processError: line =>
                    {
                        processError.Add(line);
                        Grasshopper.Instances.DocumentEditor.Invoke(new Action(() =>
                        {
                            this.OnPingDocument().ScheduleSolution(1, doc =>
                            {
                                this.ExpireSolution(false);
                            });
                        }));
                    }
                );

                Rhino.RhinoApp.InvokeOnUiThread(() =>
                {
                    this.ExpireSolution(true); 
                });
            });
        }

        public void RunWSL()
        {
            string Output;
            string Error;

            PyWSL pyWSL = new PyWSL(scriptPath, scriptArgs);

            (Output, Error) = pyWSL.Run();

            processOutput.Add(Output);
            processError.Add(Error);

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FilePath", "FP", "", GH_ParamAccess.item, "/mnt/c/Users/jord9/source/repos/Mintherbi/PointCloudDiffusion/Hello/Hello.py");
            pManager.AddTextParameter("Arguments", "Args", "", GH_ParamAccess.item);
            pManager.AddTextParameter("CondaEnv", "Env", "", GH_ParamAccess.item, "dpm-pc-gen");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ProcessOutput", "Out", "Process Output", GH_ParamAccess.item);
            pManager.AddTextParameter("ProcessError", "Err", "Process Error", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref scriptPath)) { return; }
            if (!DA.GetData(1, ref scriptArgs)) { return; }
            if (!DA.GetData(2, ref condaEnv)) { return; }

            DA.SetDataList(0, processOutput);
            DA.SetDataList(1, processError);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.PyWSL_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "PyWSL_64.png");
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
            get { return new Guid("D5015E93-5759-48EC-B34D-6E1626146061"); }
        }
    }
}