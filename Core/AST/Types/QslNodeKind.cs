namespace QSL.Core.Ast.Types
{
    public enum QslNodeKind
    {
        Literal,
        Identifier,
        Unary,
        Binary,
        Call,
        Embedded,
        InterpolatedString,
        Block,
        Error,
        MemberAccess,
        Ternary
    }
}