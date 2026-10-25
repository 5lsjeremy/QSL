namespace QSL.Core.Ast.Types
{
    public enum QslEvaluationStrategy
    {
        Eager,
        ShortCircuitAnd,
        ShortCircuitOr,
        NullCoalescing,
        Arrow,
        Assignment
    }
}