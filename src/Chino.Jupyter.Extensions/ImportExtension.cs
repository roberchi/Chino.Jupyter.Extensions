using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Chino.Jupiter.Extensions.JupyterNotebook;
using System.CommandLine.NamingConventionBinder;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.CSharp;

namespace Chino.Jupiter.Extensions
{
    public class ImportExtension : IKernelExtension
    {
        const string MAGIC_CMD = "#!import";
        static Command importCommand;

        public Task OnLoadAsync(Kernel kernel)
        {
            importCommand = new Command(MAGIC_CMD, "Import a notebook as a module.");
            importCommand.AddArgument(new Argument<string>("notebook"));
//                importCommand.AddOption(new Option<bool>(new[] { "-v", "--verbose" }, "Display imported notebook output"));
                
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
                    var jpynb = Notebook.FromJson(json);

                    // verify notebook is .net notebook
                    //if (jpynb.Metadata.Kernelspec.Name != context.HandlingKernel.Name)
                    //    throw new InvalidOperationException($"Kernel '{jpynb.Metadata.Kernelspec.Name}' not supported");

                    //string directives = string.Join("\n", context.HandlingKernel.Directives.Select(d => d.Name));
                    //try { 
                    if (!context.HandlingKernel.Directives.Contains(importCommand))
                    {
                        context.HandlingKernel.AddDirective(importCommand);
                    }
                    //} catch (Exception ex) { }
                    //context.Display(new HtmlString($@"directive names <b>{directives}</b>"));

                    int i = 0;
                    foreach (var cell in jpynb.Cells.Where(c=> c.CellType == CellType.Code))
                    {
                        var theCode = string.Join("\n", cell.Source);

                        context.Display(new HtmlString(i + theCode.Replace("\n", "<br>")));
                        
                        var result = await context.HandlingKernel.SubmitCodeAsync(string.Join("\n",cell.Source));

                        context.Display(new HtmlString("done " + i));
                        i++;
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
