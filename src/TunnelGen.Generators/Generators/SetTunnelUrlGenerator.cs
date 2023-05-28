using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TunnelGen.Attributes;

namespace TunnelGen.Generators.Generators
{

    [Generator]
    public class SetTunnelUrlGenerator : ISourceGenerator
    {
        //public void Execute(GeneratorExecutionContext context)
        //{
        //    if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
        //        return;

        //    foreach (var attributeSyntax in receiver.CandidateAttributes)
        //    {
        //        // Use semantic model to get detailed attribute information
        //        var model = context.Compilation.GetSemanticModel(attributeSyntax.SyntaxTree);
        //        var attributeSymbol = model.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;

        //        if (attributeSymbol == null)
        //            continue;

        //        var attributeClassSymbol = attributeSymbol.ContainingType;

        //        // Check attribute types directly
        //        if (attributeClassSymbol.Equals(context.Compilation.GetTypeByMetadataName(typeof(SetTunnelUrl).Name)))
        //        {
        //            var classSymbol = model.GetDeclaredSymbol(attributeSyntax.Parent.Parent);

        //            if (classSymbol == null)
        //                continue;

        //            var classSyntax = classSymbol.DeclaringSyntaxReferences[0].GetSyntax() as ClassDeclarationSyntax;

        //            if (classSyntax == null)
        //                continue;

        //            var namespaceSyntax = classSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        //            if (namespaceSyntax == null)
        //                continue;

        //            var namespaceName = namespaceSyntax.Name.ToString();
        //            var className = classSyntax.Identifier.ValueText;

        //            // Get the field name
        //            var fieldDeclarationSyntax = (FieldDeclarationSyntax)attributeSyntax.Parent.Parent;
        //            var variableName = fieldDeclarationSyntax.Declaration.Variables.First().Identifier.Text;

        //            // Get the tunnel name from the attribute, default to the field name if no tunnel name is provided
        //            var attributeData = attributeSymbol.GetAttributes().First();
        //            var tunnelNameArgument = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == "TunnelName");
        //            var tunnelName = tunnelNameArgument.Value.Value as string ?? variableName;

        //            // Generate the code
        //            string generatedCode = GenerateCode(attributeClassSymbol.Name, tunnelName);

        //            // Add source to the compilation
        //            context.AddSource(variableName, SourceText.From(generatedCode, Encoding.UTF8));
        //        }
        //    }
        //}

        public void Execute(GeneratorExecutionContext context)
        {
            Log.Print("Executing code generator");
            var receiver = (SyntaxReceiver)context.SyntaxReceiver;

            var compilation = (CSharpCompilation)context.Compilation;
            var attributeSymbol = compilation.GetTypeByMetadataName("TunnelGen.Attributes.SetTunnelUrl");

            foreach (var attributeSyntax in receiver.CandidateAttributes)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(attributeSyntax.Parent.Parent) is IFieldSymbol fieldSymbol &&
                    fieldSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol)) is AttributeData attributeData)
                {
                    string namespaceName = fieldSymbol.ContainingNamespace.ToDisplayString();
                    string className = fieldSymbol.ContainingType.Name;
                    string variableName = fieldSymbol.Name;
                    string tunnelName = attributeData.NamedArguments.FirstOrDefault(na => na.Key == "TunnelName").Value.Value?.ToString() ?? variableName;
                    string modifier = fieldSymbol.IsStatic ? "static" : null;

                    string generatedCode = GenerateClassWithTunnelUrlProperty(namespaceName, className, variableName, tunnelName, modifier);
                    context.AddSource($"{className}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                }
            }

            Log.FlushLogs(context);
        }



        public void Initialize(GeneratorInitializationContext context)
        {
            Log.Print("Initialising...");
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<AttributeSyntax> CandidateAttributes { get; } = new List<AttributeSyntax>();

            // Called for every syntax node in the compilation
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Look for attributes that have our specific names
                if (syntaxNode is AttributeSyntax attributeSyntax
                    && (attributeSyntax.Name.ToString().Contains(typeof(SetTunnelUrlAttribute).Name)))
                {
                    CandidateAttributes.Add(attributeSyntax);
                    Log.Print($"Candidate {attributeSyntax.Name} added");
                }
                else
                {
                    Log.Print($"{syntaxNode.TryGetInferredMemberName()} is not a candidate");
                }
            }
        }

        private string GenerateClassWithTunnelUrlProperty(
            string namespaceName, string className, string variableName, string tunnelName,
            string modifier = null)
        {
            string pascalCaseVariableName = ConvertToPascalCase(variableName);

            string tunnelUrl = GetTunnelUrl(tunnelName);

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


        private string GetTunnelUrl(string tunnelName)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(appDataPath, $".{tunnelName}", "vstunnel.txt");
                string value = File.ReadAllText(filePath);
                return value;
            }
            catch
            {
                return "Could not get tunnel URL";
            }
        }
    }
}