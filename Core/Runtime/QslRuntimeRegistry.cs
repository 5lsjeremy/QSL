using System;
using System.Collections.Generic;
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Evaluators;

namespace QSL.Core.Runtime
{
    public sealed class QslRuntimeRegistry : QslBaseObject
    {
        private readonly Dictionary<string, IQslOperator> _operators
            = new();

        private readonly Dictionary<string, IQslFunction> _functions
            = new();

        private readonly Dictionary<string, IQslDescriptor> _descriptors
            = new();

        private readonly Dictionary<string, IQslCapability> _capabilities
            = new();

        private readonly Dictionary<QslNodeKind, IQslEvaluator> _evaluators
            = new();

        public override string Type => "registry";

        public IReadOnlyDictionary<string, IQslOperator> Operators
            => _operators;

        public IReadOnlyDictionary<string, IQslFunction> Functions
            => _functions;

        public IReadOnlyDictionary<string, IQslDescriptor> Descriptors
            => _descriptors;

        public IReadOnlyDictionary<string, IQslCapability> Capabilities
            => _capabilities;

        public IReadOnlyDictionary<QslNodeKind, IQslEvaluator> Evaluators
            => _evaluators;

        public void RegisterDefaultEvaluators()
        {
            RegisterEvaluator(
                QslNodeKind.Literal,
                new QslLiteralEvaluator());

            RegisterEvaluator(
                QslNodeKind.Identifier,
                new QslIdentifierEvaluator());

            RegisterEvaluator(
                QslNodeKind.Unary,
                new QslUnaryEvaluator());

            RegisterEvaluator(
                QslNodeKind.Binary,
                new QslBinaryEvaluator());

            RegisterEvaluator(
                QslNodeKind.Call,
                new QslCallEvaluator());

            RegisterEvaluator(
                QslNodeKind.Embedded,
                new QslEmbeddedEvaluator());
            
            RegisterEvaluator(
                QslNodeKind.InterpolatedString,
                new QslInterpolatedStringEvaluator()
            );

            RegisterEvaluator(
                QslNodeKind.Block,
                new QslBlockEvaluator());

            RegisterEvaluator(
                QslNodeKind.Error,
                new QslErrorEvaluator());

            RegisterEvaluator(
                QslNodeKind.MemberAccess,
                new QslMemberAccessEvaluator());

            RegisterEvaluator(
                QslNodeKind.Ternary,
                new QslTernaryEvaluator());
        }

        public void RegisterType(IQslObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj is QslRuntimeObject runtimeObject)
            {
                runtimeObject.AppendSlug(
                    $"{runtimeObject.Type}-{runtimeObject.Name}");
            }

            switch (obj)
            {
                case IQslFunction function:
                    Register(
                        _functions,
                        function.Name,
                        function,
                        "function");
                    return;

                case IQslCapability capability:
                    Register(
                        _capabilities,
                        capability.Name,
                        capability,
                        "capability");
                    return;

                case IQslDescriptor descriptor:
                    Register(
                        _descriptors,
                        descriptor.Name,
                        descriptor,
                        "descriptor");
                    return;

                case IQslOperator op:
                    Register(
                        _operators,
                        op.Name,
                        op,
                        "operator");
                    return;

                case IQslEvaluator evaluator:
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        nameof(QslRuntimeRegistry),
                        $"Evaluator '{evaluator.Name}' requires a node-kind registration."
                    );
                    return;

                default:
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        nameof(QslRuntimeRegistry),
                        $"Unsupported QSL registry type: {obj.GetType().Name}"
                    );
                    return;
            }
        }

        public void RegisterEvaluator(
            QslNodeKind kind,
            IQslEvaluator evaluator)
        {
            if (evaluator == null)
                throw new ArgumentNullException(nameof(evaluator));

            if (evaluator is QslRuntimeObject runtimeObject)
            {
                runtimeObject.AppendSlug(
                    $"{runtimeObject.Type}-{runtimeObject.Name}");
            }

            if (_evaluators.ContainsKey(kind))
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    nameof(QslRuntimeRegistry),
                    $"Evaluator already registered for node kind: {kind}"
                );

                return;
            }

            _evaluators.Add(
                kind,
                evaluator);
        }

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node == null)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    nameof(QslRuntimeRegistry),
                    "Cannot evaluate a null node."
                );

                return null;
            }

            if (_evaluators.TryGetValue(
                    node.Kind,
                    out var evaluator))
            {
                return evaluator.Evaluate(
                    node,
                    context);
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                nameof(QslRuntimeRegistry),
                $"No evaluator registered for node kind: {node.Kind}"
            );

            return null;
        }

        private void Register<T>(
            Dictionary<string, T> registry,
            string key,
            T item,
            string itemType)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    nameof(QslRuntimeRegistry),
                    $"Cannot register {itemType} with an empty name."
                );

                return;
            }

            if (registry.ContainsKey(key))
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    nameof(QslRuntimeRegistry),
                    $"{itemType} '{key}' is already registered."
                );

                return;
            }

            registry.Add(
                key,
                item);
        }
    }
}