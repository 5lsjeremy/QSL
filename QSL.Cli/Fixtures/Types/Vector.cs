namespace QSL.Cli.Fixtures.Types
{
    public readonly struct Vector
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector(
            double x,
            double y)
            : this(
                x,
                y,
                0.0)
        {
        }

        public Vector(
            double x,
            double y,
            double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return Z == 0.0
                ? $"Vector(X={X}, Y={Y})"
                : $"Vector(X={X}, Y={Y}, Z={Z})";
        }
    }
}
