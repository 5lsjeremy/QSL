# Evaluators

Evaluators bridge an AST node to the current runtime context.

```csharp
public interface IQslEvaluator : IQslRegistryItem
{
    object? Evaluate(IQslNode node, QslRuntimeContext context);
}
```

## Standard Evaluators

QSL Runtime 1.0 registers evaluators for literal, identifier, unary, binary, call, embedded, interpolated string, block, error, member access, and ternary nodes.

## Registration

Evaluators require an explicit node-kind mapping:

```csharp
registry.RegisterEvaluator(QslNodeKind.Literal, evaluator);
```

Passing an evaluator through `RegisterType` is rejected because a node kind is required.

## Custom Evaluator Guidance

Implement a custom evaluator only when extending an AST node processing path. Ordinary host data and behavior should use descriptors, capabilities, functions, and runtime objects instead.

A custom evaluator must validate the node type it supports and preserve the runtime diagnostic path.
