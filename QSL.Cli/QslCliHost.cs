using QSL.Core;
using QSL.Core.Runtime;

internal sealed class QslCliHost
{
    public QslEngine Engine { get; }

    public QslRuntimeContext Context { get; }

    private QslCliHost(
        QslEngine engine,
        QslRuntimeContext context)
    {
        Engine = engine;
        Context = context;
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

        var engine =
            new QslEngine(
                context
            );

        return new QslCliHost(
            engine,
            context
        );
    }
}