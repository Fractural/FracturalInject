using Fractural.Commons;
using Fractural.Utils;
using Godot;
using Godot.Collections;

namespace Fractural.DependencyInjection
{
    [RegisteredType(nameof(Dependency), "addons/FracturalInject/assets/dependency.svg")]
    [Tool]
    public class Dependency : Node
    {
        private Resource _classType;
        private Resource ClassType
        {
            get => _classType;
            set
            {
                _classType = value;
                PropertyListChangedNotify();
            }
        }
        public IClassTypeRes ClassTypeRes => (IClassTypeRes)_classType;
        public NodePath DependencyPath { get; set; }

        // Lazy-load DependencyValue
        private Node _dependencyValue;
        public Node DependencyValue
        {
            get
            {
                if (_dependencyValue == null)
                {
                    // We attempt to fetch the dependency from the saved DependencyPath if _dependencyValue has not already been injected by the DIContainer.
                    var valueNode = GetNode(DependencyPath);
                    if (valueNode == null)
                        GD.PushError($"{nameof(Dependency)}: Dependency at \"GetPath()\" could not get node at path \"{DependencyPath}\"");
                    else if (valueNode is Dependency dependency)
                        _dependencyValue = dependency.DependencyValue;
                    else if (valueNode.GetType() != ClassTypeRes.ClassType)
                        GD.PushError($"{nameof(Dependency)}: Dependency at \"{GetPath()}\" has class type {ClassTypeRes.ClassType.FullName} but actual DependencyPath type was {DependencyValue.GetType()}");
                }
                return _dependencyValue;
            }
            set
            {
                _dependencyValue = value;
            }
        }
        public T GetValueTyped<T>() where T : Node
        {
            if (typeof(T) != ClassTypeRes.ClassType)
            {
                GD.PushError($"{nameof(Dependency)}: Dependency at \"{GetPath()}\" has class type {ClassTypeRes.ClassType.FullName} but user attempted to fetch {typeof(T).FullName}");
                return null;
            }
            return DependencyValue as T;
        }

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(new PropertyListItem(
                name: nameof(ClassType),
                type: Variant.Type.Object,
                hint: PropertyHint.ResourceType,
                hintString: HintString.ClassType
            ));
            builder.AddItem(new PropertyListItem(
                name: nameof(DependencyPath),
                type: Variant.Type.NodePath,
                hint: PropertyHint.None,
                hintString: HintString.DependencyPath,
                usage: ClassType != null ? PropertyUsageFlags.Default : PropertyUsageFlags.Noeditor
            ));
            return builder.Build();
        }

        public override string _GetConfigurationWarning()
        {
            if (ClassType == null)
                return "Class Type must be assigned.";
            return "";
        }
    }
}