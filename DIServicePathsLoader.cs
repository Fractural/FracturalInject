using Godot;
using System;
using System.Collections.Generic;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// Loads all the services at servicePath
    /// as services for the DIContainer
    /// on _Ready.
    /// </summary>
    public class DIServicePathsLoader : Node
    {
        public DIContainer DIContainer;

        [Export]
        private List<NodePath> _servicePaths;
        [Export]
        private NodePath _diContainerPath;

        public override void _Ready()
        {
            DIContainer = GetNode<DIContainer>(_diContainerPath);
            foreach (NodePath path in _servicePaths)
                DIContainer.AddDependency(GetNode(path));
        }
    }
}