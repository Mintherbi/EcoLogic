using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PointCloudDiffusion.Client
{
    public class PyWSL
    {
        private Process process;

        //Constructor
        public PyWSL(string scriptPath, string args = null, string conda = "base")
        {
            this.process = new Process();

            var psi = new ProcessStartInfo();
            
            // Determine OS and set appropriate command
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows - use WSL
                psi.FileName = "wsl";
                psi.Arguments = $"source ~/.zshrc && conda activate {conda} && python3 -u {scriptPath} {args}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || 
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // macOS/Linux - use zsh directly
                psi.FileName = "zsh";
                psi.Arguments = $"-c \"source ~/.zshrc && conda activate {conda} && python3 -u {scriptPath} {args}\"";
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

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    processOutput?.Invoke($"[Output] {e.Data}");
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    processError?.Invoke($"[Error] {e.Data}");
            };

            process.Exited += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            process.EnableRaisingEvents = true;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await tcs.Task;
            process.WaitForExit();    
            process.WaitForExit(100);
        }

        public (string Process_Output, string Process_Error) Run()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            var tcs = new TaskCompletionSource<bool>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    output.AppendLine($"[Output] {e.Data}");
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine($"[Error] {e.Data}");
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return (output.ToString(), error.ToString());
        }
    }
}
