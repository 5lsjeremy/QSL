using System;

namespace QSL.Core.Extensions
{
    internal static class QslGuidExtensions
    {
        public static string ToSlug(this Guid guid)
        {
            var s = guid.ToString("N"); // 32 hex chars, no hyphens
            return $"{s[..4]}-{s[^4..]}";
        }
    }
}
