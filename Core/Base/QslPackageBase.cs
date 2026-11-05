using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Context
{
    public abstract class QslPackageBase :
        QslBaseObject,
        IQslPackage
    {
        public object Target { get; }

        public IReadOnlyDictionary<string, object?>
            Properties { get; }

        protected QslPackageBase(
            object target,
            IReadOnlyDictionary<string, object?> properties)
        {
            Target =
                target
                ?? throw new ArgumentNullException(
                    nameof(target)
                );

            Properties =
                properties
                ?? throw new ArgumentNullException(
                    nameof(properties)
                );

            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslPackageBase),
                "QSL package created.",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        Target.GetType().FullName
                        ?? Target.GetType().Name,

                    ["propertyCount"] =
                        Properties.Count
                }
            );
        }

        /// <summary>
        /// Records the completed application of this package
        /// through the QSL runtime surface.
        /// </summary>
        internal void RecordApplied(
            int affected)
        {
            AddProvenance(
                QslProvenanceType.Context,
                QslProvenanceCategory.QslRuntime,
                nameof(QslPackageBase),
                "QSL package applied.",
                new Dictionary<string, object>
                {
                    ["targetType"] =
                        Target.GetType().FullName
                        ?? Target.GetType().Name,

                    ["propertyCount"] =
                        Properties.Count,

                    ["affected"] =
                        affected
                }
            );
        }

        public override string Type =>
            "package";
    }
}