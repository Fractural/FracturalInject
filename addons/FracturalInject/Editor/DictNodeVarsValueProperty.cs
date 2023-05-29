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
        private Dictionary<string, NodeVarData> _fixedNodeVarsDict;
        private Dictionary<string, NodeVarData> _defaultNodeVarsDict;

        private string EditButtonText => $"DictNodeVars [{Value.Count}]";
        private bool HasFixedNodeVars => _fixedNodeVarsDict != null;
        private bool HasDefaultNodeVars => _defaultNodeVarsDict != null;
        private bool _canAddNewVars;
        private IAssetsRegistry _assetsRegistry;

        public DictNodeVarsValueProperty() { }
        public DictNodeVarsValueProperty(
            IAssetsRegistry assetsRegistry,
            Node sceneRoot,
            Node relativeToNode,
            NodeVarData[] localFixedNodeVars = null,
            NodeVarData[] defaultNodeVars = null,
            bool canAddNewVars = true
        ) : base()
        {
            _assetsRegistry = assetsRegistry;
            _sceneRoot = sceneRoot;
            _relativeToNode = relativeToNode;
            if (localFixedNodeVars != null && localFixedNodeVars.Length > 0)
            {
                // fixedNodeVars are NodeVars that should be shown, but not actually saved unless they've changed.
                // This can include
                // - Default node vars values inherited from the PackedScene that the node was an instance of
                // - NodeVar attributes on some of the Node's properties
                _fixedNodeVarsDict = new Dictionary<string, NodeVarData>();
                foreach (var nodeVar in localFixedNodeVars)
                    _fixedNodeVarsDict.Add(nodeVar.Name, nodeVar);
            }
            if (defaultNodeVars != null && defaultNodeVars.Length > 0)
            {
                if (_fixedNodeVarsDict == null)
                    _fixedNodeVarsDict = new Dictionary<string, NodeVarData>();
                _defaultNodeVarsDict = new Dictionary<string, NodeVarData>();
                foreach (var nodeVar in defaultNodeVars)
                {
                    _fixedNodeVarsDict.Add(nodeVar.Name, nodeVar);
                    _defaultNodeVarsDict.Add(nodeVar.Name, nodeVar);
                }
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

        private bool IsValueDefault()
        {
            if (_defaultNodeVarsDict == null)
                return false;
            if (Value.Count != _defaultNodeVarsDict.Count)
                return false;
            foreach (string key in Value.Keys)
            {
                var itemNodeVar = NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(key), key);
                if (_defaultNodeVarsDict.TryGetValue(key, out NodeVarData defaultNodeVar) && !itemNodeVar.Equals(defaultNodeVar))
                    return false;
            }
            return true;
        }

        public override void UpdateProperty()
        {
            if (Value == null || IsValueDefault())
                Value = new GDC.Dictionary();

            var displayedNodeVars = new Dictionary<string, NodeVarData>();
            foreach (string key in Value.Keys)
                displayedNodeVars.Add(key, NodeVarData.FromGDDict(Value.Get<GDC.Dictionary>(key), key));

            if (HasFixedNodeVars)
            {
                // Popupulate Value with any _fixedNodeVars that it is missing
                foreach (var fixedNodeVar in _fixedNodeVarsDict.Values)
                {
                    var displayNodeVar = fixedNodeVar;
                    if (displayedNodeVars.TryGetValue(fixedNodeVar.Name, out NodeVarData existingNodeVar))
                    {
                        if (existingNodeVar.ValueType == fixedNodeVar.ValueType)
                            displayNodeVar = fixedNodeVar.WithChanges(existingNodeVar);
                        else
                            // If the exiting entry's type is different from the fixed entry, then we must purge
                            // the existing entry to ensure the saved entries are always consistent with the fixed entries
                            Value.Remove(existingNodeVar.Name);
                    }
                    displayedNodeVars[fixedNodeVar.Name] = displayNodeVar;
                }
                if (!_canAddNewVars)
                {
                    // If we cannot add new vars, then the Value dict
                    // must only contain fixed node vars
                    foreach (string key in Value.Keys)
                    {
                        if (_fixedNodeVarsDict.ContainsKey(key))
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

            var sortedDisplayNodeVars = new List<NodeVarData>(displayedNodeVars.Values);
            sortedDisplayNodeVars.Sort((a, b) =>
            {
                if (_fixedNodeVarsDict != null)
                {
                    // Sort by whether it's fixed, and then by alphabetical order
                    int fixedOrdering = _fixedNodeVarsDict.ContainsKey(b.Name).CompareTo(_fixedNodeVarsDict.ContainsKey(a.Name));
                    if (fixedOrdering == 0)
                        return a.Name.CompareTo(b.Name);
                    return fixedOrdering;
                }
                return a.Name.CompareTo(b.Name);
            });

            // Move the current focused entry into it's Value dict index inside the entries vBox.
            // We don't want to just overwrite the current focused entry since that would
            // cause the user to retain gui focus on the wrong entry.
            var currFocusedEntry = _currentFocused?.GetAncestor<DictNodeVarsValuePropertyEntry>();
            if (currFocusedEntry != null)
            {
                // Find the new index of the current focused entry within the Value dictionary.
                int keyIndex = sortedDisplayNodeVars.FindIndex(x => x.Name == currFocusedEntry.Data.Name);
                if (keyIndex < 0)
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
            int index = 0;
            int childCount = _keyValueEntriesVBox.GetChildCount();
            foreach (NodeVarData nodeVar in sortedDisplayNodeVars)
            {
                DictNodeVarsValuePropertyEntry entry;
                if (index >= childCount)
                    entry = CreateDefaultEntry();
                else
                    entry = _keyValueEntriesVBox.GetChild<DictNodeVarsValuePropertyEntry>(index);

                if (currFocusedEntry == null || entry != currFocusedEntry)
                    entry.SetData(nodeVar, _fixedNodeVarsDict?.GetValue(nodeVar.Name, null)?.InitialValue);
                if (HasFixedNodeVars)
                {
                    var isFixed = _fixedNodeVarsDict.ContainsKey(nodeVar.Name);
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

        private void OnEntryDataChanged(string key, NodeVarData newValue)
        {
            // Remove entry if it is the same as the default value (no point in storing redundant information)
            if (HasFixedNodeVars && _fixedNodeVarsDict.TryGetValue(key, out NodeVarData existingDefaultValue) && (
                    (existingDefaultValue.InitialValue == null && newValue.InitialValue == null) ||
                    (existingDefaultValue.InitialValue?.Equals(newValue.InitialValue) ?? false)
                ))
                Value.Remove(key);
            else
                Value[key] = newValue.ToGDDict();
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
            _fixedNodeVarsDict = null;
        }

        public void OnAfterDeserialize() { }
    }
}
