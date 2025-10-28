using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static PointCloudDiffusion.Utils.Utils;
namespace PointCloudDiffusion.Client
{
    public class CondaCheckEnv
    {
         private Process process;

        //Constructor
        public CondaCheckEnv()
        {
            this.process = new Process();

            var psi = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = $"zsh -c \"source ~/.zshrc && conda env list\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            this.process.StartInfo = psi; 
        }


        public async Task AsyncRun(Action<string> EnvList)
        {
            var tcs = new TaskCompletionSource<bool>();

            this.process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    EnvList?.Invoke($"{e.Data}");
            };

            this.process.Exited += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            this.process.EnableRaisingEvents = true;

            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();

            await tcs.Task;
            this.process.WaitForExit();    
            this.process.WaitForExit(100);
        }
    }
}
