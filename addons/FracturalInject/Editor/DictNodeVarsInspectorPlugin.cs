using Godot;
using Fractural.Plugin;
using System;
using Fractural.Utils;

namespace Fractural.DependencyInjection
{
    public class DictNodeVarsInspectorPlugin : EditorInspectorPlugin
    {
        private ExtendedPlugin _plugin;

        public DictNodeVarsInspectorPlugin() { }
        public DictNodeVarsInspectorPlugin(ExtendedPlugin plugin)
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
            if (parser.TryGetArgs(nameof(HintString.DictNodeVars), out string modeString))
            {
                var objectType = node.GetCSharpType();
                NodeVarData[] fixedDictNodeVars = null;
                bool canAddNewVars = false;

                //if (node.Filename != "")
                //{
                //    var scene = ResourceLoader.Load(node.Filename);
                //}

                var mode = (HintString.DictNodeVarsMode)Enum.Parse(typeof(HintString.DictNodeVarsMode), modeString);
                if (mode == HintString.DictNodeVarsMode.Attributes || mode == HintString.DictNodeVarsMode.LocalAttributes)
                    fixedDictNodeVars = DictNodeVarsUtils.GetFixedDictNodeVarTemplates(objectType);
                if (mode == HintString.DictNodeVarsMode.Local || mode == HintString.DictNodeVarsMode.LocalAttributes)
                    canAddNewVars = true;

                AddPropertyEditor(path, new ValueEditorProperty(
                    new DictNodeVarsValueProperty(
                        _plugin.AssetsRegistry,
                        _plugin.GetEditorInterface().GetEditedSceneRoot(),
                        @object as Node,
                        fixedDictNodeVars,
                        canAddNewVars)
                    )
                );
                return true;
            }
            return false;
        }
    }
}
