using System;
using System.Collections.Generic;
using System.Linq;

namespace QSL.Core.Provenance
{
    public sealed class QslProvenanceBus
    {
        private readonly List<QslProvenanceEntry> _entries = new();
        private readonly HashSet<QslProvenanceEntry> _published = new();

        private readonly Dictionary<string, List<QslProvenanceEntry>> _bySlug
            = new(StringComparer.Ordinal);

        private readonly Dictionary<Guid, List<QslProvenanceEntry>> _byGuid
            = new();

        private readonly Dictionary<QslProvenanceType, List<QslProvenanceEntry>> _byType
            = new();

        private readonly Dictionary<QslProvenanceCategory, List<QslProvenanceEntry>> _byCategory
            = new();

        public event Action<QslProvenanceEntry>? Stream;

        public IDisposable Subscribe(
            Func<QslProvenanceEntry, bool> filter,
            Action<QslProvenanceEntry> handler)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));

            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            void SubscriptionHandler(QslProvenanceEntry entry)
            {
                if (filter(entry))
                    handler(entry);
            }

            Stream += SubscriptionHandler;

            return new Subscription(
                () => Stream -= SubscriptionHandler
            );
        }

        internal void Publish(QslProvenanceEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            if (!_published.Add(entry))
                return;

            Index(entry);

            Stream?.Invoke(entry);
        }

        private void Index(QslProvenanceEntry entry)
        {
            _entries.Add(entry);

            IndexBySlug(entry);
            IndexByGuid(entry);
            IndexByType(entry);
            IndexByCategory(entry);
        }

        private void IndexBySlug(QslProvenanceEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.SubjectSlug))
                return;

            if (!_bySlug.TryGetValue(entry.SubjectSlug, out var entries))
            {
                entries = new List<QslProvenanceEntry>();
                _bySlug[entry.SubjectSlug] = entries;
            }

            entries.Add(entry);
        }

        private void IndexByGuid(QslProvenanceEntry entry)
        {
            if (!_byGuid.TryGetValue(entry.SubjectId, out var entries))
            {
                entries = new List<QslProvenanceEntry>();
                _byGuid[entry.SubjectId] = entries;
            }

            entries.Add(entry);
        }

        private void IndexByType(QslProvenanceEntry entry)
        {
            if (!_byType.TryGetValue(entry.EntryType, out var entries))
            {
                entries = new List<QslProvenanceEntry>();
                _byType[entry.EntryType] = entries;
            }

            entries.Add(entry);
        }

        private void IndexByCategory(QslProvenanceEntry entry)
        {
            if (!_byCategory.TryGetValue(entry.Category, out var entries))
            {
                entries = new List<QslProvenanceEntry>();
                _byCategory[entry.Category] = entries;
            }

            entries.Add(entry);
        }

        public IReadOnlyList<QslProvenanceEntry> GetAll()
        {
            return _entries.ToArray();
        }

        public IReadOnlyList<QslProvenanceEntry> GetForSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Array.Empty<QslProvenanceEntry>();

            return _bySlug.TryGetValue(slug, out var entries)
                ? entries.ToArray()
                : Array.Empty<QslProvenanceEntry>();
        }

        public IReadOnlyList<QslProvenanceEntry> GetForGuid(Guid id)
        {
            return _byGuid.TryGetValue(id, out var entries)
                ? entries.ToArray()
                : Array.Empty<QslProvenanceEntry>();
        }

        public IReadOnlyList<QslProvenanceEntry> GetByType(
            QslProvenanceType type)
        {
            return _byType.TryGetValue(type, out var entries)
                ? entries.ToArray()
                : Array.Empty<QslProvenanceEntry>();
        }

        public IReadOnlyList<QslProvenanceEntry> GetByCategory(
            QslProvenanceCategory category)
        {
            return _byCategory.TryGetValue(category, out var entries)
                ? entries.ToArray()
                : Array.Empty<QslProvenanceEntry>();
        }

        public IReadOnlyList<QslProvenanceEntry> GetBetween(
            DateTime start,
            DateTime end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException(
                    nameof(end),
                    "The end timestamp cannot precede the start timestamp."
                );

            return _entries
                .Where(entry =>
                    entry.TimestampUtc >= start &&
                    entry.TimestampUtc <= end)
                .ToArray();
        }

        public IReadOnlyList<(
            string slug,
            QslProvenanceType type,
            int count)> Compress()
        {
            return _bySlug
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .SelectMany(pair =>
                    pair.Value
                        .GroupBy(entry => entry.EntryType)
                        .OrderBy(group => group.Key)
                        .Select(group => (
                            slug: pair.Key,
                            type: group.Key,
                            count: group.Count()
                        )))
                .ToArray();
        }

        public string Export(
            Func<QslProvenanceEntry, bool>? filter = null)
        {
            IEnumerable<QslProvenanceEntry> entries = _entries;

            if (filter is not null)
                entries = entries.Where(filter);

            return System.Text.Json.JsonSerializer.Serialize(
                entries.ToArray()
            );
        }

        private sealed class Subscription : IDisposable
        {
            private Action? _unsubscribe;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                var unsubscribe = _unsubscribe;

                if (unsubscribe is null)
                    return;

                _unsubscribe = null;
                unsubscribe();
            }
        }
    }
}