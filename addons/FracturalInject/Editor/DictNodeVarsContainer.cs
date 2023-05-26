using Fractural.Commons;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    public static class DictNodeVarsContainerExtensions
    {
        public static T GetDictNodeVar<T>(this IDictNodeVarsContainer container, string key) => (T)container.GetDictNodeVar(key);
    }

    public interface IDictNodeVarsContainer
    {
        NodeVarData[] GetDictNodeVarsList();
        object GetDictNodeVar(string key);
        void SetDictNodeVar(string key, object value);
    }

    [RegisteredType(nameof(DictNodeVarsContainer), "res://addons/FracturalInject/Assets/dependency-container.svg")]
    [Tool]
    public class DictNodeVarsContainer : Node, IDictNodeVarsContainer
    {
        // Native C# Dictionary is around x9 faster than Godot Dictionary
        public IDictionary<string, NodeVarData> DictNodeVars { get; private set; }

        private GDC.Dictionary _nodeVars;

        private HintString.DictNodeVarsMode _mode = HintString.DictNodeVarsMode.LocalAttributes;
        [Export]
        public HintString.DictNodeVarsMode Mode
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
            DictNodeVars = new Dictionary<string, NodeVarData>();
            foreach (string key in _nodeVars)
            {
                var data = NodeVarData.FromGDDict(_nodeVars.Get<GDC.Dictionary>(key), key);
                DictNodeVars[key] = data;

                if (data.IsPointer)
                    data.Container = GetNode(data.ContainerPath);
                else
                    data.Reset();
            }
        }

        public T GetDictNodeVar<T>(string key) => (T)GetDictNodeVar(key);
        public object GetDictNodeVar(string key)
        {
            var data = DictNodeVars[key];
            return data.Value;
        }

        public void SetDictNodeVar(string key, object value)
        {
            var data = DictNodeVars[key];
            data.Value = value;
        }

        public NodeVarData[] GetDictNodeVarsList()
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

        private bool _isFetchingPropertyList = false;
        public override GDC.Array _GetPropertyList()
        {
            if (!_isFetchingPropertyList)
            {
                _isFetchingPropertyList = true;
                GD.Print("_GetPropList: Curr props: ");
                foreach (var prop in GetPropertyList().Cast<GDC.Dictionary>().Select(x => PropertyListItem.FromGDDict(x)))
                {
                    GD.Print($"\t{prop.name} = {Get(prop.name)}");
                }
                _isFetchingPropertyList = false;
            }

            var builder = new PropertyListBuilder();
            builder.AddDictNodeVarsProp(
                name: nameof(_nodeVars),
                mode: Mode
            );
            builder.AddItem(
                name: NodeVarData.NodeVarToPropertyName("lol"),
                type: Variant.Type.String
            );
            builder.AddItem(
                name: NodeVarData.NodeVarToPropertyName("xd"),
                type: Variant.Type.String
            );
            return builder.Build();
        }
    }
}
