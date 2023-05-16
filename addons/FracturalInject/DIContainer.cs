using Godot;
using System;
using System.Collections.Generic;
using Fractural.Utils;
using Fractural.Commons;
using System.Linq;
using System.Reflection;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// Interface for manually resolving dependencies inside Construct function using the container.
    /// </summary>
    public interface IInjectDIContainer
    {
        void Construct(DIContainer container);
    }

    /// <summary>
    /// Attribute to mark a class as needing injections, and to mark fields, properties, and methods as injection points
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Class, AllowMultiple = false)]
    public class InjectAttribute : System.Attribute
    {
        public InjectAttribute() { }
    }

    /// <summary>
    /// Holds a mapping for dependencies.
    /// </summary>
    [RegisteredType(nameof(DIContainer), "res://addons/FracturalInject/Assets/dependency-container.svg")]
    public class DIContainer : Node
    {
        public class BindingBuilder
        {
            private DIContainer _container;
            private Type _fromType;

            public BindingBuilder(Type fromType, DIContainer container)
            {
                _fromType = fromType;
                _container = container;
            }

            public void To(Type toType)
            {
                if (toType != _fromType && !toType.IsSubclassOf(_fromType))
                    throw new Exception("toType must be a sub class of the fromType in a binding!");
                _container.DependencyMapping.Add(_fromType, toType);
            }

            public void ToSingle(object instance)
            {
                var toType = instance.GetType();
                if (toType != _fromType && !toType.IsSubclassOf(_fromType))
                    throw new Exception("toType must be a sub class of the fromType in a binding!");
                _container.DependencyMapping.Add(_fromType, instance);
            }
        }

        public class BindingBuilder<T>
        {
            private DIContainer _container;

            public BindingBuilder(DIContainer container)
            {
                _container = container;
            }

            public void To<TInherited>() where TInherited : T
            {
                // NOTE: Cyclical dependencies are not possible since TInherited must be either T or a subclass of T -- it can never be a parent class of T.
                _container.DependencyMapping.Add(typeof(T), typeof(TInherited));
            }

            public void ToSingle<TInherited>(TInherited instance) where TInherited : T
            {
                _container.DependencyMapping.Add(typeof(T), instance);
            }
        }

        public static DIContainer Global { get; private set; }

        [Signal]
        public delegate void Readied();

        [Export]
        public bool IsSelfContained { get; set; }
        public Dictionary<Type, object> DependencyMapping { get; set; } = new Dictionary<Type, object>();
        public DIContainer ParentContainer { get; set; }

        public override async void _Ready()
        {
            if (Global != null)
            {
                QueueFree();
                return;
            }
            Global = this;

            // Add DIContainer itself as dependency so it can be injected
            Bind<DIContainer>().ToSingle(this);

            if (!IsSelfContained)
            {
                await ToSignal(GetTree(), "idle_frame");

                Node root = GetTree().Root;
                this.Reparent(root);
            }
        }

        public T InstantiatePrefab<T>(PackedScene prefab) where T : Node
        {
            var instance = prefab.Instance();
            ResolveNode(instance);
            return (T)instance;
        }

        public void ResolveNode(Node prefabInstance)
        {
            // Try resolve Dependency nodes in the prefab
            foreach (var dependency in prefabInstance.GetNodeDependencies())
                ResolveDependencyNode(dependency);

            ResolveObject(prefabInstance);
        }

        public void ResolveObject(object instance)
        {
            if (instance is IInjectDIContainer injectable)
                // Try using interface injection (The node will service locate dependencies using the injected DIContainer.)
                injectable.Construct(this);
            else if (instance.GetType().IsDefined(typeof(InjectAttribute), true))
            {
                // Try reflection dependency injection on the node itself
                ResolveObjectWithFlags(instance, BindingFlags.Public | BindingFlags.Instance);
                ResolveObjectWithFlags(instance, BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        private void ResolveObjectWithFlags(object instance, BindingFlags flags)
        {
            foreach (var field in instance.GetType().GetFields(flags))
                if (field.IsDefined(typeof(InjectAttribute), true))
                {
                    var injectedType = field.FieldType;
                    field.SetValue(instance, Resolve(injectedType));
                }
            foreach (var property in instance.GetType().GetProperties(flags))
                if (property.IsDefined(typeof(InjectAttribute), true))
                {
                    var injectedType = property.PropertyType;
                    property.SetValue(instance, Resolve(injectedType));
                }
            foreach (var method in instance.GetType().GetMethods(flags))
                if (method.IsDefined(typeof(InjectAttribute), true))
                {
                    var injectedMethodParameters = method.GetBaseDefinition().GetParameters().Select(x => Resolve(x.ParameterType)).ToArray();
                    method.Invoke(instance, injectedMethodParameters);
                }
        }

        public void ResolveDependencyNode(Dependency dependency)
        {
            dependency.DependencyValue = Resolve(dependency.ClassTypeRes.ClassType);
            GD.Print($"resolving class type for node: {dependency.ClassTypeRes.ClassType} result: {dependency.DependencyValue}");
        }

        public BindingBuilder Bind(Type type)
        {
            if (HasBinding(type))
                throw new Exception("Binding already exists! Use Rebind to change the binding.");
            return new BindingBuilder(type, this);
        }

        public BindingBuilder<T> Bind<T>()
        {
            if (HasBinding<T>())
                throw new Exception("Binding already exists! Use Rebind to change the binding.");
            return new BindingBuilder<T>(this);
        }

        public BindingBuilder<T> Rebind<T>() => new BindingBuilder<T>(this);

        public bool HasBinding(Type type) => DependencyMapping.ContainsKey(type);
        public bool HasBinding<T>() => DependencyMapping.ContainsKey(typeof(T));

        public void Unbind<T>()
        {
            DependencyMapping.Remove(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (DependencyMapping.TryGetValue(type, out object result))
            {
                if (result is Type resultType)
                    return Resolve(resultType);
                return result;
            }
            else if (ParentContainer != null)
                return ParentContainer.Resolve(type);
            return null;
        }

        public T Resolve<T>()
        {
            if (DependencyMapping.TryGetValue(typeof(T), out object result))
                return (T)result;
            return default;
        }

        public DIContainer GetSubDIContainer()
        {
            var newContainer = new DIContainer();
            newContainer.ParentContainer = this;
            return newContainer;
        }
    }
}