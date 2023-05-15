using Godot;
using System;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// Loads it's children as services for the DIContainer.
    /// </summary>
    public class DIServiceChildrenLoader : Node
    {
        public DIContainer DIContainer;

        [Export]
        private NodePath _diContainerPath;

        public override void _Ready()
        {
            DIContainer = GetNode<DIContainer>(_diContainerPath);
            foreach (Node node in GetChildren())
                DIContainer.Bind(node.GetType()).ToSingle(node);
        }
    }
}