using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Capabilities;
using QSL.Core.Runtime.Context;
namespace QSL.Core.Runtime.Functions
{
    public sealed class ApplyForceFunction : QslRegistryItem, IQslFunction
    {
        private readonly PhysicsCapability _capability;

        public ApplyForceFunction(PhysicsCapability capability)
        {
            _capability = capability;
        }

        public override string Type => "function";
        public override string Name => "ApplyForce";

        public object? Invoke(object?[] args, QslRuntimeContext context)
        {
            // Delegate to the capabilityâ€™s ApplyForce method
            return _capability.ApplyForce(args, context);
        }
    }
}