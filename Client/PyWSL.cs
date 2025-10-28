using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PointCloudDiffusion.Client
{
    public class PyWSL
    {
        private Process process;

        //Constructor
        public PyWSL(string scriptPath, string args = null, string conda = "base")
        {
            this.process = new Process();

            var psi = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = $"source ~/.zshrc && conda activate {conda} && python3 -u {scriptPath} {args}",
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
