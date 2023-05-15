using Godot;
using System.Collections.Generic;

namespace Fractural.DependencyInjection
{
    public static class DIUtils
    {
        public static Node GetNodeDependencyHolder(this Node node) => node.GetNode("Dependencies");
        public static Dependency[] GetNodeDependencies(this Node node)
        {
            var dependencies = new List<Dependency>();
            foreach (Node child in node.GetNodeDependencyHolder().GetChildren())
                if (child is Dependency dependency)
                    dependencies.Add(dependency);
            return dependencies.ToArray();
        }
    }
}