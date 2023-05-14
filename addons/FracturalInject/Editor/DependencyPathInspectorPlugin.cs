using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System.Linq;

#if TOOLS
namespace Fractural.DependencyInjection
{
    public class DependencyPathInspectorPlugin : EditorInspectorPlugin
    {
        private EditorPlugin _plugin;

        public DependencyPathInspectorPlugin() { }
        public DependencyPathInspectorPlugin(EditorPlugin plugin)
        {
            _plugin = plugin;
        }

        public override bool CanHandle(Godot.Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (hintText.Split(",").Any(x => x == HintString.DependencyPath))
            {
                AddPropertyEditor(path, new NodePathSelectEditorProperty(
                    _plugin.GetEditorInterface().GetEditedSceneRoot(),
                    (node, editedObject) =>
                    {
                        var editedDependency = editedObject as Dependency;
                        if (node is Dependency dependency && dependency.ClassTypeRes != null)
                            return editedDependency.ClassTypeRes == dependency.ClassTypeRes;
                        if (node.GetCSharpType() == editedDependency.ClassTypeRes.ClassType)
                            return true;
                        return false;
                    }
                ));
                return true;
            }
            return false;
        }

    }
}
#endif