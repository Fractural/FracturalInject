using Fractural.Plugin;
using Godot;

#if TOOLS
namespace Fractural.DependencyInjection
{
    [Tool]
    public class DIPlugin : ExtendedPlugin
    {
        public override string PluginName => "Fractural Inject";

        protected override void Load()
        {
            AddManagedInspectorPlugin(new ClassTypeInspectorPlugin(this));
            AddManagedInspectorPlugin(new DependencyInspectorPlugin());
        }
    }
}
#endif