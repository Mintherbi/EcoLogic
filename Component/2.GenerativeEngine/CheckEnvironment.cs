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

namespace PointCloudDiffusion.Component.Train
{
    public class CheckEnvironment : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CheckEnvironment()
          : base("CheckEnvironment", "Env",
              "Check Conda Environment that is Installed",
              "EcoLogic", "2.GenerativeEngine")
        {
        }

        List<string> EnvList;

        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "CheckEnv", AsyncRunWSL, "Check Conda Environment");
        }
        public void AsyncRunWSL()
        {
            Task.Run(async () =>
            {
                CondaCheckEnv CheckEnv = new CondaCheckEnv();

                EnvList = new List<string>();

                await CheckEnv.AsyncRun(
                    EnvList : line =>
                    {
                        EnvList.Add(line);
                        Grasshopper.Instances.DocumentEditor.Invoke(new Action(() =>
                        {
                            this.OnPingDocument().ScheduleSolution(1, doc =>
                            {
                                this.ExpireSolution(false); // false: 중간 갱신
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
            pManager.AddTextParameter("EnvironmentList", "EnvList", "List of Conda Environment", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> environments = new List<string>();
         
            for (int i = 2; i < EnvList.Count; i++)
            {
                string line = EnvList[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 공백 기준으로 첫 단어 추출
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    environments.Add(parts[0]);
                }
            }

            DA.SetDataList(0, environments);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "EcoLogic.IconResource.CheckEnvironment_64.png";
                var stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new System.Drawing.Bitmap(stream);

                var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "IconResource");
                var file = System.IO.Path.Combine(folder, "CheckEnvironment_64.png");
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
            get { return new Guid("9BF3EECA-6CAC-4DA0-8091-BDE2E5AFABC1"); }
        }
    }
}