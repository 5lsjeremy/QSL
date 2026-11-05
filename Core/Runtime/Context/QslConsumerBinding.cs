using System;

namespace QSL.Core.Runtime.Context
{
    public sealed class QslConsumerBinding
    {
        public string Name { get; }

        public string Type { get; }

        public object Value { get; }

        public QslConsumerBinding(
            string name,
            string type,
            object value)
        {
            if (string.IsNullOrWhiteSpace(
                    name))
            {
                throw new ArgumentException(
                    "Consumer binding name cannot be empty.",
                    nameof(name)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    type))
            {
                throw new ArgumentException(
                    "Consumer type cannot be empty.",
                    nameof(type)
                );
            }

            Value =
                value
                ?? throw new ArgumentNullException(
                    nameof(value)
                );

            Name =
                name;

            Type =
                type;
        }
    }
}