using System.Collections.Generic;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Packages
{
    public sealed class QslDecayPackage :
        QslPackageBase
    {
        public bool RemoveAtZero { get; }

        public QslDecayPackage(
            object target,
            IReadOnlyDictionary<string, object?> properties,
            bool removeAtZero = true)
            : base(
                target,
                properties)
        {
            RemoveAtZero =
                removeAtZero;
        }
    }
}
