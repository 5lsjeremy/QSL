namespace QSL.Cli.Fixtures.Consumers
{
    public sealed class ConsumerPlayer
    {
        public ConsumerPlayer(
            double health,
            double mana)
        {
            Health = health;
            Mana = mana;
        }

        public double Health { get; set; }

        public double Mana { get; set; }
    }
}