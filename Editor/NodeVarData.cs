using Fractural.Utils;
using Godot;
using System;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    public class NodeVarData
    {
        // Serialized
        public Type ValueType { get; set; }
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
            var dict = new GDC.Dictionary()
            {
                { nameof(ValueType), ValueType.FullName },
                { nameof(Operation), (int)Operation },
            };
            if (ContainerPath != null)
                dict[nameof(ContainerPath)] = ContainerPath;
            if (Value != null)
                dict[nameof(Value)] = Value;
            if (IsFixed)
                dict[nameof(IsFixed)] = true;
            if (IsPointer)
            {
                dict[nameof(ContainerPath)] = ContainerPath;
                dict[nameof(ContainerVarName)] = ContainerVarName;
            }
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
