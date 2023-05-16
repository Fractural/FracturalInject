using WAT;
using Fractural.DependencyInjection;
using Godot;

namespace Tests
{
    public class DependencyContainerTests : WAT.Test
    {
        [Test]
        public void TestAddAndFetchingDependency()
        {
            Describe("When adding a type to a DIContainer");

            var diContainer = new DIContainer();
            AddChild(diContainer);

            var customNode = new CustomNodeTypeA();

            Assert.IsFalse(diContainer.HasBinding<CustomNodeTypeA>(), "Then CustomNodeTypeA dependency should not exist at first.");

            diContainer.Bind<CustomNodeTypeA>().ToSingle(customNode);

            Assert.IsTrue(diContainer.HasBinding<CustomNodeTypeA>(), "Then CustomNodeTypeA dependency should exist after being added.");
            Assert.IsEqual(diContainer.Resolve<CustomNodeTypeA>(), customNode, "Then fetched CustomNodeTypeA dependency should be correct.");

            diContainer.QueueFree();
        }

        [Test]
        public void TestInstantiatePrefabInjection()
        {
            Describe("When instantiate a prefab and resolving it");

            var diContainer = new DIContainer();
            AddChild(diContainer);

            var instanceA = new CustomNodeTypeA();
            var instanceB = new CustomNodeTypeB();

            diContainer.Bind<CustomNodeTypeA>().ToSingle(instanceA);
            diContainer.Bind<CustomNodeTypeB>().ToSingle(instanceB);

            var prefab = ResourceLoader.Load<PackedScene>("res://tests/TestDependencyNodesInjection.tscn");
            var instance = diContainer.InstantiatePrefab<Node>(prefab);
            AddChild(instance);

            Assert.IsEqual(instance.GetNode<Dependency>("Dependencies/InstanceA").DependencyValue, instanceA, "Then instanceA should be injected after resolve.");
            Assert.IsEqual(instance.GetNode<Dependency>("Dependencies/InstanceB").DependencyValue, instanceB, "Then instanceB should be injected after resolve.");

            diContainer.QueueFree();
            instanceA.QueueFree();
            instanceB.QueueFree();
            instance.QueueFree();
        }

        [Test]
        public void TestResolveNodeDependencyNodesInjection()
        {
            Describe("When resolving a prefab instance with Dependency nodes");

            var diContainer = new DIContainer();
            AddChild(diContainer);

            var instanceA = new CustomNodeTypeA();
            var instanceB = new CustomNodeTypeB();

            diContainer.Bind<CustomNodeTypeA>().ToSingle(instanceA);
            diContainer.Bind<CustomNodeTypeB>().ToSingle(instanceB);

            var prefab = ResourceLoader.Load<PackedScene>("res://tests/TestDependencyNodesInjection.tscn");
            var instance = prefab.Instance();
            AddChild(instance);

            Assert.IsNull(instance.GetNode<Dependency>("Dependencies/InstanceA").DependencyValue, "Then instanceA should be null at first.");
            Assert.IsNull(instance.GetNode<Dependency>("Dependencies/InstanceB").DependencyValue, "Then instanceB should be null at first.");

            diContainer.ResolveNode(instance);

            Assert.IsEqual(instance.GetNode<Dependency>("Dependencies/InstanceA").DependencyValue, instanceA, "Then instanceA should be injected after resolve.");
            Assert.IsEqual(instance.GetNode<Dependency>("Dependencies/InstanceB").DependencyValue, instanceB, "Then instanceB should be injected after resolve.");

            diContainer.QueueFree();
            instanceA.QueueFree();
            instanceB.QueueFree();
            instance.QueueFree();
        }

        [Test]
        public void TestResolveObjectIInjectDIContainerInjection()
        {
            Describe("When resolving TestIInjectDIContainer, which resolves instanceA and instanceB using a Construct method");

            var diContainer = new DIContainer();
            AddChild(diContainer);

            var instanceA = new CustomNodeTypeA();
            var instanceB = new CustomNodeTypeB();

            diContainer.Bind<CustomNodeTypeA>().ToSingle(instanceA);
            diContainer.Bind<CustomNodeTypeB>().ToSingle(instanceB);

            AddChild(instanceA);
            AddChild(instanceB);

            var testIInjectDIContainer = new TestIInjectDIContainerClass();

            Assert.IsNull(testIInjectDIContainer.InstanceA, "Then instanceA should be null at first.");
            Assert.IsNull(testIInjectDIContainer.InstanceB, "Then instanceB should be null at first.");

            diContainer.ResolveObject(testIInjectDIContainer);

            Assert.IsEqual(testIInjectDIContainer.InstanceA, instanceA, "Then instanceA should be injected after resolve.");
            Assert.IsEqual(testIInjectDIContainer.InstanceB, instanceB, "Then instanceB should be injected after resolve.");

            diContainer.QueueFree();
            instanceA.QueueFree();
            instanceB.QueueFree();
        }

