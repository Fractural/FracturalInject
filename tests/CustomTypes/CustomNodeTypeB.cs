using Godot;

namespace Tests
{
    public class CustomNodeTypeB : Node
    {
        [Export]
        public int NumberFromB { get; set; }
        [Export]
        public string TextFromB { get; set; }
    }
}