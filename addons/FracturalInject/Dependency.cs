using Fractural.Commons;
using Godot;

namespace Fractural.DependencyInjection
{
    [RegisteredType(nameof(Dependency), "addons/FracturalInject/assets/dependency.svg")]
    [Tool]
    public class Dependency : Node
    {
        [Export]
        private Resource _classTypeRes;
        public IClassTypeRes ClassTypeRes => (IClassTypeRes)_classTypeRes;
        [Export]
        public NodePath DependencyPath { get; set; }
        public Node DependencyValue => GetNode(DependencyPath);

        public override void _Ready()
        {
            GD.Print("DEPENDENCY READY");
            GD.Print(ClassTypeRes.ClassType.FullName);
        }
    }
}