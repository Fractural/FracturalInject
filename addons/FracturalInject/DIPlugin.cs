using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
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
            AssetsRegistry = new EditorAssetsRegistry(this);

            AddManagedInspectorPlugin(new ClassTypeInspectorPlugin(this));
            AddManagedInspectorPlugin(new DependencyPathInspectorPlugin(this));
        }
    }
}
#endif