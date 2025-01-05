using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Typhoon.AspNetCore.SignalR.Client.Generator;

[Generator]
public class HubProxyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(
            () => new HubProxyClassesReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not HubProxyClassesReceiver receiver)
            return;

        foreach (var hubProxyClass in receiver.HubProxyClasses)
        {
            var hubAttribute = hubProxyClass.GetAttributes()
                .FirstOrDefault(ad => ad.AttributeClass?.Name == "HubInvokerAttribute`1");
            var clientAttribute = hubProxyClass.GetAttributes()
                .FirstOrDefault(ad => ad.AttributeClass?.Name == "HubListenerAttribute`1");

            if (hubAttribute == null || clientAttribute == null) continue;

            var hubType = hubAttribute.AttributeClass?.TypeArguments[0];
            var clientType = clientAttribute.AttributeClass?.TypeArguments[0];

            if (hubType == null || clientType == null) continue;

            var sourceBuilder = new StringBuilder();
            GenerateProxyClass(sourceBuilder, hubProxyClass, hubType, clientType);

            context.AddSource($"{hubProxyClass.Name}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }

    private void GenerateProxyClass(StringBuilder source, INamedTypeSymbol classSymbol, ITypeSymbol hubType, ITypeSymbol clientType)
    {
        var ns = classSymbol.ContainingNamespace.ToDisplayString();
        
        source.AppendLine(
        $$"""
            using Microsoft.AspNetCore.SignalR.Client;
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace {{ns}}
            {
                public partial class {{classSymbol.Name}}
                {
                    public Listener On { get; }
                    public Invoker Invoke { get; }
                    public Sender Send { get; }
            
                    public {{classSymbol.Name}}(HubConnection connection)
                    {
                        On = new(connection);
                        Send = new(connection);
                        Invoke = new(connection);
                    }
            
                    public class Sender
                    {
                        private readonly HubConnection _c;
                        public Sender(HubConnection c) => _c = c;
            
                        {{GenerateServerMethods(hubType, "SendCoreAsync")}}
                    }
            
                    public class Invoker
                    {
                        private readonly HubConnection _c;
                        public Invoker(HubConnection c) => _c = c;
            
                        {{GenerateServerMethods(hubType, "InvokeCoreAsync")}}
                    }
            
                    public class Listener
                    {
                        private readonly HubConnection _c;
                        public Listener(HubConnection c) => _c = c;
            
                        {{GenerateClientMethods(clientType)}}
                    }
                }
            }
            """);
    }

    private string GenerateServerMethods(ITypeSymbol hubType, string methodType)
    {
        var methods = new StringBuilder();
        
        foreach (var member in hubType.GetMembers())
        {
            if (member is not IMethodSymbol method) continue;
            
            var parameters = string.Join(", ", method.Parameters
                .Select(p => $"{p.Type} {p.Name}"));
            
            var parametersList = string.Join(", ", method.Parameters
                .Select(p => p.Name));

            if (parameters.Length > 0)
                parameters += ", ";

            methods.AppendLine($@"            public Task {method.Name}({parameters}CancellationToken cancellationToken = default)
            {{
                return _c.{methodType}(nameof({method.Name}), new object[] {{{parametersList}}}, cancellationToken);
            }}");
        }

        return methods.ToString();
    }

    private string GenerateClientMethods(ITypeSymbol clientType)
    {
        var methods = new StringBuilder();
        
        foreach (var member in clientType.GetMembers())
        {
            if (member is not IMethodSymbol method) continue;
            
            var delegateParameters = string.Join(", ", method.Parameters
                .Select(p => $"{p.Type} {p.Name}"));
            
            methods.AppendLine($@"            public IDisposable {method.Name}(Func<{delegateParameters}, Task> handler)
            {{
                return _c.On(nameof({method.Name}), handler);
            }}");
        }

        return methods.ToString();
    }
}