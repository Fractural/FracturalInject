using Godot;
using Fractural.Plugin;
using System;
using System.Collections.Generic;
using Fractural.Utils;
using System.Linq;

namespace Fractural.DependencyInjection
{
    public class NodeVarsInspectorPlugin : EditorInspectorPlugin
    {
        private ExtendedPlugin _plugin;

        public NodeVarsInspectorPlugin() { }
        public NodeVarsInspectorPlugin(ExtendedPlugin plugin)
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
                NodeVarData[] fixedNodeVars = null;
                bool canAddNewVars = false;

                //if (node.Filename != "")
                //{
                //    var scene = ResourceLoader.Load(node.Filename);
                //}

                var mode = (HintString.NodeVarsMode)Enum.Parse(typeof(HintString.NodeVarsMode), modeString);
                if (mode == HintString.NodeVarsMode.Attributes || mode == HintString.NodeVarsMode.LocalAttributes)
                    fixedNodeVars = NodeVarsUtils.GetFixedNodeVarTemplates(objectType);
                if (mode == HintString.NodeVarsMode.Local || mode == HintString.NodeVarsMode.LocalAttributes)
                    canAddNewVars = true;

                AddPropertyEditor(path, new ValueEditorProperty(
                    new NodeVarsValueProperty(
                        _plugin.AssetsRegistry,
                        _plugin.GetEditorInterface().GetEditedSceneRoot(),
                        @object as Node,
                        fixedNodeVars,
                        canAddNewVars)
                    )
                );
                return true;
            }
            return false;
        }
    }
}
