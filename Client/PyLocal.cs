using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;


namespace PointCloudDiffusion.Client
{
    public class PyLocal
    {
        Process process;
       /*
        public PyLocal(string scriptPath, string args)
        : this(PATH.pythonPath, scriptPath, args) { }
            */

        public PyLocal(string scriptPath, string args=null)
        {
            //string inlineCode = $"exec(open(r\"{scriptPath}\").read())";

            this.process = new Process();
            
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoExit -Command \"python \\\"{scriptPath}\\\" {args}\"",
                UseShellExecute = true,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                CreateNoWindow = false
            };

            this.process.StartInfo = psi;
        }

        public void Run()
        {
            this.process.Start();
        }
    }
}
