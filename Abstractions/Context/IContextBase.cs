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

        void SetVariable(
            string key,
            object? value);

        bool RemoveVariable(
            string key);

        bool RemoveVariable(
            string key,
            object expectedValue);

        void ClearVariables();

        void SetMetadata(
            string key,
            object? value);

        bool RemoveMetadata(
            string key);

        void ClearMetadata();
    }
}