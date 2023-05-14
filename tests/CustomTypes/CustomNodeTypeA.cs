using Godot;

namespace Tests
{
    public class CustomNodeTypeA : Node
    {
        [Export]
        public int Number { get; set; }
        [Export]
        public string SomeText { get; set; }
    }
}