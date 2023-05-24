using Godot;
using Fractural.Plugin;
using System;
using System.Reflection;
using System.Collections.Generic;
using Fractural.Utils;
using System.Linq;

namespace Fractural.DependencyInjection
{
    public class NodeVarsInspectorPlugin : EditorInspectorPlugin
    {
        private EditorPlugin _plugin;

        public NodeVarsInspectorPlugin() { }
        public NodeVarsInspectorPlugin(EditorPlugin plugin)
        {
            _plugin = plugin;
        }

        public override bool CanHandle(Godot.Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (!(@object is Node node)) return false;
            var parser = new HintArgsParser(hintText);
            if (parser.TryGetArgs(nameof(HintString.NodeVars), out string modeString))
            {
                var objectType = node.GetCSharpType();
                // User can add NodeVars to StateGraphs themselves.
                List<NodeVarData> fixedNodeVars = null;
                bool canAddNewVars = false;

                var mode = (HintString.NodeVarsMode)Enum.Parse(typeof(HintString.NodeVarsMode), modeString);
                if (mode == HintString.NodeVarsMode.Attributes || mode == HintString.NodeVarsMode.LocalAttributes)
                {
                    // Use NodeVar attributes attached to properties on the State's C# script
                    // to determine what NodeVars are exposed
                    //
                    // User cannot edit the NodeVars of a IState, since it's determined
                    // by the State's script.
                    fixedNodeVars = new List<NodeVarData>();
                    foreach (var property in objectType.GetProperties())
                    {
                        var attribute = property.GetCustomAttribute<NodeVarAttribute>();
                        if (attribute == null)
                            continue;
                        fixedNodeVars.Add(new NodeVarData()
                        {
                            Name = property.Name,
                            ValueType = property.PropertyType,
                            Operation = attribute.Operation,
                        });
                    }
                }

                if (mode == HintString.NodeVarsMode.Local || mode == HintString.NodeVarsMode.LocalAttributes)
                    canAddNewVars = true;

                AddPropertyEditor(path, new ValueEditorProperty(new NodeVarsValueProperty(_plugin.GetEditorInterface().GetEditedSceneRoot(), @object as Node, fixedNodeVars?.ToArray(), canAddNewVars)));
                return true;
            }
            return false;
        }
    }
}
