using Godot;

#if TOOLS
namespace Fractural.DependencyInjection
{
    [CSharpScript]
    [Tool]
    public class DependencyInspector : Control
    {
        public override void _Ready()
        {
            var vBox = new VBoxContainer();
            var hSplit = new HSplitContainer();
            hSplit.DraggerVisibility = SplitContainer.DraggerVisibilityEnum.Hidden;

            var label = new Label();
            label.Text = "Class Type";
            label.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            var classTypeSelector = new ClassTypeSelector();
            classTypeSelector.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            hSplit.AddChild(label);
            hSplit.
        }
    }
}
#endif