using QSL.Core.Provenance;

namespace QSL.Core.Runtime
{
    public sealed class QslRuntime
    {
        public static QslRuntime Instance { get; } = new QslRuntime();

        // Provenance bus (streaming, indexing, filtering, export)
        public QslProvenanceBus ProvenanceBus { get; } = new QslProvenanceBus();

        // Runtime registry (operators, functions, descriptors, capabilities, evaluators, block factories)
        public QslRuntimeRegistry Registry { get; }

        // Global runtime context (variables, state)
        public QslRuntimeContext Context { get; }

        private QslRuntime()
        {
            Registry = new QslRuntimeRegistry();
            Registry.RegisterDefaultEvaluators();

            Context = new QslRuntimeContext(Registry);
        }
    }
}