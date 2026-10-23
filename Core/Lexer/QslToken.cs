namespace QSL.Core.Lexer
{
    public readonly struct QslToken
    {
        public QslTokenKind Kind { get; }
        public string Text { get; }

        public QslToken(QslTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public override string ToString() => $"{Kind}: {Text}";
    }
}