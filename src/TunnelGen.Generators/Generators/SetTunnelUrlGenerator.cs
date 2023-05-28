using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TunnelGen.Generators.Generators
{

    [Generator]
    public class SetTunnelUrlGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Log.Print("Executing code generator");

            try
            {
                Log.Print("Trying to get syntax receiver...");
                var receiver = (SyntaxReceiver)context.SyntaxReceiver;

                Log.Print("Got the syntax receiver...");

                var compilation = (CSharpCompilation)context.Compilation;
                var attributeSymbol = compilation.GetTypeByMetadataName("TunnelGen.Attributes.SetTunnelUrlAttribute");

                Log.Print("Got the attribute symbol...");

                if (!receiver.CandidateAttributes.Any())
                {
                    Log.Print("No attributes found");
                }
                else
                {
                    Log.Print($"Found {receiver.CandidateAttributes.Count} attributes");
                }

                foreach (var attributeSyntax in receiver.CandidateAttributes)
                {
                    Log.Print($"Getting semantic model for {attributeSyntax.Name}...");
                    
                    SemanticModel semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

                    Log.Print("Got the semantic model.");
                    
                    if (semanticModel.GetDeclaredSymbol(attributeSyntax.Parent.Parent) is IFieldSymbol fieldSymbol)
                    {
                        var firstAttribute = fieldSymbol.GetAttributes().FirstOrDefault(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeSymbol));
                        
                        if (firstAttribute is null)
                        {
                            Log.Print("No attributes matching the symbol found.");
                        }

                        if (firstAttribute is AttributeData attributeData)
                        {
                            string namespaceName = fieldSymbol.ContainingNamespace.ToDisplayString();
                            string className = fieldSymbol.ContainingType.Name;
                            string variableName = fieldSymbol.Name;
                            string tunnelName = attributeData.NamedArguments.FirstOrDefault(na => na.Key == "TunnelName").Value.Value?.ToString() ?? variableName;
                            string modifier = fieldSymbol.IsStatic ? "static" : null;

                            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TunnelUrl", out var tunnelUrl);

                            string generatedCode = GenerateClassWithTunnelUrlProperty(namespaceName, className, variableName, tunnelUrl, modifier);
                            context.AddSource($"{className}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                        }
                        else
                        {
                            Log.Print($"{firstAttribute.AttributeClass.Name} did not match {attributeSymbol}");
                        }
                    }
                    else
                    {
                        Log.Print($"Semantic Model {semanticModel} is not field symbol.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Print($"Something went wrong: {ex.Message}");
                Log.Print(ex.StackTrace);
            }

            Log.Print("Finished execution");

            Log.FlushLogs(context);
        }



        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            Log.Print("Initialising...");
            try
            {
                context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            }
            catch (Exception ex)
            {
                Log.Print($"Failed to register syntax receiver: {ex.Message}");
            }
            Log.Print("Set syntax receiver.");
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<AttributeSyntax> CandidateAttributes { get; } = new List<AttributeSyntax>();

            // Called for every syntax node in the compilation
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Look for attributes that have our specific names
                if (syntaxNode is AttributeSyntax attributeSyntax)
                {
                    if (attributeSyntax.Name.ToString().Contains("SetTunnelUrl"))
                    {
                        CandidateAttributes.Add(attributeSyntax);
                        Log.Print($"Candidate {attributeSyntax.Name} added");
                    }
                }
            }
        }

        private string GenerateClassWithTunnelUrlProperty(
            string namespaceName, string className, string variableName, string tunnelUrl,
            string modifier = null)
        {
            string pascalCaseVariableName = ConvertToPascalCase(variableName);
            
            var code = $@"
        namespace {Escape(namespaceName)}
        {{
            public {Escape(modifier)} partial class {Escape(className)}
            {{
                public {Escape(modifier)} string {Escape(pascalCaseVariableName)}
                {{
#if DEBUG
                    get => @""{Escape(tunnelUrl)}"";
#else
                    get => {variableName};
#endif
                }}
            }}
        }}";

            return code;
        }

        private string ConvertToPascalCase(string input)
        {
            // This is a simple implementation and might not handle all possible input correctly.
            // You may want to use a more robust implementation or a library function.
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        private string Escape(string input)
        {
            if (input is null)
                return null;

            // This is a simple implementation. You might want to escape other characters as well.
            return input.Replace("\"", "\\\"");
        }
    }
}