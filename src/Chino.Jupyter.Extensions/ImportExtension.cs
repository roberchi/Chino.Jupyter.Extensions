using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Documents;
using Microsoft.DotNet.Interactive.Documents.Jupyter;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chino.Jupiter.Extensions
{
    public class ImportExtension : IKernelExtension
    {
        const string MAGIC_CMD = "#!import";
        static Command importCommand;

        static string[] SUPPORTED_LANGUAGES = { "csharp","fsharp","javascript", "pwsh" };

        public Task OnLoadAsync(Kernel kernel)
        {
            importCommand = new Command(MAGIC_CMD, "Import a notebook as a module.");
            importCommand.AddArgument(new Argument<string>("notebook"));
            //importCommand.AddOption(new Option<bool>(new[] { "-v", "--verbose" }, "Display imported notebook output"));

            importCommand.Handler = CommandHandler.Create(
                async (string notebook, bool? ver, KernelInvocationContext context) =>
                {
                    if (string.IsNullOrWhiteSpace(notebook))
                    {
                        context.Display(new HtmlString($@"<b>Missing Notebook path argument</b>"));
                        return;
                    }

                    var notebookPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                        Path.GetDirectoryName(notebook),
                        Path.GetFileNameWithoutExtension(notebook) + ".ipynb");

                    context.Display(new HtmlString($@"Loading notebook <b>{notebook}</b> [{notebookPath}] ..."));

                    // load notebook cells
                    var json = await File.ReadAllTextAsync(notebookPath);
                    var jpynb = Notebook.Parse(json);

                    // verify notebook is .net notebook
                    //string kernelspecName = ((dynamic)jpynb.Metadata["kernelspec"])["name"];
                    //if (kernelspecName != context.HandlingKernel.Name)
                    //    throw new InvalidOperationException($"Kernel '{kernelspecName}' not supported");

                    //this is to support nested notebook-as-module loading
                    if (!context.HandlingKernel.Directives.Contains(importCommand))
                    {
                        context.HandlingKernel.AddDirective(importCommand);
                    }

                    int i = 0;
                    foreach (InteractiveDocumentElement cell in jpynb)
                    {
                        if (SUPPORTED_LANGUAGES.Contains(cell.Language))
                        {
                            //context.Display(new HtmlString(i + " " + cell.Contents.Replace(Environment.NewLine,"<br/>")));

                            var result = await context.HandlingKernel.SubmitCodeAsync(cell.Contents);

                            //context.Display(new HtmlString("done " + i));
                            i++;
                        }
                    }

                    context.Display(new HtmlString($@"notebook <b>{notebook}</b> loaded"));
                });

            kernel.AddDirective(importCommand);

            if (KernelInvocationContext.Current is { } context)
            {
                $"`{nameof(ImportExtension)}` is loaded. It adds the import notebook as module. Try it by running: `{MAGIC_CMD} path\\notebook name.ipynb`".DisplayAs("text/markdown");
            }

            return Task.CompletedTask;
        }

        private static string GetValidNamespace(string ns)
        {
            return string.Join(".", ns.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(chunk => chunk)); // TODO: replace invalid char in chunk 
        }
    }
}
