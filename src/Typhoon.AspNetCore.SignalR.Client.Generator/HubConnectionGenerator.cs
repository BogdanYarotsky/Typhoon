using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Typhoon.AspNetCore.SignalR.Client.Generator;

[Generator]
public class HubConnectionGenerator : IIncrementalGenerator
{
    private const string InitialCode = 
       """
          using System;
          namespace Typhoon.AspNetCore.SignalR.Client.Generator;
          [AttributeUsage(AttributeTargets.Class)]
          internal sealed class HubInvokerAttribute<T> : Attribute {}
          [AttributeUsage(AttributeTargets.Class)]
          internal sealed class HubListenerAttribute<T> : Attribute {}
       """;

    private const string HubListenerAttributeFullyQualifiedName 
        = "Typhoon.AspNetCore.SignalR.Client.Generator.HubListenerAttribute`1";
    
    private const string HubInvokerAttributeFullyQualifiedName 
        = "Typhoon.AspNetCore.SignalR.Client.Generator.HubInvokerAttribute`1";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource(
                "SignalRClientGenerator.g.cs",
                SourceText.From(InitialCode, Encoding.UTF8));
        });
        
        var listenerPipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            HubListenerAttributeFullyQualifiedName,
            predicate: IsSuitableClassDeclaration,
            transform: ExtractHubListenerModel);
        
        context.RegisterImplementationSourceOutput(listenerPipeline, AddHubListenerSource);
    }

    private static void AddHubListenerSource(SourceProductionContext context, HubListenerModel model)
    {
        context.AddSource($"{model.ClassName}.Listener.g.cs", 
            SourceText.From(model.ToSourceText(), Encoding.UTF8));
    }

    private static HubListenerModel ExtractHubListenerModel(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
    {
        var typeArgument = ctx.Attributes[0].AttributeClass!.TypeArguments[0];
        
        var requiredNamespaces = new HashSet<string>();
        var hubListenerMethods = new List<HubListenerMethod>();
        
        foreach (var method in typeArgument.GetMembers().OfType<IMethodSymbol>())
        {
            foreach (var parameter in method.Parameters)
            {
                if (parameter.ContainingNamespace is {} ns)
                {
                    requiredNamespaces.Add(ns.ToDisplayString());
                }
            }
        }
        

        var methods = typeArgument.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public)
            .Select(m =>
            {
                var requiredNamespaces = m.Parameters.Select(p => p.Type)
                    .Select(t => t.ContainingNamespace)
                    .Where(ns => ns != null)
                    .Select(ns => ns.ToDisplayString())
                    .Distinct()
                    .ToArray();

                var parameterTypeNames = m.Parameters.Select(p => p.Type.Name).ToArray();
                return new { Method = new HubListenerMethod(m.Name, new(parameterTypeNames)), requiredNamespaces };
            })
            .ToArray();

        return new(
            new(methods.SelectMany(m => m.requiredNamespaces).Distinct().ToArray()), 
            ctx.TargetSymbol.ContainingNamespace?.ToDisplayString(), 
            ctx.TargetSymbol.DeclaredAccessibility, ctx.TargetSymbol.Name, 
            new(methods.Select(m => m.Method).ToArray()));
    }

    private static bool IsSuitableClassDeclaration(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax declaration 
               && IsNotNestedClass(declaration)
               && IsStaticPartialClass(declaration);
    }

    private static bool IsStaticPartialClass(ClassDeclarationSyntax declaration)
    {
        var count = declaration.Modifiers.Count;
        if (count < 2) return false;
        
        const int staticKeywordKind = 8347;
        const int partialKeywordKind = 8406;

        var isStatic = false;
        var isPartial = false;

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < count; i++)
        {
            var modifier = declaration.Modifiers[i];
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (modifier.RawKind == staticKeywordKind)
            {
                if (isPartial) return true;
                isStatic = true;
            }
            else if (modifier.RawKind == partialKeywordKind)
            {
                if (isStatic) return true;
                isPartial = true;
            }
        }
        
        return false;
    }

    private static bool IsNotNestedClass(ClassDeclarationSyntax classDeclaration) 
        => classDeclaration.Parent
            is NamespaceDeclarationSyntax
            or FileScopedNamespaceDeclarationSyntax
            or CompilationUnitSyntax;

    private record HubListenerMethod(string Name, EqualArray<string> ParameterTypes);
    private record HubListenerModel(
        EqualArray<string> RequiredNamespaces,
        string Namespace, Accessibility Accessibility,
        string ClassName, EqualArray<HubListenerMethod> Methods)
    {

        public string ToSourceText()
        {
            var sb = new StringBuilder();

            foreach (var ns in RequiredNamespaces.Values)
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine("using Microsoft.AspNetCore.SignalR.Client;");

            if (Namespace is not null)
            {
                sb.AppendLine($"namespace {Namespace};");
            }

            var accessibility = Accessibility == Accessibility.Public ? "public" : "internal";
            sb.AppendLine($"{accessibility} static partial class {ClassName} {{");

            // Add methods
            foreach (var method in Methods.Values)
            {
                var parameters = string.Join(", ", method.ParameterTypes.Values);

                // Action version
                sb.AppendLine($"  public static IDisposable On{method.Name}(this HubConnection connection, Action<{parameters}> handler) => connection.On(\"{method.Name}\", handler);");

                // Func version
                sb.AppendLine($"  public static IDisposable On{method.Name}(this HubConnection connection, Func<{parameters}, Task> handler) => connection.On(\"{method.Name}\", handler);");
            }
            sb.Append("}");

            return sb.ToString();
        }
        
    };
}