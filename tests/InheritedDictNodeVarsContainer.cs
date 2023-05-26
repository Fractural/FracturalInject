using Fractural.DependencyInjection;
using Godot;
using GDC = Godot.Collections;

namespace Tests
{
    [Tool]
    public class InheritedDictNodeVarsContainer : DictNodeVarsContainer
    {
        [Export]
        public Vector2 SomeVector2 { get; set; }
        [Export]
        public GDC.Dictionary SomeDictionary { get; set; }
        [Export]
        public GDC.Array SomeArray { get; set; }

        [DictNodeVar]
        public float MyFloatVar
        {
            get => GetDictNodeVar<float>(nameof(MyFloatVar));
            set => SetDictNodeVar(nameof(MyFloatVar), value);
        }

        [DictNodeVar]
        public bool MyBoolVar
        {
            get => GetDictNodeVar<bool>(nameof(MyBoolVar));
            set => SetDictNodeVar(nameof(MyBoolVar), value);
        }

        [DictNodeVar]
        public bool MySetVar
        {
            set => SetDictNodeVar(nameof(MySetVar), value);
        }

        [DictNodeVar]
        public bool MyGetVar
        {
            get => GetDictNodeVar<bool>(nameof(MyGetVar));
        }

        [DictNodeVar(NodeVarOperation.Set)]
        public bool MyAttributeSetVar
        {
            get => GetDictNodeVar<bool>(nameof(MyAttributeSetVar));
            set => SetDictNodeVar(nameof(MyAttributeSetVar), value);
        }

        [DictNodeVar(NodeVarOperation.Get)]
        public bool MyAttributeGetVar
        {
            get => GetDictNodeVar<bool>(nameof(MyAttributeGetVar));
            set => SetDictNodeVar(nameof(MyAttributeGetVar), value);
        }
    }
}