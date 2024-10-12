using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Playgound;

[Generator]
public class TypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationContext = context.CompilationProvider.Select(
            static (x, _) =>
                (
                    Compilation: x,
                    WellKnownTypes: new WellKnownTypes(x),
                    FileNameBuilder: new FileNameBuilder()
                )
        );

        var mapperDefinitions = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(MapAttribute).FullName!,
                static (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (ctx.TargetSymbol, TargetNode: (ClassDeclarationSyntax)ctx.TargetNode)
            )
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select(
                static (x, _) =>
                    new MapperDefinition((INamedTypeSymbol)x.TargetSymbol, x.TargetNode)
            );

        var mappers = mapperDefinitions
            .Combine(compilationContext)
            .Select(
                static (x, _) =>
                    new MapperNode(x.Right.FileNameBuilder.Build("example"), SourceEmitter.Build())
            );

        context.RegisterImplementationSourceOutput(
            mappers,
            static (spc, mapper) =>
                spc.AddSource(
                    mapper.FileName,
                    SourceText.From(mapper.Body.ToFullString(), Encoding.UTF8)
                )
        );
    }
}

public readonly record struct MapperNode(string FileName, CompilationUnitSyntax Body);

public readonly record struct MapperDefinition(
    INamedTypeSymbol TargetSymbol,
    ClassDeclarationSyntax TargetNode
);

public class WellKnownTypes(Compilation compilation)
{
    private readonly Dictionary<string, INamedTypeSymbol?> _cachedTypes = new();

    // use string type name as they are not available in netstandard2.0
    public INamedTypeSymbol? DateOnly => TryGet("System.DateOnly");

    public INamedTypeSymbol? TimeOnly => TryGet("System.TimeOnly");

    public ITypeSymbol GetArrayType(ITypeSymbol type) =>
        compilation.CreateArrayTypeSymbol(type, elementNullableAnnotation: type.NullableAnnotation);

    public ITypeSymbol GetArrayType(
        ITypeSymbol elementType,
        int rank,
        NullableAnnotation elementNullableAnnotation
    ) => compilation.CreateArrayTypeSymbol(elementType, rank, elementNullableAnnotation);

    public INamedTypeSymbol Get<T>() => Get(typeof(T));

    public INamedTypeSymbol Get(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }

        return Get(
            type.FullName
                ?? throw new InvalidOperationException("Could not get name of type " + type)
        );
    }

    public INamedTypeSymbol? TryGet(string typeFullName)
    {
        if (_cachedTypes.TryGetValue(typeFullName, out var typeSymbol))
        {
            return typeSymbol;
        }

        typeSymbol = GetBestTypeByMetadataName(compilation, typeFullName);
        _cachedTypes.Add(typeFullName, typeSymbol);

        return typeSymbol;
    }

    private INamedTypeSymbol Get(string typeFullName) =>
        TryGet(typeFullName)
        ?? throw new InvalidOperationException("Could not get type " + typeFullName);

    public static INamedTypeSymbol? GetBestTypeByMetadataName(
        Compilation compilation,
        string fullyQualifiedMetadataName
    )
    {
#if ROSLYN4_4_OR_GREATER
        INamedTypeSymbol? type = null;

        foreach (var currentType in compilation.GetTypesByMetadataName(fullyQualifiedMetadataName))
        {
            if (ReferenceEquals(currentType.ContainingAssembly, compilation.Assembly))
                return currentType;

            switch (currentType.GetResultantVisibility())
            {
                case SymbolVisibility.Public:
                case SymbolVisibility.Internal
                    when currentType.ContainingAssembly.GivesAccessTo(compilation.Assembly):
                    break;

                default:
                    continue;
            }

            if (type != null)
                return null;

            type = currentType;
        }

        return type;
#else
        // RS0030 banned api: GetTypesByMetadataName is not supported for Roslyn < 4.4
#pragma warning disable RS0030
        return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
#pragma warning restore RS0030
#endif
    }

    // Copy from https://github.com/dotnet/roslyn/blob/d2ff1d83e8fde6165531ad83f0e5b1ae95908289/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L28-L73
    private static SymbolVisibility GetResultantVisibility(ISymbol symbol)
    {
        while (true)
        {
            // Start by assuming it's visible.
            var visibility = SymbolVisibility.Public;
            switch (symbol.Kind)
            {
                case SymbolKind.Alias:
                    // Aliases are uber private.  They're only visible in the same file that they
                    // were declared in.
                    return SymbolVisibility.Private;

                case SymbolKind.Parameter:
                    // Parameters are only as visible as their containing symbol
                    symbol = symbol.ContainingSymbol;
                    continue;

                case SymbolKind.TypeParameter:
                    // Type Parameters are private.
                    return SymbolVisibility.Private;
            }

            while (symbol != null && symbol.Kind != SymbolKind.Namespace)
            {
                switch (symbol.DeclaredAccessibility)
                {
                    // If we see anything private, then the symbol is private.
                    case Accessibility.NotApplicable:
                    case Accessibility.Private:
                        return SymbolVisibility.Private;

                    // If we see anything internal, then knock it down from public to
                    // internal.
                    case Accessibility.Internal:
                    case Accessibility.ProtectedAndInternal:
                        visibility = SymbolVisibility.Internal;
                        break;
                    // For anything else (Public, Protected, ProtectedOrInternal), the
                    // symbol stays at the level we've gotten so far.
                }
                symbol = symbol.ContainingSymbol;
            }

            return visibility;
        }
    }

    private enum SymbolVisibility
    {
        Public,
        Internal,
        Private,
    }
}

public class FileNameBuilder
{
    private const string GeneratedFileSuffix = ".g.cs";

    private readonly UniqueNameBuilder _uniqueNameBuilder = new();

    internal string Build(string mapperName) =>
        _uniqueNameBuilder.New(mapperName) + GeneratedFileSuffix;
}

public class UniqueNameBuilder()
{
    private readonly HashSet<string> _usedNames = new(StringComparer.Ordinal);
    private readonly UniqueNameBuilder? _parentScope;

    private UniqueNameBuilder(UniqueNameBuilder parentScope)
        : this()
    {
        _parentScope = parentScope;
    }

    public void Reserve(string name) => _usedNames.Add(name);

    public UniqueNameBuilder NewScope() => new(this);

    public string New(string name)
    {
        var i = 0;
        var uniqueName = name;
        while (Contains(uniqueName))
        {
            i++;
            uniqueName = name + i;
        }

        _usedNames.Add(uniqueName);

        return uniqueName;
    }

    public string New(string name, IEnumerable<string> reservedNames)
    {
        var scope = NewScope();
        scope.Reserve(reservedNames);
        var uniqueName = scope.New(name);
        _usedNames.Add(uniqueName);
        return uniqueName;
    }

    private void Reserve(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            _usedNames.Add(name);
        }
    }

    private bool Contains(string name)
    {
        if (_usedNames.Contains(name))
            return true;

        if (_parentScope != null)
            return _parentScope.Contains(name);

        return false;
    }
}
