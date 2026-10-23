using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslMemberAccessEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type =>
            "evaluator";

        public override string Name =>
            "member-access";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not
                QslMemberAccessNode memberNode)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Member-access evaluator received an incompatible node."
                );

                return null;
            }

            var targetValue =
                context.Evaluate(
                    memberNode.Target
                );

            if (targetValue is null)
            {
                Report(
                    memberNode,
                    "Target evaluated to null."
                );

                return null;
            }

            // Capability member access.
            if (targetValue is
                IQslCapability capability)
            {
                var method =
                    capability
                        .GetType()
                        .GetMethod(
                            memberNode.MemberName
                        );

                if (method is not null)
                    return method;

                Report(
                    memberNode,
                    $"Capability '{capability.Name}' has no " +
                    $"member '{memberNode.MemberName}'."
                );

                return null;
            }

            // Descriptor member access.
            //
            // Descriptor application is initialization behavior.
            // Reading a descriptor member must not reapply defaults
            // and overwrite existing runtime state.
            if (targetValue is
                IQslDescriptor descriptor)
            {
                var resolved =
                    context.GetVariable(
                        memberNode.MemberName
                    );

                if (resolved is not null)
                    return resolved;

                Report(
                    memberNode,
                    $"Member '{memberNode.MemberName}' not found " +
                    $"on descriptor '{descriptor.Name}'."
                );

                return null;
            }

            // Plain-object property access.
            var targetType =
                targetValue.GetType();

            var property =
                targetType.GetProperty(
                    memberNode.MemberName
                );

            if (property is not null)
            {
                return property.GetValue(
                    targetValue,
                    null
                );
            }

            // Plain-object field access.
            var field =
                targetType.GetField(
                    memberNode.MemberName
                );

            if (field is not null)
            {
                return field.GetValue(
                    targetValue
                );
            }

            Report(
                memberNode,
                $"Member '{memberNode.MemberName}' not found " +
                $"on object of type '{targetType.Name}'."
            );

            return null;
        }

        private void Report(
            QslMemberAccessNode memberNode,
            string message)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                $"{message} " +
                $"(target: '{memberNode.Target}', " +
                $"member: '{memberNode.MemberName}')"
            );
        }
    }
}