using System;
using System.Collections.Generic;
using QSL.Abstractions.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime;

namespace QSL.Core.Base
{
    public abstract class QslBaseObject : IQslObject
    {
        public Guid Id { get; } = Guid.NewGuid();

        private string? _slug;

        public string Slug => _slug ??= Id.ToSlug();

        /// <summary>
        /// Extends this object's existing heritage with another
        /// processing or transformation identifier.
        /// </summary>
        public void AppendSlug(string suffix)
        {
            _slug = $"{Slug}-{suffix}";
        }

        /// <summary>
        /// Establishes inherited or reconstructed heritage without
        /// requiring this object to have experienced each represented
        /// transformation itself.
        ///
        /// Intended for controlled operations such as cloning,
        /// deep copying, or restoration.
        /// </summary>
        protected void SetSlug(string newSlug)
        {
            _slug = newSlug;
        }

        private readonly List<QslProvenanceEntry> _provenance = new();

        public IReadOnlyList<QslProvenanceEntry> Provenance => _provenance;

        protected QslProvenanceEntry AddProvenance(
            QslProvenanceType type,
            QslProvenanceCategory category,
            string source,
            string message,
            Dictionary<string, object>? metadata = null)
        {
            var metadataSnapshot = metadata is null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(metadata);

            var entry = new QslProvenanceEntry
            {
                SubjectId = Id,
                SubjectSlug = Slug,
                EntryType = type,
                Category = category,
                Source = source,
                Message = message,
                Metadata = metadataSnapshot
            };

            _provenance.Add(entry);

            QslRuntime.Instance.ProvenanceBus.Publish(entry);

            return entry;
        }

        public abstract string Type { get; }
    }
}