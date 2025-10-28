using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel.Parameters;
using System.IO;
using static PointCloudDiffusion.Utils.Utils;
using Grasshopper.Kernel.Types;

namespace PointCloudDiffusion.Component.Train
{
    public class ModelOptionComponent : GH_Component
    {
        public string SelectedFilePath = "";
        private List<PythonArg> parsedArgs = new();

        public ModelOptionComponent()
          : base("ModelOption", "Arg",
              "Construct Model Parser for Trainging \n" +
                "Double Click Component to Set which model to run",
              "BinaryNature", "Train")
        {
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
        /// Reloads the input parameters for the component from the specified file.
        /// </summary>
        /// <remarks>This method clears the existing input parameters and replaces them with new
        /// parameters parsed from the specified file. After reloading, the component's UI is updated, and the solution
        /// is expired to trigger a re-execution of the component.</remarks>
        /// <param name="filePath">The path to the file containing the parameters to load. Must be a valid file path.</param>

        public void ReloadParametersFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File does not exist.");
                return;
            }

            SelectedFilePath = filePath;
            parsedArgs = ParseArguments(filePath);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Parsed {parsedArgs.Count} arguments from file.");

            this.Params.Input.Clear();

            foreach (var arg in parsedArgs)
            {
                IGH_Param param = CreateParamFromArg(arg);
                if (param != null)
                {
                    this.Params.RegisterInputParam(param);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Registered input param: {arg.Name} ({arg.Type})");
                }
            }

            this.Params.OnParametersChanged();
            this.ExpireSolution(true);
        }
        /// <summary>
        /// Creates a Grasshopper parameter based on the specified Python argument.
        /// </summary>
        /// <remarks>The parameter's name and nickname are set to the argument's name, and its description
        /// includes the default value. If the argument specifies a default value, it is converted and added as volatile
        /// data to the parameter.</remarks>
        /// <param name="arg">The Python argument containing the type, name, and default value used to configure the parameter.</param>
        /// <returns>An <see cref="IGH_Param"/> instance configured according to the type and properties of the provided
        /// <paramref name="arg"/>. The parameter's type is determined by the argument's type (e.g., "int" maps to <see
        /// cref="Param_Integer"/>).</returns>
        /// 

/*
        private IGH_Param CreateParamFromArg(PythonArg arg)
        {
            IGH_Param param;
            switch (arg.Type)
            {
                case "int":
                    param = new Param_Integer { Access = GH_ParamAccess.item };
                    break;
                case "float":
                case "double":
                    param = new Param_Number { Access = GH_ParamAccess.item };
                    break;
                case "bool":
                case "eval":
                    param = new Param_Boolean { Access = GH_ParamAccess.item };
                    break;
                case "str":
                    param = new Param_String { Access = GH_ParamAccess.item };
                    break;
                case "str_list":
                    param = new Param_String { Access = GH_ParamAccess.list };
                    break;
                default:
                    param = new Param_String();
                    break;
            }

            param.Name = arg.Name;
            param.NickName = arg.Name;
            param.Description = $"Default: {arg.Default}";

            if (arg.Default != null)
            {
                var val = ConvertDefault(arg.Type, arg.Default);
                param.AddVolatileData(new GH_Path(0), 0, val);
            }

            return param;
        }
*/
        private IGH_Param CreateParamFromArg(PythonArg arg)
        {
            IGH_Param param;
            switch (arg.Type)
            {
                case "int":
                    param = new Param_Integer { Access = GH_ParamAccess.item };
                    break;
                case "float":
                case "double":
                    param = new Param_Number { Access = GH_ParamAccess.item };
                    break;
                case "bool":
                    param = new Param_Boolean { Access = GH_ParamAccess.item };
                    break;
                case "str":
                    param = new Param_String { Access = GH_ParamAccess.item };
                    break;
                case "str_list":
                    param = new Param_String { Access = GH_ParamAccess.list };
                    break;
                default:
                    param = new Param_String();
                    break;
            }

            param.Name = arg.Name;
            param.NickName = arg.Name;
            param.Description = $"Default: {arg.Default}";

            if (arg.Default != null)
            {
                var val = ConvertDefault(arg.Type, arg.Default);
                var path = new GH_Path(0);

                switch (param)
                {
                    case Param_Integer p when val is int i:
                        p.AddVolatileData(path, 0, new GH_Integer(i));
                        break;
                    case Param_Number p when val is double d:
                        p.AddVolatileData(path, 0, new GH_Number(d));
                        break;
                    case Param_Boolean p when val is bool b:
                        p.AddVolatileData(path, 0, new GH_Boolean(b));
                        break;
                    case Param_String p:
                        p.AddVolatileData(path, 0, new GH_String(val.ToString()));
                        break;
                }
            }

            return param;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("SelectedFilePath", "SelFile", "Selected File Path", GH_ParamAccess.item);
            pManager.AddTextParameter("Arguments", "Arg", "Arguments for CommandLine", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            for (int i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i];
                object val = null;

                switch (arg.Type.ToLower())
                {
                    case "int":
                        int iVal = 0;
                        if (DA.GetData(i, ref iVal)) val = iVal;
                        break;
                    case "float":
                    case "double":
                        double dVal = 0;
                        if (DA.GetData(i, ref dVal)) val = dVal;
                        break;
                    case "bool":
                        bool bVal = false;
                        if (DA.GetData(i, ref bVal)) val = bVal;
                        break;
                    case "str_list":
                        List<string> listVal = new();
                        if (DA.GetDataList(i, listVal)) val = listVal;
                        break;
                    default:
                        string sVal = null;
                        if (DA.GetData(i, ref sVal)) val = sVal;
                        break;
                }

                if (val != null)
                    arg.Value = val;
            }

            DA.SetData(0, ConvertWindowsPathToLinux(SelectedFilePath));
            DA.SetData(1, ToCommandLineArguments(parsedArgs));
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
            get { return new Guid("39DFDB9D-4210-489B-80B6-7B53A0D08B70"); }
        }


        public class DoubleClickComponentAttributes : GH_ComponentAttributes
        {
            public DoubleClickComponentAttributes(IGH_Component component) : base(component) { }
            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*";
                dialog.Title = "Select a Python script";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Owner is ModelOptionComponent comp)
                    {
                        comp.ReloadParametersFromFile(dialog.FileName);
                    }
                }

                return GH_ObjectResponse.Handled;
            }
        }
    }
}
