using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

namespace Typhoon.AspNetCore.SignalR.Client.Generator;

internal class HubProxyClassesReceiver : ISyntaxContextReceiver
{
    private static readonly HashSet<string> HubProxyAttributeNames = 
    [
        typeof(HubListenerAttribute<>).FullName,
        typeof(HubInvokerAttribute<>).FullName,
        typeof(HubSenderAttribute<>).FullName
    ];
    
    public List<INamedTypeSymbol> HubProxyClasses { get; } = [];

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration
            && declaration.AttributeLists.SelectMany(al => al.Attributes).Any(IsLikeHubProxyAttribute)
            && context.SemanticModel.GetDeclaredSymbol(declaration) is INamedTypeSymbol symbol
            && symbol.GetAttributes().Any(IsHubProxyAttribute))
        {
            HubProxyClasses.Add(symbol);
        }
    }
    
    private static bool IsLikeHubProxyAttribute(AttributeSyntax attr)
    {
        var name = attr.Name.ToString();
        return name.Contains("HubListener") || 
               name.Contains("HubInvoker") || 
               name.Contains("HubSender");
    }

    private static bool IsHubProxyAttribute(AttributeData a)
    {
        return HubProxyAttributeNames
            .Contains(a.AttributeClass?.ToDisplayString());
    }
}