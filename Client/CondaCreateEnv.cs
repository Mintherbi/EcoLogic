using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static PointCloudDiffusion.Utils.Utils;
namespace PointCloudDiffusion.Client
{
    public class CondaCreateEnv
    {
         private Process process;

        //Constructor
        public CondaCreateEnv(string ymlPath)
        {
            this.process = new Process();

            var psi = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = $"zsh -c \"source ~/.zshrc && conda deactivate && conda env create --file=\"{ymlPath}\"\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            this.process.StartInfo = psi; 
        }


        public async Task AsyncRun(Action<string> processOutput, Action<string> processError)
        {
            var tcs = new TaskCompletionSource<bool>();

            this.process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    processOutput?.Invoke($"[Output] {e.Data}");
            };

            this.process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    processError?.Invoke($"[Error] {e.Data}");
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
