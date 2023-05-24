using System;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// Attribute to mark a property as a NodeVar that's settable from the inspector. Used within State nodes.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Class, AllowMultiple = false)]
    public class NodeVarAttribute : System.Attribute
    {
        public NodeVarOperation Operation { get; set; }
        public NodeVarAttribute(NodeVarOperation operation = NodeVarOperation.GetSet)
        {
            Operation = operation;
        }
    }
}