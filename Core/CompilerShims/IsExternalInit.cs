#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    internal sealed class IsExternalInit : Attribute { }
}
#endif