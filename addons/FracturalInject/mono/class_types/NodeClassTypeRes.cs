using Godot;
using MonoCustomResourceRegistry;
using System;

namespace Fractural
{
	[RegisteredType(nameof(NodeClassTypeRes))]
	public class NodeClassTypeRes : ClassTypeRes<Node> { }
}