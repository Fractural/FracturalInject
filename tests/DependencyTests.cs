using WAT;
using Fractural.DependencyInjection;
using Godot;

namespace Tests
{
    public class DependencyTests : WAT.Test
    {
        /// <summary>
        /// Test a single chain of dependencies (Dependency -> ValueNode)
        /// </summary>
        [Test]
        public void TestSingleChainNodeDependency()
        {
            Describe("When fetching single chain dependency");
            var scene = ResourceLoader.Load<PackedScene>("res://tests/TestSingleChainNodeDependency.tscn");
            var instance = scene.Instance();
            AddChild(instance);

            var value = (CustomNodeTypeA)FindNode("Value", owned: false);
            var dependency = (Dependency)FindNode("Dependency", owned: false);

            Assert.IsNotNull(dependency.DependencyValue, "Then value should not be null");
            Assert.IsEqual(dependency.DependencyValue, value, "Then value should be correct");

            instance.QueueFree();
        }

        /// <summary>
        /// Test a double chain of dependencies (Dependency -> Dependency -> ValueNode)
        /// </summary>
        [Test]
        public void TestDoubleChainNodeDependency()
        {
            Describe("When fetching double chain dependency");
            var scene = ResourceLoader.Load<PackedScene>("res://tests/TestDoubleChainNodeDependency.tscn");
            var instance = scene.Instance();
            AddChild(instance);

            var value = (CustomNodeTypeA)FindNode("Value", owned: false);
            var dependency = (Dependency)FindNode("Dependency", owned: false);

            Assert.IsNotNull(dependency.DependencyValue, "Then value should not be null");
            Assert.IsEqual(dependency.DependencyValue, value, "Then value should be correct");

            instance.QueueFree();
        }

        /// <summary>
        /// Test a cyclical dependency (Dependency -> Dependency2, Dependency2 -> Dependency)
        /// </summary>
        [Test]
        public void TestCyclicalNodeDependency()
        {
            Describe("When fetching cyclical dependencies");

            var scene = ResourceLoader.Load<PackedScene>("res://tests/TestCyclicalNodeDependency.tscn");
            var instance = scene.Instance();
            AddChild(instance);

            var dependency = (Dependency)FindNode("Dependency", owned: false);

            Assert.IsNull(dependency.DependencyValue, "Then value should be null since cyclical dependencies are not allowed");

            instance.QueueFree();
        }
    }
}