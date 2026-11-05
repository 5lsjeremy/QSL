using System.Collections.Generic;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Packages
{
    public sealed class QslStatePackage :
        QslPackageBase
    {
        public QslStatePackage(
            object target,
            IReadOnlyDictionary<string, object?> properties)
            : base(
                target,
                properties)
        {
        }
    } 
}
