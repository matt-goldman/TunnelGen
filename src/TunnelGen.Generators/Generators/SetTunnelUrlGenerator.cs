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
            Debug.WriteLine("Executing code generator");

            try
            {
                Debug.WriteLine("Trying to get syntax receiver...");
                var receiver = (SyntaxReceiver)context.SyntaxReceiver;

                Debug.WriteLine("Got the syntax receiver...");

                var compilation = (CSharpCompilation)context.Compilation;
                var attributeSymbol = compilation.GetTypeByMetadataName("TunnelGen.Attributes.SetTunnelUrlAttribute");
                Debug.WriteLine($"Attribute symbol full name: {attributeSymbol.ToDisplayString()}");


                Debug.WriteLine("Got the attribute symbol...");

                if (!receiver.CandidateAttributes.Any())
                {
                    Debug.WriteLine("No attributes found");
                }
                else
                {
                    Debug.WriteLine($"Found {receiver.CandidateAttributes.Count} attributes");
                }

                foreach (var attributeSyntax in receiver.CandidateAttributes)
                {
                    Debug.WriteLine($"Getting semantic model for {attributeSyntax.Name}...");

                    SemanticModel semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

                    Debug.WriteLine("Got the semantic model.");

                    Debug.WriteLine($"attributeSyntax.Parent is of type {attributeSyntax.Parent.GetType().Name}, with kind {attributeSyntax.Parent.Kind()}");
                    Debug.WriteLine($"attributeSyntax.Parent.Parent is of type {attributeSyntax.Parent.Parent.GetType().Name}, with kind {attributeSyntax.Parent.Parent.Kind()}");

                    var parentParent = attributeSyntax.Parent.Parent;
                    Debug.WriteLine($"Parent.Parent is of type {parentParent.GetType().Name}, with kind {parentParent.Kind()}");
                    if (parentParent is FieldDeclarationSyntax fieldDecl)
                    {
                        Debug.WriteLine($"Field name: {fieldDecl.Declaration.Variables.First().Identifier.Text}");
                    }
                    else if (parentParent is PropertyDeclarationSyntax propDecl)
                    {
                        Debug.WriteLine($"Property name: {propDecl.Identifier.Text}");
                    }

                    if (attributeSyntax.Parent.Parent is FieldDeclarationSyntax fieldDeclarationSyntax)
                    {
                        Debug.WriteLine($"{attributeSyntax.Parent.Parent} is FieldDeclarationSyntax");
                        foreach (var variable in fieldDeclarationSyntax.Declaration.Variables)
                        {
                            var fieldSymbol = semanticModel.GetDeclaredSymbol(variable);
                            if (fieldSymbol != null)
                            {
                                Debug.WriteLine($"{fieldSymbol} is not null");

                                foreach (var attribute in fieldSymbol.GetAttributes())
                                {
                                    Debug.WriteLine($"Attribute on field: {attribute.AttributeClass.ToDisplayString()}");
                                }


                                var firstAttribute = fieldSymbol.GetAttributes().FirstOrDefault(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeSymbol));

                                if (firstAttribute is null)
                                {
                                    Debug.WriteLine("No attributes matching the symbol found.");
                                }

                                if (firstAttribute is AttributeData attributeData)
                                {
                                    Debug.WriteLine($"{firstAttribute} is {attributeData}");
                                    string namespaceName = fieldSymbol.ContainingNamespace.ToDisplayString();
                                    string className = fieldSymbol.ContainingType.Name;
                                    string variableName = fieldSymbol.Name;
                                    string tunnelName = attributeData.NamedArguments.FirstOrDefault(na => na.Key == "TunnelName").Value.Value?.ToString() ?? variableName;
                                    string modifier = fieldSymbol.IsStatic ? "static" : null;

                                    string tunnelProperty = $"build_property_{tunnelName}";

                                    context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(tunnelProperty, out var tunnelUrl);

                                    context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DefaultTunnel", out var defaultTunnelUrl);

                                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0004", "Debug", $"[TunnelUrlGenerator] Named TunnelUrl: {tunnelUrl}", "Debug", DiagnosticSeverity.Warning, true), null));
                                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0004", "Debug", $"[TunnelUrlGenerator] Default TunnelUrl: {tunnelUrl}", "Debug", DiagnosticSeverity.Warning, true), null));


                                    if (string.IsNullOrWhiteSpace(tunnelUrl))
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0002", "TunnelUrlMissing", $"Tunnel URL for '{tunnelName}' not found. Falling back to the default tunnel URL.", "TunnelGenCategory", DiagnosticSeverity.Warning, true), null));
                                        tunnelUrl = string.IsNullOrWhiteSpace(tunnelUrl) ? defaultTunnelUrl : tunnelUrl;
                                    }

                                    if (string.IsNullOrWhiteSpace(tunnelUrl))
                                    {
                                        var tunnelUrlFile = context.AdditionalFiles
                                                .FirstOrDefault(f => Path.GetFileName(f.Path).Equals("tunnelurl.txt"));

                                        if (tunnelUrlFile != null)
                                        {
                                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0003", "TunnelUrlFileFound", "Tunnel URL file found in additional files.", "TunnelGenCategory", DiagnosticSeverity.Warning, true), null));
                                            tunnelUrl = tunnelUrlFile.GetText(context.CancellationToken)?.ToString();
                                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0003", "TunnelUrlValue", $"Tunnel URL value read from additional files: {tunnelUrl}", "TunnelGenCategory", DiagnosticSeverity.Warning, true), null));
                                        }
                                        else
                                        {
                                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0003", "TunnelUrlFileMissing", "Unable to read the tunnel URL file.", "TunnelGenCategory", DiagnosticSeverity.Warning, true), null));
                                        }
                                    }

                                    if (string.IsNullOrWhiteSpace(tunnelUrl))
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG0003", "TunnelUrlMissing", "Both specific and default tunnel URLs are missing. Code generation halted.", "TunnelGenCategory", DiagnosticSeverity.Error, true), null));
                                        return;
                                    }

                                    //string tunnelUrl = "https://urlhardcodedinsourcegen.com";

                                    string generatedCode = GenerateClassWithTunnelUrlProperty(namespaceName, className, variableName, tunnelUrl, modifier);

                                    Debug.WriteLine("Generated this code");
                                    Debug.WriteLine(generatedCode);

                                    context.AddSource($"{className}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                                }
                                else
                                {
                                    Debug.WriteLine($"{firstAttribute.AttributeClass.Name} did not match {attributeSymbol}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"{variable} FieldSymbol us null");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"{attributeSyntax.Parent.Parent} is not FieldDeclarationSyntax");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Something went wrong: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
            
            Debug.WriteLine("Finished execution");
        }



        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            Debug.WriteLine("Initialising...");
            try
            {
                context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to register syntax receiver: {ex.Message}");
            }
            Debug.WriteLine("Set syntax receiver.");
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
                        Debug.WriteLine($"Candidate {attributeSyntax.Name} added");
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