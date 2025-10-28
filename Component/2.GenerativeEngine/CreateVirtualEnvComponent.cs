using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

using PointCloudDiffusion.Client;
using static PointCloudDiffusion.Utils.Utils;

namespace PointCloudDiffusion.Component.Train
{
    public class CreateVirtualEnvComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CreateVirtualEnvComponent()
          : base("CreateVirtualEnvironment", "CondaEnv",
              "Create Conda Virtual Environment",
              "EcoLogic", "2.GenerativeEngine")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "CreateEnv", CreateCondaEnv, "Create Environment by Conda");
        }

        string ymlPath = "";
        List<string> processError;
        List<string> processOutput;
        public void CreateCondaEnv()
        {
            Task.Run(async () =>
            {
                processOutput = new List<string>();
                processError = new List<string> ();

                CondaCreateEnv condaCreateEnv = new CondaCreateEnv(ConvertWindowsPathToLinux(ymlPath));
                await condaCreateEnv.AsyncRun(
                    processOutput: line =>
                    {
                        processOutput.Add(line);
                        
                        Grasshopper.Instances.DocumentEditor.Invoke(new Action(() =>
                        {
                            OnPingDocument().ScheduleSolution(1, doc =>
                            {
                                ExpireSolution(false);
                            });
                        }));
                    },

                    processError: line =>
                    {
                        processError.Add(line);
                        Grasshopper.Instances.DocumentEditor.Invoke(new Action(() =>
                        {
                            OnPingDocument().ScheduleSolution(1, doc =>
                            {
                                ExpireSolution(false);
                            });
                        }));
                    }                        
                );
            });
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CondaEnvPath", "yml", "Path of environment.yml file", GH_ParamAccess.item,
                "/mnt/c/Users/jord9/source/repos/Mintherbi/PointCloudDiffusion/DPM3D/env.yml");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ProcessOutput", "Out", "Process from Conda", GH_ParamAccess.list);
            pManager.AddTextParameter("ProcessError", "Err", "Error from Conda", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref ymlPath)) { return; }

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
                var resourceName = "EcoLogic.IconResource.CreateVirtualEnv_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "CreateVirtualEnv_64.png");
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
            get { return new Guid("EDF1C7C5-47FD-4161-9251-2BB08A580E6A"); }
        }
    }
}