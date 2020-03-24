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

namespace Chino.Jupiter.Extensions
{
    public class ImportExtension : IKernelExtension
    {
        public async Task OnLoadAsync(IKernel kernel)
        {
            if (kernel is KernelBase kernelBase)
            {
                var importCommand = new Command("#!import", "Import a notebook as a module.");
                importCommand.AddArgument(new Argument<string>("notebook"));
                importCommand.AddOption(new Option<string>(new[] { "-ns", "--namespace" }, "Namespace to wrap imported notebook, default is notebook name"));
                importCommand.AddOption(new Option<bool>(new[] { "-v", "--verbose" }, "Display imported notebook output"));
                
                importCommand.Handler = CommandHandler.Create(
                    async (string notebook, string ns, bool? ver, KernelInvocationContext context) =>
                    {
                        if (string.IsNullOrWhiteSpace(notebook))
                        {
                            await context.DisplayAsync(new HtmlString($@"<b>Missing Notebook path argument</b>"));
                            return;
                        }

                        ns = GetValidNamespace(string.IsNullOrEmpty(ns)?Path.GetFileNameWithoutExtension(notebook):ns);
                        
                        var notebookPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                            Path.GetDirectoryName(notebook),
                            Path.GetFileNameWithoutExtension(notebook) + ".ipynb");

                        await context.DisplayAsync(new HtmlString($@"<b>Loading notebook {notebookPath} ...</b>"));

                        // load notebook cells
                        var json = await File.ReadAllTextAsync(notebookPath);
                        var jpynb = Notebook.FromJson(json);

                        // verify notebook is .net notebook
                        //if (jpynb.Metadata.Kernelspec.Name != context.HandlingKernel.Name)
                        //    throw new InvalidOperationException($"Kernel '{jpynb.Metadata.Kernelspec.Name}' not supported");

                        // foreach cells get kernel and call HandleSubmitCode

                        //context.CurrentKernel.KernelEvents.Subscribe(e => context.DisplayAsync(e.ToDisplayString()).GetAwaiter().GetResult()) ;
                        foreach (var cell in jpynb.Cells.Where(c=> c.CellType == CellType.Code))
                        {
                            var result = await context.CurrentKernel.SubmitCodeAsync(string.Join("\n",cell.Source));
                           
                        }

                        await context.DisplayAsync(new HtmlString($@"<b>Notebook {notebook} loaded</b>"));

                    });

                kernelBase.AddDirective(importCommand);
            }

            if (KernelInvocationContext.Current is { } context)
            {
                await context.DisplayAsync($"`{nameof(ImportExtension)}` is loaded. It adds the import notebook as module. Try it by running: `#!import path\\name`", "text/markdown");
            }
        }

        private static string GetValidNamespace(string ns)
        {
            return string.Join(".", ns.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(chunk => chunk)); // TODO: replace invalid char in chunk 
        }
    }
}
