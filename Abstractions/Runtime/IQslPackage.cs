using System.Collections.Generic;

namespace QSL.Abstractions.Runtime
{
    public interface IQslPackage
    {
        object Target { get; }

        IReadOnlyDictionary<string, object?>
            Properties { get; }
    }
}