using System;
using System.Collections.Generic;
using QSL.Core.Provenance;

namespace QSL.Abstractions.Base
{
    // ------------------------------------------------------------
    // QSL Object (root of everything)
    // ------------------------------------------------------------
    public interface IQslObject
    {
        Guid Id { get; }
        string Slug { get; }
        string Type { get; }
        IReadOnlyList<QslProvenanceEntry> Provenance { get; }
    }
}