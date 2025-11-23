using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

            var psi = new ProcessStartInfo();
            
            // Determine OS and set appropriate command
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows - use WSL
                psi.FileName = "wsl";
                psi.Arguments = $"zsh -c \"source ~/.zshrc && conda deactivate && conda env create --file=\"{ymlPath}\"\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || 
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // macOS/Linux - use zsh directly
                psi.FileName = "zsh";
                psi.Arguments = $"-c \"source ~/.zshrc && conda deactivate && conda env create --file=\"{ymlPath}\"\"";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system");
            }

            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = false;

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
