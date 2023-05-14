using Godot;
using System;

namespace Fractural.DependencyInjection
{
    public class DependencyWrapper : GDScriptWrapper
    {
        public DependencyWrapper() { }
        public DependencyWrapper(Godot.Object source) : base(source) { }

        public string DependencyName
        {
            get
            {
                return (string)Source.Get("dependency_name");
            }
            set
            {
                Source.Set("dependency_name", value);
            }
        }

        public NodePath DependencyPath
        {
            get
            {
                return (NodePath)Source.Get("dependency_path");
            }
            set
            {
                Source.Set("dependency_path", value);
            }
        }

        public Godot.Object DependencyObject
        {
            get
            {
                return (Godot.Object)Source.Get("dependency");
            }
            set
            {
                Source.Set("dependency", value);
            }
        }
    }
}