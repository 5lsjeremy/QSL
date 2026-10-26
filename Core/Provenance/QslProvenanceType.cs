namespace QSL.Core.Provenance
{
    public enum QslProvenanceType
    {
        Info, Warning, Error,
        Parse, Tokenize, Evaluate, Execute,
        DescriptorApply, CapabilityApply,
        Semantic, System, 
        Merge, Context, BlockCreate, BlockProcess
    }

    public enum QslProvenanceCategory
    {
        QslSyntax,
        QslRuntime,
        System,
        Evaluation,
        Capability
    }
}
