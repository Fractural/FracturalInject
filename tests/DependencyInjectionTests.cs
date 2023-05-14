using WAT;
using Fractural.DependencyInjection;
using Godot;
using Fractural.Utils;

namespace Tests
{
    public class DependencyInjectionTests : WAT.Test
    {
        /// <summary>
        /// Test a single chain of dependencies (Dependency -> ValueNode)
        /// </summary>
        [Test]
        public void TestSingleChainNodeDependency()
        {
            // TODO: Get test to pass
            Describe("When dependency is readied");
            var scene = ResourceLoader.Load<PackedScene>("res://Tests/TestSingleChainNodeDependency.tscn");
            var instance = scene.Instance();
            AddChild(instance);

            var value = (CustomNodeTypeA)FindNode("Value", owned: false);
            var dependency = (Dependency)FindNode("Dependency", owned: false);

            EditorHackUtils.PrintTree(this);

            Assert.IsNotNull(dependency.DependencyValue, "Then value should not be null");
            Assert.IsEqual(dependency.DependencyValue, value, "Then value should be correct");
        }
    }
}