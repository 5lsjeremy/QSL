#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class IsExternalInit : Attribute { }
}
#endif