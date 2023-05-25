using Fractural.Commons;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    public static class NodeVarsContainerExtensions
    {
        public static T GetNodeVar<T>(this INodeVarsContainer container, string key) => (T)container.GetNodeVar(key);
    }

    public interface INodeVarsContainer
    {
        NodeVarData[] GetNodeVarsList();
        object GetNodeVar(string key);
        void SetNodeVar(string key, object value);
    }

    [RegisteredType(nameof(NodeVarsContainer), "res://addons/FracturalInject/Assets/dependency-container.svg")]
    [Tool]
    public class NodeVarsContainer : Node, INodeVarsContainer
    {
        // Native C# Dictionary is around x9 faster than Godot Dictionary
        public IDictionary<string, NodeVarData> NodeVars { get; private set; }

        private GDC.Dictionary _nodeVars;

        private HintString.NodeVarsMode _mode = HintString.NodeVarsMode.LocalAttributes;
        [Export]
        public HintString.NodeVarsMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                PropertyListChangedNotify();
            }
        }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            NodeVars = new Dictionary<string, NodeVarData>();
            foreach (string key in _nodeVars)
            {
                var data = NodeVarData.FromGDDict(_nodeVars.Get<GDC.Dictionary>(key), key);
                NodeVars[key] = data;

                if (data.IsPointer)
                    data.Container = GetNode<INodeVarsContainer>(data.ContainerPath);
            }
        }

        public T GetNodeVar<T>(string key) => (T)GetNodeVar(key);
        public object GetNodeVar(string key)
        {
            var data = NodeVars[key];
            if (data.IsPointer)
                return data.Container.GetNodeVar(key);
            else
            {
                if (data.Operation != NodeVarOperation.Get && data.Operation != NodeVarOperation.GetSet)
                    throw new Exception($"{GetType().Name}: Attempted to get a non-getttable NodeVar of key \"{key}\".");
                return data.Value;
            }
        }

        public void SetNodeVar(string key, object value)
        {
            var data = NodeVars[key];
            if (data.IsPointer)
                data.Container.SetNodeVar(key, value);
            else
            {
                if (data.Operation != NodeVarOperation.Set && data.Operation != NodeVarOperation.GetSet)
                    throw new Exception($"{GetType().Name}: Attempted to set a non-setttable Nodevar of key \"{key}\".");
                data.Value = value;
            }
        }

        public NodeVarData[] GetNodeVarsList()
        {
            int index = 0;
            NodeVarData[] result = new NodeVarData[_nodeVars.Count];
            foreach (string key in _nodeVars.Keys)
            {
                result[index] = NodeVarData.FromGDDict(_nodeVars.Get<GDC.Dictionary>(key), key);
                index++;
            }
            return result;
        }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(_nodeVars),
                type: Variant.Type.Dictionary,
                hintString: HintString.NodeVars(Mode)
            );
            return builder.Build();
        }
    }
}
