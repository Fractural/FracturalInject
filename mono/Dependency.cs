using Fractural.CSharpResourceRegistry;
using Godot;

namespace Fractural.DependencyInjection
{
	[RegisteredType(nameof(Dependency), "addons/FracturalInject/assets/dependency.svg")]
	[Tool]
	public class Dependency : Node
	{
		[Export]
		private Resource classTypeRes;
		public IClassTypeRes ClassTypeRes => (IClassTypeRes)classTypeRes;
		[Export]
		public NodePath DependencyPath { get; set; }

		public override void _Ready()
		{
			GD.Print("DEPENDENCY READY");
			GD.Print(ClassTypeRes.DependencyType.FullName);
		}
	}
}