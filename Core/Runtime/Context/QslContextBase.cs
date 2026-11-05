using System;
using System.Collections.Generic;
using QSL.Abstractions.Context;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Context
{
    public abstract class QslContextBase :
        QslBaseObject,
        IContextBase
    {
        private readonly Dictionary<string, object?>
            _variables =
                new Dictionary<string, object?>();

        private readonly Dictionary<string, object?>
            _metadata =
                new Dictionary<string, object?>();

        public IReadOnlyDictionary<string, object?>
            Variables =>
                _variables;

        public IReadOnlyDictionary<string, object?>
            Metadata =>
                _metadata;

        public void SetVariable(
            string key,
            object? value)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                throw new ArgumentException(
                    "Variable key cannot be empty.",
                    nameof(key)
                );
            }

            _variables[key] =
                value;

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Variable set: {key}",
                new Dictionary<string, object>
                {
                    ["value"] =
                        value ?? "<null>"
                }
            );
        }

        public object? GetVariable(
            string key)
        {
            return _variables.TryGetValue(
                key,
                out var value)
                ? value
                : null;
        }

        public bool RemoveVariable(
            string key)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                return false;
            }

            var removed =
                _variables.Remove(
                    key
                );

            if (!removed)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Variable removed: {key}"
            );

            return true;
        }

        /// <summary>
        /// Removes a variable only when it still references the
        /// expected exact runtime object.
        ///
        /// This prevents a stale consumer binding from removing
        /// a newer binding that reused the same identifier.
        /// </summary>
        public bool RemoveVariable(
            string key,
            object expectedValue)
        {
            if (string.IsNullOrWhiteSpace(
                    key) ||
                expectedValue == null)
            {
                return false;
            }

            if (!_variables.TryGetValue(
                    key,
                    out var currentValue))
            {
                return false;
            }

            if (!ReferenceEquals(
                    currentValue,
                    expectedValue))
            {
                return false;
            }

            return RemoveVariable(
                key
            );
        }

        public void ClearVariables()
        {
            if (_variables.Count == 0)
            {
                return;
            }

            var count =
                _variables.Count;

            _variables.Clear();

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Variables cleared: {count}"
            );
        }

        public void SetMetadata(
            string key,
            object? value)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                throw new ArgumentException(
                    "Metadata key cannot be empty.",
                    nameof(key)
                );
            }

            _metadata[key] =
                value;

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Metadata set: {key}",
                new Dictionary<string, object>
                {
                    ["value"] =
                        value ?? "<null>"
                }
            );
        }

        public bool RemoveMetadata(
            string key)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                return false;
            }

            var removed =
                _metadata.Remove(
                    key
                );

            if (!removed)
            {
                return false;
            }

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Metadata removed: {key}"
            );

            return true;
        }

        public void ClearMetadata()
        {
            if (_metadata.Count == 0)
            {
                return;
            }

            var count =
                _metadata.Count;

            _metadata.Clear();

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslContextBase),
                $"Metadata cleared: {count}"
            );
        }

        public override string Type =>
            "context";
    }
}