        [Test]
        public void TestAttributeInjection()
        {
            Describe("When resolving TestAttributeInjectionClass using Inject attribute");

            var diContainer = new DIContainer();
            AddChild(diContainer);

            var instanceA = new CustomNodeTypeA();
            var instanceB = new CustomNodeTypeB();

            diContainer.Bind<CustomNodeTypeA>().ToSingle(instanceA);
            diContainer.Bind<CustomNodeTypeB>().ToSingle(instanceB);

            AddChild(instanceA);
            AddChild(instanceB);

            var testInstance = new TestAttributeInjectionClass();

            Assert.IsNull(testInstance.InstanceAPrivateProp, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPrivateProp)} == null");
            Assert.IsNull(testInstance.InstanceAPublicProp, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPublicProp)} == null");
            Assert.IsNull(testInstance.InstanceAPrivateField, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPrivateField)} == null");
            Assert.IsNull(testInstance.instanceAPublicField, $"Then testInstance.{nameof(TestAttributeInjectionClass.instanceAPublicField)} == null");

            Assert.IsNull(testInstance.InstanceBPrivateProp, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPrivateProp)} == null");
            Assert.IsNull(testInstance.InstanceBPublicProp, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPublicProp)} == null");
            Assert.IsNull(testInstance.InstanceBPrivateField, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPrivateField)} == null");
            Assert.IsNull(testInstance.instanceBPublicField, $"Then testInstance.{nameof(TestAttributeInjectionClass.instanceBPublicField)} == null");

            Assert.IsNull(testInstance.PublicConstructInstanceAValue, $"Then testInstance.{nameof(TestAttributeInjectionClass.PublicConstructInstanceAValue)} == null");
            Assert.IsNull(testInstance.PublicConstructInstanceBValue, $"Then testInstance.{nameof(TestAttributeInjectionClass.PublicConstructInstanceBValue)} == null");
            Assert.IsNull(testInstance.PrivateConstructInstanceAValue, $"Then testInstance.{nameof(TestAttributeInjectionClass.PrivateConstructInstanceAValue)} == null");
            Assert.IsNull(testInstance.PrivateConstructInstanceBValue, $"Then testInstance.{nameof(TestAttributeInjectionClass.PrivateConstructInstanceBValue)} == null");

            diContainer.ResolveObject(testInstance);

            Assert.IsEqual(testInstance.InstanceAPrivateProp, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPrivateProp)} should be injected after resolve.");
            Assert.IsEqual(testInstance.InstanceAPublicProp, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPublicProp)} should be injected after resolve.");
            Assert.IsEqual(testInstance.InstanceAPrivateField, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceAPrivateField)} should be injected after resolve.");
            Assert.IsEqual(testInstance.instanceAPublicField, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.instanceAPublicField)} should be injected after resolve.");

            Assert.IsEqual(testInstance.InstanceBPrivateProp, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPrivateProp)} should be injected after resolve.");
            Assert.IsEqual(testInstance.InstanceBPublicProp, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPublicProp)} should be injected after resolve.");
            Assert.IsEqual(testInstance.InstanceBPrivateField, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.InstanceBPrivateField)} should be injected after resolve.");
            Assert.IsEqual(testInstance.instanceBPublicField, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.instanceBPublicField)} should be injected after resolve.");

            Assert.IsEqual(testInstance.PublicConstructInstanceAValue, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.PublicConstructInstanceAValue)} should be injected after resolve.");
            Assert.IsEqual(testInstance.PublicConstructInstanceBValue, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.PublicConstructInstanceBValue)} should be injected after resolve.");
            Assert.IsEqual(testInstance.PrivateConstructInstanceAValue, instanceA, $"Then testInstance.{nameof(TestAttributeInjectionClass.PrivateConstructInstanceAValue)} should be injected after resolve.");
            Assert.IsEqual(testInstance.PrivateConstructInstanceBValue, instanceB, $"Then testInstance.{nameof(TestAttributeInjectionClass.PrivateConstructInstanceBValue)} should be injected after resolve.");

            diContainer.QueueFree();
            instanceA.QueueFree();
            instanceB.QueueFree();
        }
    }

    [Inject]
    public class TestAttributeInjectionClass
    {
        #region InstanceA Property & Field Injection
        [Inject]
        private CustomNodeTypeA _InstanceAPrivateProp { get; set; }
        public CustomNodeTypeA InstanceAPrivateProp => _InstanceAPrivateProp;
        [Inject]
        public CustomNodeTypeA InstanceAPublicProp { get; set; }

        [Inject]
        private CustomNodeTypeA _instanceAPrivateField;
        public CustomNodeTypeA InstanceAPrivateField => _instanceAPrivateField;
        [Inject]
        public CustomNodeTypeA instanceAPublicField;
        #endregion

        #region InstanceB Property & Field Injection

        [Inject]
        private CustomNodeTypeB _InstanceBPrivateProp { get; set; }
        public CustomNodeTypeB InstanceBPrivateProp => _InstanceBPrivateProp;
        [Inject]
        public CustomNodeTypeB InstanceBPublicProp { get; set; }

        [Inject]
        private CustomNodeTypeB _instanceBPrivateField;
        public CustomNodeTypeB InstanceBPrivateField => _instanceBPrivateField;
        [Inject]
        public CustomNodeTypeB instanceBPublicField;
        #endregion

        #region Method Injection
        public CustomNodeTypeA PublicConstructInstanceAValue { get; set; }
        public CustomNodeTypeB PublicConstructInstanceBValue { get; set; }
        [Inject]
        public void PublicConstruct(CustomNodeTypeA a, CustomNodeTypeB b)
        {
            PublicConstructInstanceAValue = a;
            PublicConstructInstanceBValue = b;
        }

        public CustomNodeTypeA PrivateConstructInstanceAValue { get; set; }
        public CustomNodeTypeB PrivateConstructInstanceBValue { get; set; }
        [Inject]
        private void PrivateConstruct(CustomNodeTypeA a, CustomNodeTypeB b)
        {
            PrivateConstructInstanceAValue = a;
            PrivateConstructInstanceBValue = b;
        }
        #endregion
    }

    public class TestIInjectDIContainerClass : IInjectDIContainer
    {
        public CustomNodeTypeA InstanceA { get; set; }
        public CustomNodeTypeB InstanceB { get; set; }

        public void Construct(DIContainer container)
        {
            InstanceA = container.Resolve<CustomNodeTypeA>();
            InstanceB = container.Resolve<CustomNodeTypeB>();
        }
    }
}