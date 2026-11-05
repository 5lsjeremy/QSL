using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Context
{
    public sealed class QslRuntimeContext :
        QslContextBase
    {
        public QslRuntimeRegistry Registry { get; }

        public QslAttachedStateStore AttachedState { get; }

        public QslRuntimeContext(
            QslRuntimeRegistry registry)
        {
            Registry =
                registry
                ?? throw new ArgumentNullException(
                    nameof(registry)
                );

            AttachedState =
                new QslAttachedStateStore();
        }

        public override string Type =>
            "runtime-context";

        /// <summary>
        /// Receives an approved state instruction for the exact
        /// target object.
        ///
        /// Authorization and semantic validation belong upstream.
        /// QSL accepts the supplied property name and value as
        /// runtime state available for subsequent computation.
        ///
        /// Existing attached state is not overwritten by receipt.
        /// Subsequent changes flow through normal state mutation.
        /// </summary>
        public bool ReceiveState(
            object target,
            string propertyName,
            object? value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    propertyName))
            {
                throw new ArgumentException(
                    "Property name cannot be empty.",
                    nameof(propertyName)
                );
            }

            var received =
                AttachedState.Introduce(
                    target,
                    propertyName,
                    value
                );

            if (!received)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslRuntimeContext),
                $"Attached state received: {propertyName}",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        target.GetType().FullName
                        ?? target.GetType().Name,

                    ["property"] =
                        propertyName,

                    ["value"] =
                        value
                        ?? "<null>"
                }
            );

            return true;
        }

        /// <summary>
        /// Applies an approved update to existing attached state.
        ///
        /// This operation does not introduce missing state.
        /// Authorization and transition calculation belong
        /// upstream.
        /// </summary>
        public bool UpdateState(
            object target,
            string propertyName,
            object? value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    propertyName))
            {
                throw new ArgumentException(
                    "Property name cannot be empty.",
                    nameof(propertyName)
                );
            }

            var updated =
                AttachedState.TrySet(
                    target,
                    propertyName,
                    value
                );

            if (!updated)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslRuntimeContext),
                $"Attached state updated: {propertyName}",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        target.GetType().FullName
                        ?? target.GetType().Name,

                    ["property"] =
                        propertyName,

                    ["value"] =
                        value
                        ?? "<null>"
                }
            );

            return true;
        }

        /// <summary>
        /// Receives an approved instruction to remove one attached
        /// state property from the exact target object.
        /// </summary>
        public bool RemoveState(
            object target,
            string propertyName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    propertyName))
            {
                throw new ArgumentException(
                    "Property name cannot be empty.",
                    nameof(propertyName)
                );
            }

            var removed =
                AttachedState.Remove(
                    target,
                    propertyName
                );

            if (!removed)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslRuntimeContext),
                $"Attached state removed: {propertyName}",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        target.GetType().FullName
                        ?? target.GetType().Name,

                    ["property"] =
                        propertyName
                }
            );

            return true;
        }

        /// <summary>
        /// Removes all attached state associated with the exact
        /// target object.
        /// </summary>
        public bool RemoveState(
            object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target)
                );
            }

            var removed =
                AttachedState.RemoveAll(
                    target
                );

            if (!removed)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslRuntimeContext),
                "All attached state removed.",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        target.GetType().FullName
                        ?? target.GetType().Name
                }
            );

            return true;
        }

        public object? Evaluate(
            IQslNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node)
                );
            }

            var key =
                node.Kind;

            if (Registry.Evaluators.TryGetValue(
                    key,
                    out var evaluator))
            {
                return evaluator.Evaluate(
                    node,
                    this
                );
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                nameof(QslRuntimeContext),
                $"No evaluator registered for node kind: {key}"
            );

            return null;
        }

        public object? ResolveIdentifier(
            string idName)
        {
            if (string.IsNullOrEmpty(
                    idName))
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    nameof(QslRuntimeContext),
                    "Attempted to resolve a null/empty identifier."
                );

                return null;
            }

            var variable =
                GetVariable(
                    idName
                );

            if (variable != null)
            {
                return variable;
            }

            if (Registry.Functions.TryGetValue(
                    idName,
                    out var function))
            {
                return function;
            }

            if (Registry.Descriptors.TryGetValue(
                    idName,
                    out var descriptor))
            {
                return descriptor;
            }

            if (Registry.Capabilities.TryGetValue(
                    idName,
                    out var capability))
            {
                return capability;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                nameof(QslRuntimeContext),
                $"Unresolved identifier: {idName}"
            );

            return null;
        }

        /// <summary>
        /// Clears runtime variables, metadata, and attached state.
        ///
        /// The runtime registry remains intact. Registered
        /// evaluators, functions, descriptors, and capabilities
        /// are not removed.
        /// </summary>
        public void Clear()
        {
            ClearVariables();
            ClearMetadata();
            AttachedState.Clear();

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslRuntimeContext),
                "Runtime context cleared."
            );
        }
    }
}