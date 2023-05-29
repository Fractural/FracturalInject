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
        public static T GetDictNodeVar<T>(this INodeVarsContainer container, string key) => (T)container.GetDictNodeVar(key);
    }

    public interface INodeVarsContainer
    {
        /// <summary>
        /// Gets a list of all DictNodeVars for this <see cref="INodeVarsContainer"/>
        /// </summary>
        /// <returns></returns>
        NodeVarData[] GetDictNodeVarsList();
        /// <summary>
        /// Gets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetDictNodeVar(string key);
        /// <summary>
        /// Sets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetDictNodeVar(string key, object value);
    }

    [RegisteredType(nameof(DictNodeVarsContainer), "res://addons/FracturalInject/Assets/dependency-container.svg")]
    [Tool]
    public class DictNodeVarsContainer : Node, INodeVarsContainer
    {
        // Native C# Dictionary is around x9 faster than Godot Dictionary
        private IDictionary<string, NodeVarData> _dictNodeVars;
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

        /// <summary>
        /// Gets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetDictNodeVar<T>(string key) => (T)GetDictNodeVar(key);
        /// <summary>
        /// Gets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetDictNodeVar(string key)
        {
            var data = DictNodeVars[key];
            return data.Value;
        }

        /// <summary>
        /// Sets a NodeVar value at runtime. Does nothing when called from the editor.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetDictNodeVar(string key, object value)
        {
            var data = DictNodeVars[key];
            data.Value = value;
        }

        /// <summary>
        /// Gets a list of all DictNodeVars for this <see cref="INodeVarsContainer"/>
        /// </summary>
        /// <returns></returns>
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

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddDictNodeVarsProp(
                name: nameof(_nodeVars),
                mode: Mode
            );
            return builder.Build();
        }
    }
}
