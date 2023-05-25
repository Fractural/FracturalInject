using Fractural.DependencyInjection;
using Godot;

namespace Tests
{
    [Tool]
    public class InheritedNodeVarsContainer : NodeVarsContainer
    {
        [NodeVar]
        public float MyFloatVar
        {
            get => GetNodeVar<float>(nameof(MyFloatVar));
            set => SetNodeVar(nameof(MyFloatVar), value);
        }

        [NodeVar]
        public bool MyBoolVar
        {
            get => GetNodeVar<bool>(nameof(MyBoolVar));
            set => SetNodeVar(nameof(MyBoolVar), value);
        }

        [NodeVar]
        public bool MySetVar
        {
            set => SetNodeVar(nameof(MySetVar), value);
        }

        [NodeVar]
        public bool MyGetVar
        {
            get => GetNodeVar<bool>(nameof(MyGetVar));
        }

        [NodeVar(NodeVarOperation.Set)]
        public bool MyAttributeSetVar
        {
            get => GetNodeVar<bool>(nameof(MyAttributeSetVar));
            set => SetNodeVar(nameof(MyAttributeSetVar), value);
        }

        [NodeVar(NodeVarOperation.Get)]
        public bool MyAttributeGetVar
        {
            get => GetNodeVar<bool>(nameof(MyAttributeGetVar));
            set => SetNodeVar(nameof(MyAttributeGetVar), value);
        }
    }
}