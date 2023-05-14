using Godot;
using System;
using System.Collections.Generic;
using Fractural.Utils;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// Holds a mapping for dependencies.
    /// </summary>
    public class DIContainer : Node
    {
        public static DIContainer Global { get; private set; }

        [Signal]
        public delegate void Readied();

        [Export]
        public bool IsSelfContained { get; set; }
        public Dictionary<Type, object> DependencyMapping { get; set; } = new Dictionary<Type, object>();

        public override void _Ready()
        {
            if (Global != null)
            {
                QueueFree();
                return;
            }
            Global = this;

            if (!IsSelfContained)
            {
                Node root = GetTree().Root;
                GetParent().RemoveChild(this);
                root.AddChild(this);
            }
        }

        public void ResolveDependencyNode(Dependency dependency)
        {
            if (DependencyMapping.TryGetValue(dependency.ClassTypeRes.ClassType, out object value))
                dependency.DependencyValue = (Node)value;
        }

        public void AddDependency<T>(T dependency)
        {
            if (HasDependency<T>())
                RemoveDependency<T>();
            DependencyMapping.Add(typeof(T), dependency);
        }

        public bool HasDependency<T>()
        {
            return DependencyMapping.ContainsKey(typeof(T));
        }

        public void RemoveDependency<T>()
        {
            DependencyMapping.Remove(typeof(T));
        }

        public T GetDependency<T>()
        {
            if (DependencyMapping.TryGetValue(typeof(T), out object result))
                return (T)result;
            return default;
        }
    }
}