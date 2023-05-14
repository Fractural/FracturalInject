using Godot;

#if TOOLS
namespace Fractural.DependencyInjection
{
    public class DependencyInspectorPlugin : EditorInspectorPlugin
    {
        public override bool CanHandle(Godot.Object @object)
        {
            return @object is Dependency;
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (path == nameof(Dependency.DependencyPath))
                AddCustomControl(new DependencyInspector());
            return false;
        }
    }
}
#endif