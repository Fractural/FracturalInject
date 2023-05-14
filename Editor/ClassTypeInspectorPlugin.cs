using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#if TOOLS
namespace Fractural.DependencyInjection
{
    public class ClassTypeInspectorPlugin : EditorInspectorPlugin, IManagedUnload
    {
        public string ClassTypesDirectory = "res://ClassTypes";
        public Dictionary<string, IClassTypeRes> ClassTypeResourcesDict { get; set; } = new Dictionary<string, IClassTypeRes>();
        public Dictionary<string, Type> NodeClassTypesDict { get; set; } = new Dictionary<string, Type>();

        private ExtendedPlugin _plugin;
        private ConfirmationDialog _confirmCreateClassTypeResourceDialog;
        private Type _currCreateClassTypeResourceType;
        private Action<Resource> _currCreateClassTypeResourceSuccessCallback;

        public ClassTypeInspectorPlugin() { }
        public ClassTypeInspectorPlugin(ExtendedPlugin plugin)
        {
            _plugin = plugin;
            plugin.GetEditorInterface().GetResourceFilesystem().Connect("filesystem_changed", this, nameof(UpdateResources));
            _confirmCreateClassTypeResourceDialog = new ConfirmationDialog();
            _confirmCreateClassTypeResourceDialog.Connect("confirmed", this, nameof(OnConfirmedCreateClassTypeResource));
            plugin.AddManagedControlToContainer(EditorPlugin.CustomControlContainer.Toolbar, _confirmCreateClassTypeResourceDialog);

            UpdateResources();
        }

        public void Unload()
        {
            _plugin.GetEditorInterface().GetResourceFilesystem().Disconnect("filesystem_changed", this, nameof(UpdateResources));
            _confirmCreateClassTypeResourceDialog.QueueFree();
            ClassTypeResourcesDict.Clear();
            NodeClassTypesDict.Clear();
        }

        public override bool CanHandle(Godot.Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (hintText.Split(",").Any(x => x == HintString.ClassType))
            {
                AddPropertyEditor(path, new ClassTypeEditorProperty(this));
                return true;
            }
            return false;
        }

        public void RequestCreateClassTypeResource(Type type, Action<Resource> successCallback = null)
        {
            _currCreateClassTypeResourceSuccessCallback = successCallback;
            _currCreateClassTypeResourceType = type;
            _confirmCreateClassTypeResourceDialog.DialogText = $"\"{type.FullName}\" doesn't have a ClassTypeRes yet. Do you want to make one?";
            _confirmCreateClassTypeResourceDialog.SoloEditorWindowPopup();
        }

        private void OnConfirmedCreateClassTypeResource()
        {
            var resource = CreateClassTypeResource(_currCreateClassTypeResourceType);
            _currCreateClassTypeResourceSuccessCallback?.Invoke(resource);

            _currCreateClassTypeResourceSuccessCallback = null;
            _currCreateClassTypeResourceType = null;
        }

        public Resource CreateClassTypeResource(Type type)
        {
            // We can't have periods for class names and file names, so we replace them with underscores.
            var underscoredFullName = type.FullName.Replace(".", "_");
            var resourceScriptClassName = $"{underscoredFullName}ClassTypeRes";
            var resourceScriptPath = $"{ClassTypesDirectory}/{resourceScriptClassName}.cs";
            var file = new File();
            if (file.FileExists(resourceScriptPath))
            {
                GD.PushWarning($"{nameof(CreateClassTypeResource)}: Failed because resource script path (\"{resourceScriptPath}\") exists.");
                return null;
            }

            var sourceCode = $@"using Fractural.Commons;
using Godot;

namespace Fractural.DependencyInjection
{{
    [Tool]
    public class {resourceScriptClassName} : ClassTypeRes<{type.FullName}> {{ }}
}}";

            file.Open(resourceScriptPath, File.ModeFlags.Write);
            file.StoreString(sourceCode);
            file.Close();

            var resourcePath = $"{ClassTypesDirectory}/{underscoredFullName}.tres";
            var resource = new Resource();
            resource.SetScript(new CSharpScript() { ResourcePath = resourceScriptPath, SourceCode = sourceCode });
            if (file.FileExists(resourcePath))
            {
                GD.PushWarning($"{nameof(CreateClassTypeResource)}: Failed because resource path (\"{resourcePath}\") exists.");
                return null;
            }
            ResourceSaver.Save(resourcePath, resource);
            return resource;
        }

        private void UpdateResources()
        {
            var files = FileUtils.GetDirFiles(ClassTypesDirectory, true, new[] { "tres" });
            ClassTypeResourcesDict.Clear();
            foreach (var filePath in files)
            {
                var resource = ResourceLoader.Load(filePath);
                if (resource is IClassTypeRes res)
                    ClassTypeResourcesDict.Add(res.ClassType.FullName, res);
            }
            NodeClassTypesDict.Clear();
            // LINQ query using reflection to fetch subclasses of Node
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type == typeof(Node) || type.IsSubclassOf(typeof(Node)) && !type.IsAbstract
                select type;
            foreach (var type in subclasses)
                NodeClassTypesDict.Add(type.FullName, type);
        }
    }
}
#endif