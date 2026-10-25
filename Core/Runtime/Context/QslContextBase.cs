using System.Collections.Generic;
using QSL.Abstractions.Context;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Context
{
    public abstract class QslContextBase : QslBaseObject, IContextBase
    {
        private readonly Dictionary<string, object?> _variables = new();
        private readonly Dictionary<string, object?> _metadata = new();

        public IReadOnlyDictionary<string, object?> Variables => _variables;

        public IReadOnlyDictionary<string, object?> Metadata => _metadata;

        public void SetVariable(
            string key,
            object? value)
        {
            _variables[key] = value;

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Variable set: {key}",
                new Dictionary<string, object>
                {
                    ["value"] = value!
                }
            );
        }

        public object? GetVariable(string key)
        {
            return _variables.TryGetValue(
                key,
                out var value)
                ? value
                : null;
        }

        public void SetMetadata(
            string key,
            object? value)
        {
            _metadata[key] = value;

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Metadata set: {key}",
                new Dictionary<string, object>
                {
                    ["value"] = value!
                }
            );
        }

        public override string Type => "context";
    }
}