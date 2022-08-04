using Godot;
using System;
using System.Collections.Generic;
using Fractural.Utils;

namespace Fractural.DependencyInjection
{
	public class DIContainer : Node
	{
		public static DIContainer Instance { get; private set; }

		[Signal]
		public delegate void Readied();
		
		[Export]
		public bool IsSelfContained { get; set; }
		public List<Godot.Object> GDScriptServices;
		public List<object> CSharpServices;

		[Export]
		private NodePath dependenciesHolderPath;

		public override void _Ready()
		{
			if (Instance != null)
			{
				QueueFree();
				return;
			}
			Instance = this;

			if (!IsSelfContained)
			{
				Node root = GetTree().Root;
				GetParent().RemoveChild(this);
				root.AddChild(this);
			}
		}

		public void AddDependency(Godot.Object dependency)
		{
			// Supports both GDScript and CSharp wrappers.
			if (dependency.GetScript() is GDScript)
			{
				GDScriptServices.Add(dependency);
				if (GDScriptUtils.IsType(dependency, "CSharpWrapper"))
					CSharpServices.Add(dependency.Get("source"));
			} else if (dependency.GetScript() is CSharpScript)
			{
				CSharpServices.Add(dependency);
				if (dependency is IGDScriptWrapper)
					GDScriptServices.Add((dependency as IGDScriptWrapper).Source);
			}
		}

		public bool HasDependency(Godot.Object otherDependency)
		{
			foreach (Godot.Object dependency in GDScriptServices)
				if (dependency.GetType() == otherDependency.GetType())
					return true;
			return false;
		}

		private void OnSceneLoaded(Node loadedScene)
		{
			if (!loadedScene.HasNode("Dependencies"))
				return;
			
			var dependenciesHolder = loadedScene.GetNode("Dependencies");
			var dependencyRequesters = dependenciesHolder.GetChildren();

			foreach (Node requester in dependencyRequesters)
				TryInjectDependencies(new DependencyWrapper(requester));
		}

		private void TryInjectDependencies(DependencyWrapper requester)
		{
			foreach (Godot.Object injectableDependency in GDScriptServices)
				if (GDScriptUtils.IsType(injectableDependency, requester.DependencyName))
				{
					requester.DependencyObject = injectableDependency;
					return;
				}
			foreach (object injectableDependency in CSharpServices)
				if (injectableDependency.GetType().FullName == requester.DependencyName)
				{
					requester.CSharpDependencyObject = injectableDependency;
					return;
				}
		}
	}

	public interface IGDScriptWrapper
	{
		Godot.Object Source { get; }
	}
}