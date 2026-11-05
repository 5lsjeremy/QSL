using QSL.Cli.Fixtures.Capabilities;
using QSL.Cli.Fixtures.Consumers;
using QSL.Cli.Fixtures.Descriptors;
using QSL.Cli.Fixtures.Functions;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

internal sealed class QslCliHost
{
    public QslEngine Engine { get; }

    public QslRuntimeContext Context { get; }

    public QslConsumerBinding Player { get; }

    public QslConsumerBinding Action { get; }

    private QslCliHost(
        QslEngine engine,
        QslRuntimeContext context,
        QslConsumerBinding player,
        QslConsumerBinding action)
    {
        Engine =
            engine;

        Context =
            context;

        Player =
            player;

        Action =
            action;
    }

    public static QslCliHost Create()
    {
        var registry =
            new QslRuntimeRegistry();

        registry.RegisterDefaultEvaluators();

        registry.RegisterType(
            new HealFunction()
        );

        registry.RegisterType(
            new VectorFunction()
        );

        var physicsCapability =
            new PhysicsCapability();

        registry.RegisterType(
            physicsCapability
        );

        var playerDescriptor =
            new PlayerDescriptor();

        registry.RegisterType(
            playerDescriptor
        );

        var context =
            new QslRuntimeContext(
                registry
            );

        physicsCapability.Apply(
            context
        );

        playerDescriptor.Apply(
            context
        );

        /*
         * Consumer-owned runtime objects.
         *
         * These objects implement no QSL contract.
         */
        var playerValue =
            new ConsumerPlayer(
                50.0,
                25.0
            );

        var actionValue =
            new ConsumerAction();

        /*
         * Bindings describe how consumer-owned runtime objects
         * enter QSL-visible vocabulary.
         *
         * Name:
         *     QSL-visible identifier.
         *
         * Type:
         *     Consumer-owned classification.
         *
         * Value:
         *     Exact consumer-owned runtime object.
         */
        var player =
            new QslConsumerBinding(
                "player",
                "Player",
                playerValue
            );

        var action =
            new QslConsumerBinding(
                "action",
                "Action",
                actionValue
            );

        var engine =
            new QslEngine(
                context
            );

        /*
         * Bind through QslEngine rather than reaching directly
         * into the runtime context.
         *
         * The CLI now exercises the same public integration
         * boundary intended for external consumers such as Wilds.
         */
        engine.BindConsumer(
            player
        );

        engine.BindConsumer(
            action
        );

        return new QslCliHost(
            engine,
            context,
            player,
            action
        );
    }
}