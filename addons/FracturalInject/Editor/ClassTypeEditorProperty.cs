using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#if TOOLS
namespace Fractural.DependencyInjection
{
    [Tool]
    [CSharpScript]
    public class ClassTypeEditorProperty : EditorProperty
    {
        private PopupSearch _popupSearch;
        private Button _selectButton;
        private Button _clearButton;
        private ClassTypeInspectorPlugin _inspectorPlugin;

        public ClassTypeEditorProperty() { }
        public ClassTypeEditorProperty(ClassTypeInspectorPlugin inspectorPlugin)
        {
            _inspectorPlugin = inspectorPlugin;

            _popupSearch = new PopupSearch();
            _popupSearch.Connect(nameof(PopupSearch.EntrySelected), this, nameof(OnTypeFullNameSelected));

            _selectButton = new Button();
            _selectButton.Connect("pressed", this, nameof(OnSelectButtonPressed));
            _selectButton.ClipText = true;
            _selectButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            _clearButton = new Button();
            _clearButton.Connect("pressed", this, nameof(OnClearButtonPressed));

            var hbox = new HBoxContainer();
            hbox.AddChild(_selectButton);
            hbox.AddChild(_clearButton);

            AddChild(hbox);
            AddChild(_popupSearch);
        }

        public override void _Ready()
        {
            _clearButton.Icon = GetIcon("Clear", "EditorIcons");
        }

        public override void UpdateProperty()
        {
            _selectButton.Text = GetEditedObject().Get<IClassTypeRes>(GetEditedProperty())?.ClassType?.FullName ?? "[empty]";
        }

        private void OnClearButtonPressed()
        {
            EmitChanged(GetEditedProperty(), null);
        }

        private void OnSelectButtonPressed()
        {
            _popupSearch.SearchEntries = _inspectorPlugin.NodeClassTypesDict.Keys.ToArray();
            _popupSearch.Popup_(_selectButton.GetGlobalRect());
        }

        private void OnTypeFullNameSelected(string typeFullName)
        {
            if (_inspectorPlugin.ClassTypeResourcesDict.TryGetValue(typeFullName, out IClassTypeRes selectedResource))
            {
                EmitChanged(GetEditedProperty(), selectedResource);
            }
            else
            {
                // Selected resource doesn't exist yet, so we have to create it.
                _inspectorPlugin.RequestCreateClassTypeResource(_inspectorPlugin.NodeClassTypesDict[typeFullName], (res) =>
                {
                    _selectButton.Text = typeFullName;
                    EmitChanged(GetEditedProperty(), res);
                    // Rebuild the solution to make sure the new ClassTypeRes is built
                    EditorUtils.BuildCSharpSolution();
                });
            }
        }
    }
}
#endif