namespace QSL.Core.Custom
{
    public sealed class Velocity
    {
        public double X { get; }
        public double Y { get; }

        public Velocity(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
            => $"Velocity(X={X}, Y={Y})";

        // Optional: add vector-like operations
        public Velocity Scale(double factor)
            => new Velocity(X * factor, Y * factor);

        public Velocity Add(Velocity other)
            => new Velocity(X + other.X, Y + other.Y);

        public Velocity Subtract(Velocity other)
            => new Velocity(X - other.X, Y - other.Y);
    }
}