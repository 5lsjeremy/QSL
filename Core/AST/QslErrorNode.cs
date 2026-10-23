using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslErrorNode : QslNode
    {
        public override QslNodeKind Kind =>
            QslNodeKind.Error;

        public string Message { get; }

        public QslErrorNode(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return $"Error: {Message}";
        }
    }
}