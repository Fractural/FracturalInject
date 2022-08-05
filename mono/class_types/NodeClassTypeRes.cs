using Fractural.CSharpResourceRegistry;
using Godot;

namespace Fractural
{
	[RegisteredType(nameof(NodeClassTypeRes))]
	public class NodeClassTypeRes : ClassTypeRes<Node> { }
}