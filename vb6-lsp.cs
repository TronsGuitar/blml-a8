using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;
using System.IO;

namespace VB6LanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var input = Console.OpenStandardInput();
            var output = Console.OpenStandardOutput();

            var rpc = JsonRpc.Attach(input, output, new LanguageServer());

            await rpc.Completion;
        }
    }

    // Custom Language Server implementation
    public class LanguageServer : IDisposable
    {
        public LanguageServer()
        {
            // Initialize any required fields here
        }

        public void Initialize(object[] parameters)
        {
            // Initialization logic, such as capability registration
        }

        public Task<InitializeResult> HandleInitializeRequestAsync(InitializeParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new InitializeResult
            {
                Capabilities = new ServerCapabilities
                {
                    TextDocumentSync = new TextDocumentSyncOptions
                    {
                        Change = TextDocumentSyncKind.Full
                    },
                    HoverProvider = true,
                    // Add more capabilities as needed
                }
            });
        }

        public Task<Hover> HandleHoverRequestAsync(TextDocumentPositionParams request, CancellationToken cancellationToken)
        {
            // Provide hover information here (e.g., VB6 function descriptions)
            return Task.FromResult(new Hover
            {
                Contents = new MarkupContent
                {
                    Kind = MarkupKind.PlainText,
                    Value = "This is a VB6 function."
                }
            });
        }

        public Task<Unit> HandleTextDocumentChangeAsync(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            // Handle text document changes here
            return Unit.Task;
        }

        public void Dispose()
        {
            // Cleanup logic if needed
        }
    }
}
