using Fractural.Utils;
using Godot;
using System;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    public class NodeVarData
    {
        // Serialized
        public Type ValueType { get; set; } = typeof(int);
        public NodeVarOperation Operation { get; set; }
        public string Name { get; set; }
        public bool IsFixed { get; set; }
        public string ContainerVarName { get; set; }
        public NodePath ContainerPath { get; set; }

        // Runtime
        public INodeVarsContainer Container { get; set; }
        public object Value { get; set; }

        /// <summary>
        /// Whether the NodeVar is a pointer to another NodeVar
        /// </summary>
        public bool IsPointer => ContainerPath != null;

        public GDC.Dictionary ToGDDict()
        {
            GD.Print("ToGDDict 1");
            var dict = new GDC.Dictionary()
            {
                { nameof(ValueType), ValueType.FullName },
                { nameof(Operation), (int)Operation },
            };
            GD.Print("ToGDDict 2");
            if (ContainerPath != null)
                dict[nameof(ContainerPath)] = ContainerPath;
            if (Value != null)
                dict[nameof(Value)] = Value;
            GD.Print("ToGDDict 3");
            if (IsFixed)
                dict[nameof(IsFixed)] = true;
            GD.Print("ToGDDict 4");
            if (IsPointer)
            {
                dict[nameof(ContainerPath)] = ContainerPath;
                dict[nameof(ContainerVarName)] = ContainerVarName;
            }
            GD.Print("ToGDDict 4");
            return dict;
        }

        public static NodeVarData FromGDDict(GDC.Dictionary dict, string name)
        {
            return new NodeVarData()
            {
                ValueType = ReflectionUtils.FindTypeFullName(dict.Get<string>(nameof(ValueType))),
                Operation = (NodeVarOperation)dict.Get<int>(nameof(Operation)),
                IsFixed = dict.Get(nameof(IsFixed), false),
                ContainerPath = dict.Get<NodePath>(nameof(ContainerPath), null),
                ContainerVarName = dict.Get<string>(nameof(ContainerVarName), null),
                Value = dict.Get<object>(nameof(Value), null),
                Name = name,
            };
        }
    }
}
