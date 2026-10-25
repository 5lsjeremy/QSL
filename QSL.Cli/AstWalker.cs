using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

internal sealed class AstWalkResult
{
    public int NodeCount { get; private set; }

    public IReadOnlyDictionary<QslNodeKind, int> KindCounts =>
        _kindCounts;

    private readonly Dictionary<QslNodeKind, int>
        _kindCounts = new();

    public void Record(
        QslNodeKind kind)
    {
        NodeCount++;

        if (_kindCounts.TryGetValue(
                kind,
                out var count))
        {
            _kindCounts[kind] =
                count + 1;

            return;
        }

        _kindCounts.Add(
            kind,
            1
        );
    }
}

internal static class AstWalker
{
    public static AstWalkResult Walk(
        IQslNode root)
    {
        ArgumentNullException.ThrowIfNull(
            root
        );

        var result =
            new AstWalkResult();

        var visited =
            new HashSet<object>(
                ReferenceEqualityComparer.Instance
            );

        Visit(
            root,
            result,
            visited
        );

        return result;
    }

    private static void Visit(
        IQslNode node,
        AstWalkResult result,
        HashSet<object> visited)
    {
        if (!visited.Add(node))
        {
            return;
        }

        result.Record(
            node.Kind
        );

        var properties =
            node.GetType().GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public
            );

        foreach (var property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? value;

            try
            {
                value =
                    property.GetValue(node);
            }
            catch
            {
                continue;
            }

            VisitValue(
                value,
                result,
                visited
            );
        }
    }

    private static void VisitValue(
        object? value,
        AstWalkResult result,
        HashSet<object> visited)
    {
        if (value is null)
        {
            return;
        }

        if (value is string)
        {
            return;
        }

        if (value is IQslNode child)
        {
            Visit(
                child,
                result,
                visited
            );

            return;
        }

        if (value is not IEnumerable sequence)
        {
            return;
        }

        foreach (var item in sequence)
        {
            if (item is IQslNode sequenceNode)
            {
                Visit(
                    sequenceNode,
                    result,
                    visited
                );
            }
        }
    }

    private sealed class ReferenceEqualityComparer :
        IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } =
            new();

        public new bool Equals(
            object? left,
            object? right)
        {
            return ReferenceEquals(
                left,
                right
            );
        }

        public int GetHashCode(
            object value)
        {
            return RuntimeHelpers.GetHashCode(
                value
            );
        }
    }
}