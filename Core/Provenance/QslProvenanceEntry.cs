using System;
using System.Collections.Generic;

namespace QSL.Core.Provenance
{
    public sealed class QslProvenanceEntry
    {
        /// <summary>
        /// Stable identity of the object this provenance describes.
        /// </summary>
        public Guid SubjectId { get; init; }

        /// <summary>
        /// Heritage of the subject at the moment this entry was recorded.
        /// This is intentionally a snapshot because a subject's slug may
        /// continue to evolve through later processing.
        /// </summary>
        public string SubjectSlug { get; init; } = string.Empty;

        public QslProvenanceType EntryType { get; init; }

        public QslProvenanceCategory Category { get; init; }

        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Component or process responsible for producing the entry.
        /// </summary>
        public string Source { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, object> Metadata { get; init; }
            = new Dictionary<string, object>();
    }
}