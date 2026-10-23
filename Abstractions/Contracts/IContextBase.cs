using System;
using System.Collections.Generic;

namespace QSL.Abstractions.Context
{
    public interface IContextBase
    {
        Guid Id { get; }
        string Slug { get; }

        IReadOnlyDictionary<string, object?> Variables { get; }
        IReadOnlyDictionary<string, object?> Metadata { get; }

        void SetVariable(string key, object value);
        void SetMetadata(string key, object value);
    }
}