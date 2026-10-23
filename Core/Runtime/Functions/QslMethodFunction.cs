using System;
using System.Reflection;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Functions
{
    public sealed class QslMethodFunction : QslRegistryItem, IQslFunction
    {
        private readonly object _target;
        private readonly MethodInfo _method;

        public QslMethodFunction(object target, MethodInfo method)
        {
            _target = target;
            _method = method;
        }

        public override string Type => "function";
        public override string Name => _method.Name;

        public object? Invoke(object?[] args, QslRuntimeContext context)
        {
            try
            {
                return _method.Invoke(_target, args);
            }
            catch (Exception ex)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    $"Error invoking method '{_method.Name}' on '{_target.GetType().Name}': {ex.Message}"
                );

                return null;
            }
        }
    }
}