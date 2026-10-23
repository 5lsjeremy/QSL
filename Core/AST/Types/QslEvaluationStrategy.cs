namespace QSL.Core.Lexer
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