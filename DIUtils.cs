using Godot;
using System.Collections.Generic;

namespace Fractural.DependencyInjection
{
    public static class DIUtils
    {
        public static Node GetNodeDependencyHolder(this Node node)
        {
            // We have to manually check children since if the node has not yet
            // been added to the tree, we cannot use GetNode
            foreach (Node child in node.GetChildren())
            {
                if (child.Name == "Dependencies")
                    return child;
            }
            return null;
        }

        public static Dependency[] GetNodeDependencies(this Node node)
        {

            var dependencies = new List<Dependency>();
            var dependenciesHolder = node.GetNodeDependencyHolder();
            if (dependenciesHolder != null)
                foreach (Node child in dependenciesHolder.GetChildren())
                    if (child is Dependency dependency)
                        dependencies.Add(dependency);
            return dependencies.ToArray();
        }
    }
}