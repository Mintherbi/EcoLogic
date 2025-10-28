using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Net.Http;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using NumSharp;
using Grasshopper.Kernel.Geometry;

namespace PointCloudDiffusion.Client
{
    public static class PATH
    {
        public static string pythonPath => @"C:\Program Files\WindowsApps\PythonSoftwareFoundation.Python.3.11_3.11.2544.0_x64__qbz5n2kfra8p0\python3.11.exe";
        public static string powershellPath => @"C:\Program Files\PowerShell\7\pwsh.exe";
        public static string projectRoot => @"..\..\";
        public static string DPM3D_Train_AE => Path.Combine(projectRoot, "DPM3D", "train_ae.py");


        public static string DPM3D_Train_Gen => Path.Combine(projectRoot, "DPM3D", "train_gen.py");
        //public static string HelloWorld => Path.Combine(modelPath,"Hello", "Hello.py");

        public static string HelloWorld => @"C\Users\jord9\source\repos\Mintherbi\PointCloudDiffusion\Hello\Hello.py";
        public static string WSL_MNT => "/mnt/";
        public static string WSL_HelloWorld => WSL_MNT + "c/Users/jord9/source/repos/Mintherbi/PoinCloudDiffusion/Hello/Hello.py";

    }
}
