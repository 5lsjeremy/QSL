namespace QSL.Core.Provenance
{
    public enum QslProvenanceType
    {
        Info, Warning, Error,
        Parse, Tokenize, Evaluate, Execute,
        DescriptorApply, CapabilityApply,
        Semantic, Belief, Trust, Memory,
        Temporal, Prediction, System, Routing,
        Merge, Context, BlockCreate, BlockProcess
    }

    public enum QslProvenanceCategory
    {
        QslSyntax,
        QslRuntime,
        WildsSemantic,
        WildsTemporal,
        WildsMemory,
        WildsPrediction,
        System,
        Evaluation,
        Capability
    }
}
