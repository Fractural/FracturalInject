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
    public class ClassTypeSelector : EditorProperty
    {
        private PopupSearch _popupSearch;
        private Button _selectButton;
        private ClassTypeInspectorPlugin _inspectorPlugin;

        public ClassTypeSelector() { }
        public ClassTypeSelector(ClassTypeInspectorPlugin inspectorPlugin)
        {
            _inspectorPlugin = inspectorPlugin;
        }

        public override void _Ready()
        {
            // TODO: Create class type file and add it to this
            _popupSearch = new PopupSearch();
            _popupSearch.Connect(nameof(PopupSearch.EntrySelected), this, nameof(OnTypeFullNameSelected));

            _selectButton = new Button();
            _selectButton.Connect("pressed", this, nameof(OnSelectButtonPressed));
            AddChild(_selectButton);
            AddChild(_popupSearch);
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
                GD.Print($"{typeFullName} exists");
                _selectButton.Text = typeFullName;
                EmitChanged(GetEditedProperty(), selectedResource);
            }
            else
            {
                GD.Print($"{typeFullName} doesn't exist");
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

        public override void UpdateProperty()
        {
            _selectButton.Text = GetEditedObject().Get<IClassTypeRes>(GetEditedProperty())?.ClassType?.FullName ?? "-- EMPTY --";
        }
    }
}
#endif