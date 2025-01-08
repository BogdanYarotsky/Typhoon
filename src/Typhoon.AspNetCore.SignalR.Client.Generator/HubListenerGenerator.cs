using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

namespace Typhoon.AspNetCore.SignalR.Client.Generator;

[Generator]
public class HubListenerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: typeof(HubListenerAttribute<>).FullName!, 
            predicate: static (_, _) => true,
            transform: static (ctx, _) =>
            {
                var typeArgument = ctx
                    .Attributes.Single()
                    .AttributeClass!.TypeArguments.Single();
                
                var methods = typeArgument
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                    .Select(m =>
                    {
                        var requiredNamespaces = m.Parameters
                            .Select(p => p.Type)
                            .Append(m.ReturnType)
                            .Select(t => t.ContainingNamespace)
                            .Where(ns => ns != null)
                            .Select(ns => ns.ToDisplayString())
                            .Distinct()
                            .ToArray();

                        var parameterTypeNames = m.Parameters.Select(p => p.Type.Name).ToArray();
                        return new
                        {
                            m.Name, 
                            ReturnTypeName = m.ReturnType.Name, 
                            parameterTypeNames, 
                            requiredNamespaces
                        };
                    })
                    .ToArray();
                
                
                return new Model(
                    ctx.TargetSymbol.Name,
                    ctx.TargetSymbol.ContainingNamespace.ToDisplayString(),
                    methods.SelectMany(m => m.requiredNamespaces).Distinct().ToArray());
            });
        
        context.RegisterImplementationSourceOutput(pipeline, static (context, model) =>
        {
            var sourceText = SourceText.From($$"""
               {{string.Join("\n", model.RequiredNamespaces.Select(ns => $"using {ns};"))}}
               using Microsoft.AspNetCore.SignalR.Client;
               
               namespace {{model.ClassNamespace}}
               {
                   public interface I{{model.ClassName}}
                   {
                   }
               
                   public partial class {{model.ClassName}} : I{{model.ClassName}}
                   {
                       private readonly HubConnection _hubConnection;
                       
                       public {{model.ClassName}}(HubConnection hubConnection)
                       {
                            _hubConnection = hubConnection 
                                ?? throw new ArgumentNullException(nameof(hubConnection));
                       }
                   
                       public void Test()
                       {
                            Console.WriteLine("{{model}}");
                           // generated code 3
                       }
                   }
               }
               """, Encoding.UTF8);

            context.AddSource($"{model.ClassName}.g.cs", sourceText);
        });
    }

    private record InterfaceMethodInfo();
    private record Model(string ClassName, string ClassNamespace, string[] RequiredNamespaces);
}