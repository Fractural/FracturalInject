using Godot;
using System.Collections.Generic;
using Fractural.Utils;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// A lookup table for the default values of PackedScenes. 
    /// It gets the default values by instancing the PackedScene, and then caches the values for future lookups.
    /// </summary>
    [Tool]
    public class PackedSceneDefaultValuesRegistry : Node
    {
        public PackedScene[] PackedScenes { get; set; }
        public string[] DirectoryBlacklist { get; set; }
        private bool _useFilesystemScan = true;
        /// <summary>
        /// Should the file system be scanned on <see cref="Reload"/>?
        /// </summary>
        [Export]
        public bool UseFilesystemScan
        {
            get => _useFilesystemScan;
            set
            {
                _useFilesystemScan = value;
                PropertyListChangedNotify();
            }
        }
        /// <summary>
        /// Should the lookup table be regenerated on ready?
        /// </summary>
        [Export]
        public bool ReloadOnReady { get; set; } = true;
        [Export]
        public bool UseCache { get; set; } = true;
        public Dictionary<string, Dictionary<string, object>> PackedSceneToDefaultValuesDict { get; private set; }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            if (ReloadOnReady)
                Reload();
        }

        public T GetDefaultValue<T>(Node node, string key) => (T)GetDefaultValue(node, key);
        public object GetDefaultValue(Node node, string key)
        {
            var defaultValues = GetDefaultValues(node);
            if (defaultValues == null)
                return null;
            return defaultValues.GetValue(key, null);
        }

        public T GetDefaultValue<T>(PackedScene packedScene, string key) => (T)GetDefaultValue(packedScene, key);
        public object GetDefaultValue(PackedScene packedScene, string key)
        {
            var defaultValues = GetDefaultValues(packedScene);
            if (defaultValues == null)
                return null;
            return defaultValues.GetValue(key, null);
        }

        public T GetDefaultValue<T>(string packedScenePath, string key) => (T)GetDefaultValue(packedScenePath, key);
        public object GetDefaultValue(string packedScenePath, string key)
        {
            var defaultValues = GetDefaultValues(packedScenePath);
            if (defaultValues == null)
                return null;
            return defaultValues.GetValue(key, null);
        }

        /// <summary>
        /// Gets the default values of a node that was instanced from a PackedScene.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDefaultValues(Node node)
        {
            if (node.Filename == "")
                return null;
            return GetDefaultValues(node.Filename);
        }

        /// <summary>
        /// Gets the default values of a PackedScene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDefaultValues(PackedScene scene)
        {
            if (scene.ResourcePath == "")
                return null;
            return GetDefaultValues(scene.ResourcePath);
        }

        /// <summary>
        /// Gets the default values of a PackedScene from it's filepath.
        /// </summary>
        /// <param name="packedScenePath"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDefaultValues(string packedScenePath)
        {
            if (!UseCache)
                return FetchDefaultValues(ResourceLoader.Load<PackedScene>(packedScenePath, null, true));
            if (!PackedSceneToDefaultValuesDict.ContainsKey(packedScenePath))
                AddDefaultValues(ResourceLoader.Load<PackedScene>(packedScenePath));
            return PackedSceneToDefaultValuesDict.GetValue(packedScenePath, null);
        }

        /// <summary>
        /// Reloads the lookup table, rescaning the filesystem if <see cref="UseFilesystemScan"/> is on.
        /// </summary>
        public void Reload()
        {
            if (!UseCache)
                return;

            PackedSceneToDefaultValuesDict = new Dictionary<string, Dictionary<string, object>>();
            if (UseFilesystemScan)
                PackedScenes = FileUtils.GetDirFiles("res://", true, new string[] { "tscn" }).Select(path => ResourceLoader.Load<PackedScene>(path)).ToArray();

            foreach (var scene in PackedScenes)
                AddDefaultValues(scene);
        }

        private void AddDefaultValues(PackedScene scene)
        {
            PackedSceneToDefaultValuesDict.Add(scene.ResourcePath, FetchDefaultValues(scene));
        }

        /// <summary>
        /// Fetches the default values by loading the from 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="packedScenePath"></param>
        /// <returns></returns>
        private Dictionary<string, object> FetchDefaultValues(PackedScene scene)
        {
            var instance = scene.Instance();
            var instancePropertyList = instance.GetPropertyList();
            var sceneDefaultValues = new Dictionary<string, object>();
            foreach (GDC.Dictionary propertyDict in instancePropertyList)
            {
                var propertyItem = PropertyListItem.FromGDDict(propertyDict);
                if ((propertyItem.usage & PropertyUsageFlags.Storage) != 0 && !sceneDefaultValues.ContainsKey(propertyItem.name))
                    sceneDefaultValues.Add(propertyItem.name, instance.Get(propertyItem.name));
            }
            return sceneDefaultValues;
        }

        public override GDC.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(PackedScenes),
                type: Variant.Type.Array,
                hint: PropertyHint.ResourceType,
                usage: _useFilesystemScan ? PropertyUsageFlags.Storage : PropertyUsageFlags.Default
            );
            builder.AddItem(
                name: nameof(DirectoryBlacklist),
                type: Variant.Type.StringArray,
                usage: _useFilesystemScan ? PropertyUsageFlags.Default : PropertyUsageFlags.Storage
            );
            return builder.Build();
        }
    }
}