using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VB6LanguageServerClient
{
    public class VB6LanguageClient : ILanguageClient
    {
        public string Name => "VB6 Language Server";

        public object InitializationOptions => null;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            // Launch the language server as an external process
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "path/to/your/language-server.exe",
                Arguments = "",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = System.Diagnostics.Process.Start(processStartInfo);

            return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
        }

        public async Task OnLoadedAsync() { /* Initialization logic, if necessary */ }

        public Task OnServerInitializedAsync() => Task.CompletedTask;

        public Task OnServerInitializeFailedAsync(Exception e) => Task.CompletedTask;
    }
}
