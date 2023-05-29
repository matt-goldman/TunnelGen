using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TunnelGen.Generators
{
    internal static class Log
    {
        public static List<string> Logs { get; } = new List<string>();

        public static void Print(string msg) => Logs.Add("//\t" + msg);

        // More print methods ...

        public static void FlushLogs(GeneratorExecutionContext context)
        {
            //foreach (var line in Logs)
            //{
            //    Debug.WriteLine(line);
            //}
            //context.AddSource($"logs.g.cs", SourceText.From(string.Join("\n", Logs), Encoding.UTF8));
        }
    }
}
