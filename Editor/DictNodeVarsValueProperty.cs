using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    /// <summary>
    /// The operation that users of the DictNodeVar can perform on a given DictNodeVar.
    /// </summary>
    public enum NodeVarOperation
    {
        /// <summary>
        /// DictNodeVar can be fetched from the outside
        /// </summary>
        Get,
        /// <summary>
        /// DictNodeVar can be set from the outside
        /// </summary>
        Set,
        /// <summary>
        /// DictNodeVar can get fetched and set from the outside
        /// </summary>
        GetSet
    }

    [Tool]
    public class DictNodeVarsValueProperty : ValueProperty<GDC.Dictionary>, ISerializationListener
    {
        private Button _editButton;
        private Control _container;
        private Button _addElementButton;
        private VBoxContainer _keyValueEntriesVBox;
        private Node _sceneRoot;
        private Node _relativeToNode;
        private Dictionary<string, NodeVarData> _fixedDictNodeVarTemplatesDict;

        private string EditButtonText => $"DictNodeVars [{Value.Count}]";
        private bool HasFixedDictNodeVars => _fixedDictNodeVarTemplatesDict != null;
        private bool _canAddNewVars;
        private IAssetsRegistry _assetsRegistry;

        public DictNodeVarsValueProperty() { }
        public DictNodeVarsValueProperty(
            IAssetsRegistry assetsRegistry,
            Node sceneRoot,
            Node relativeToNode,
            NodeVarData[] fixedDictNodeVarTemplates = null,
            bool canAddNewVars = true
        ) : base()
        {
            _assetsRegistry = assetsRegistry;
            _sceneRoot = sceneRoot;
            _relativeToNode = relativeToNode;
            if (fixedDictNodeVarTemplates != null)
            {
                _fixedDictNodeVarTemplatesDict = new Dictionary<string, NodeVarData>();
                foreach (var fixedDictNodeVar in fixedDictNodeVarTemplates)
                    _fixedDictNodeVarTemplatesDict.Add(fixedDictNodeVar.Name, fixedDictNodeVar);
            }
            _canAddNewVars = canAddNewVars;

            _editButton = new Button();
            _editButton.ToggleMode = true;
            _editButton.ClipText = true;
            _editButton.Connect("toggled", this, nameof(OnEditToggled));
            AddChild(_editButton);

            _addElementButton = new Button();
            _addElementButton.Text = "Add DictNodeVar";
            _addElementButton.Connect("pressed", this, nameof(OnAddElementPressed));
            _addElementButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _addElementButton.RectMinSize = new Vector2(24 * 4, 0);
            _addElementButton.Visible = _canAddNewVars;

            _keyValueEntriesVBox = new VBoxContainer();

            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            vbox.AddChild(_addElementButton);
            vbox.AddChild(_keyValueEntriesVBox);

            _container = vbox;
            AddChild(_container);
        }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            _addElementButton.Icon = GetIcon("Add", "EditorIcons");
            GetViewport().Connect("gui_focus_changed", this, nameof(OnFocusChanged));
        }

        public override void _Process(float delta)
        {
            // We need SetBottomEditor to run here because it won't work in _Ready due to
            // the tree being busy setting up nodes.
            SetBottomEditor(_container);
            SetProcess(false);
        }

        private Control _currentFocused;
        private void OnFocusChanged(Control control) => _currentFocused = control;

        public override void UpdateProperty()
        {
            if (Value == null)
                Value = new GDC.Dictionary();

            if (HasFixedDictNodeVars)
            {
                // Popupulate Value with any _fixedDictNodeVars that it is missing
                foreach (var entry in _fixedDictNodeVarTemplatesDict.Values)
                {
                    if (Value.Contains(entry.Name))
                    {
                        // Try to copy over the value from the existing entry if it exists
                        var existingEntry = NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(entry.Name), entry.Name);
                        if (existingEntry.ValueType == entry.ValueType)
                            entry.InitialValue = existingEntry.InitialValue;
                    }
                    // Value dict does not contain an entry in _fixedDictNodeVars, so we add it to Value dict
                    Value[entry.Name] = entry.ToGDDict();
                }
                if (!_canAddNewVars)
                {
                    // If we cannot add new vars, then the Value dict
                    // must only contain fixed node vars
                    foreach (string key in Value.Keys)
                    {
                        if (_fixedDictNodeVarTemplatesDict.ContainsKey(key))
                            continue;
                        var entry = NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(key), key);
                        // _fixedDictNodeVars doesn't contain an entry in Value dict, so we remove it from Value dict
                        Value.Remove(key);
                    }
                }
            }

            _container.Visible = this.GetMeta<bool>("visible", true);   // Default to being visible if the meta tag doesn't exist.
            _editButton.Pressed = _container.Visible;
            _editButton.Text = EditButtonText;

            int index = 0;
            int childCount = _keyValueEntriesVBox.GetChildCount();

            // Move the current focused entry into it's Value dict index inside the entries vBox.
            // We don't want to just overwrite the current focused entry since that would
            // cause the user to retain gui focus on the wrong entry.
            var currFocusedEntry = _currentFocused?.GetAncestor<DictNodeVarsValuePropertyEntry>();
            if (currFocusedEntry != null)
            {
                // Find the new index of the current focused entry within the Value dictionary.
                int keyIndex = 0;
                foreach (var key in Value.Keys)
                {
                    if (key != null && key.Equals(currFocusedEntry.Data.Name))
                        break;
                    keyIndex++;
                }
                if (keyIndex == Value.Keys.Count)
                {
                    // Set current focused entry back to null. We couldn't
                    // find the current focused entry in the new dictionary, meaning
                    // this entry must have been deleted, therefore we don't care about it
                    // anymore.
                    currFocusedEntry = null;
                }
                else
                {
                    // Swap the entry that's currently in the focused entry's place with the focused entry.
                    var targetEntry = _keyValueEntriesVBox.GetChild<DictNodeVarsValuePropertyEntry>(keyIndex);
                    _keyValueEntriesVBox.SwapChildren(targetEntry, currFocusedEntry);
                }
            }

            // Set the data of each entry with the corresponding values from the Value dictionary
            foreach (string key in Value.Keys)
            {
                DictNodeVarsValuePropertyEntry entry;
                if (index >= childCount)
                    entry = CreateDefaultEntry();
                else
                    entry = _keyValueEntriesVBox.GetChild<DictNodeVarsValuePropertyEntry>(index);

                if (currFocusedEntry == null || entry != currFocusedEntry)
                    entry.SetData(NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(key), key));
                if (HasFixedDictNodeVars)
                {
                    var isFixed = _fixedDictNodeVarTemplatesDict.ContainsKey(key);
                    entry.NameEditable = !isFixed;
                    entry.ValueTypeEditable = !isFixed;
                    entry.OperationEditable = !isFixed;
                    entry.Deletable = !isFixed;
                }
                index++;
            }

            // Free extra entries
            if (index < childCount)
            {
                for (int i = childCount - 1; i >= index; i--)
                {
                    var entry = _keyValueEntriesVBox.GetChild<DictNodeVarsValuePropertyEntry>(i);
                    entry.NameChanged -= OnEntryNameChanged;
                    entry.DataChanged -= OnEntryDataChanged;
                    entry.QueueFree();
                }
            }

            if (!IsInstanceValid(currFocusedEntry))
                currFocusedEntry = null;

            var nextKey = GetNextVarName();
            _addElementButton.Disabled = !_canAddNewVars || (Value?.Contains(nextKey) ?? false);
        }

        private new ValueProperty CreateValueProperty(Type type)
        {
            var property = ValueProperty.CreateValueProperty(type);
            if (type == typeof(NodePath) && property is NodePathValueProperty valueProperty)
            {
                valueProperty.SelectRootNode = _sceneRoot;
                valueProperty.RelativeToNode = _relativeToNode;
            }
            return property;
        }

        private DictNodeVarsValuePropertyEntry CreateDefaultEntry()
        {
            var entry = new DictNodeVarsValuePropertyEntry(_assetsRegistry, _sceneRoot, _relativeToNode);
            entry.NameChanged += OnEntryNameChanged;
            entry.DataChanged += OnEntryDataChanged;
            entry.Deleted += OnEntryDeleted;
            // Add entry if we ran out of existing ones
            _keyValueEntriesVBox.AddChild(entry);
            return entry;
        }

        private void OnEntryNameChanged(string oldKey, DictNodeVarsValuePropertyEntry entry)
        {
            var newKey = entry.Data.Name;
            if (Value.Contains(newKey))
            {
                // Revert CurrentKey back
                entry.Data.Name = oldKey;
                // Reject change since the newKey already exists
                entry.NameProperty.SetValue(oldKey);
                return;
            }
            var currValue = Value[oldKey];
            Value.Remove(oldKey);
            Value[newKey] = currValue;
            InvokeValueChanged(Value);
        }

        private void OnEntryDataChanged(object key, object newValue)
        {
            GD.Print("data changed, invoking value changed");
            Value[key] = newValue;
            InvokeValueChanged(Value);
        }

        private void OnEntryDeleted(object key)
        {
            Value.Remove(key);
            InvokeValueChanged(Value);
        }

        private void OnAddElementPressed()
        {
            // The adding is done in UpdateProperty
            // Note the edited a field in Value doesn't invoke ValueChanged, so we must do it manually
            //
            // Use default types for the newly added element
            var nextKey = GetNextVarName();
            Value[nextKey] = new NodeVarData()
            {
                Name = nextKey,
                ValueType = typeof(int),
                InitialValue = DefaultValueUtils.GetDefault<int>()
            }.ToGDDict();
            InvokeValueChanged(Value);
        }

        private void OnEditToggled(bool toggled)
        {
            SetMeta("visible", toggled);
            _container.Visible = toggled;
        }

        private string GetNextVarName()
        {
            var previousValues = Value?.Keys.Cast<string>();
            uint highestNumber = 0;
            if (previousValues != null)
            {
                foreach (var value in previousValues)
                    if (uint.TryParse(value.TrimPrefix("Var"), out uint intValue) && intValue > highestNumber)
                        highestNumber = intValue;
                highestNumber++;
            }
            return "Var" + highestNumber.ToString();
        }

        public void OnBeforeSerialize()
        {
            _fixedDictNodeVarTemplatesDict = null;
        }

        public void OnAfterDeserialize() { }
    }
}
