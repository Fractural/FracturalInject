using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Fractural.Utils;
using Godot;
using System;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    [Tool]
    public class DictNodeVarsValuePropertyEntry : HBoxContainer
    {
        private class ValueTypeData
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public Texture Icon { get; set; }
            public int Index { get; set; }
            public bool UseIconOnly { get; set; }
        }

        private class OperationTypeData
        {
            public string Name { get; set; }
            public NodeVarOperation Operation { get; set; }
            public int Index { get; set; }
        }

        /// <summary>
        /// NameChanged(string oldName, Entry entry)
        /// </summary>
        public event Action<string, DictNodeVarsValuePropertyEntry> NameChanged;
        /// <summary>
        /// DataChanged(string name, NodeVarData newValue)
        /// </summary>
        public event Action<string, NodeVarData> DataChanged;
        /// <summary>
        /// Deleted(string name)
        /// </summary>
        public event Action<string> Deleted;

        private bool _disabled = false;
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                if (IsInsideTree())
                    UpdateDisabledUI();
            }
        }
        private bool _nameEditable = true;
        public bool NameEditable
        {
            get => _nameEditable;
            set
            {
                _nameEditable = value;
                if (IsInsideTree())
                    UpdateDisabledUI();
            }
        }
        private bool _valueTypeEditable = true;
        public bool ValueTypeEditable
        {
            get => _valueTypeEditable;
            set
            {
                _valueTypeEditable = value;
                if (IsInsideTree())
                    UpdateDisabledUI();
            }
        }
        private bool _operationEditable = true;
        public bool OperationEditable
        {
            get => _operationEditable;
            set
            {
                _operationEditable = value;
                if (IsInsideTree())
                    UpdateDisabledUI();
            }
        }
        private bool _deletable = true;
        public bool Deletable
        {
            get => _deletable;
            set
            {
                _deletable = value;
                if (IsInsideTree())
                    UpdateDisabledUI();
            }
        }

        public StringValueProperty NameProperty { get; set; }
        public NodeVarData Data { get; set; }
        public object DefaultInitialValue { get; set; }

        private PopupSearch _containerVarPopupSearch;
        private Button _containerVarSelectButton;
        private NodePathValueProperty _containerPathProperty;
        private OptionButton _valueTypeButton;
        private OptionButton _operationButton;
        private Button _isPointerButton;
        private MarginContainer _valuePropertyContainer;
        private ValueProperty _valueProperty;
        private Button _deleteButton;
        private ValueTypeData[] _valueTypes;
        private OperationTypeData[] _operationTypes;
        private Node _relativeToNode;
        private IAssetsRegistry _assetsRegistry;
        private Button _resetInitalValueButton;

        public DictNodeVarsValuePropertyEntry() { }
        public DictNodeVarsValuePropertyEntry(IAssetsRegistry assetsRegistry, Node sceneRoot, Node relativeToNode)
        {
            _assetsRegistry = assetsRegistry;
            _relativeToNode = relativeToNode;

            var control = new Control();
            control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            control.SizeFlagsStretchRatio = 0.1f;
            control.RectSize = new Vector2(24, 0);
            AddChild(control);

            var vBox = new VBoxContainer();
            vBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            vBox.SizeFlagsStretchRatio = 0.9f;
            AddChild(vBox);

            var firstRowHBox = new HBoxContainer();
            var secondRowHBox = new HBoxContainer();
            vBox.AddChild(firstRowHBox);
            vBox.AddChild(secondRowHBox);

            NameProperty = new StringValueProperty();
            NameProperty.ValueChanged += OnNameChanged;
            NameProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            _containerVarSelectButton = new Button();
            _containerVarSelectButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _containerVarSelectButton.ClipText = true;
            _containerVarSelectButton.Connect("pressed", this, nameof(OnContainerVarSelectPressed));

            _containerVarPopupSearch = new PopupSearch();
            _containerVarPopupSearch.ItemListLineHeight = (int)(_containerVarPopupSearch.ItemListLineHeight * _assetsRegistry.Scale);
            _containerVarPopupSearch.Connect(nameof(PopupSearch.EntrySelected), this, nameof(OnContainerVarNameSelected));

            _valuePropertyContainer = new MarginContainer();
            _valuePropertyContainer.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            _containerPathProperty = new NodePathValueProperty(sceneRoot, (node) => node is INodeVarsContainer);
            _containerPathProperty.ValueChanged += OnNodePathChanged;
            _containerPathProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _containerPathProperty.RelativeToNode = relativeToNode;

            _valueTypeButton = new OptionButton();
            _valueTypeButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _valueTypeButton.ClipText = true;
            _valueTypeButton.Connect("item_selected", this, nameof(OnValueTypeSelected));

            _operationButton = new OptionButton();
            _operationButton.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            _operationButton.RectMinSize = new Vector2(80 * _assetsRegistry.Scale, 0);
            _operationButton.Connect("item_selected", this, nameof(OnOperationSelected));

            _isPointerButton = new Button();
            _isPointerButton.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            _isPointerButton.ToggleMode = true;
            _isPointerButton.Connect("toggled", this, nameof(OnIsPointerToggled));

            _deleteButton = new Button();
            _deleteButton.Connect("pressed", this, nameof(OnDeletePressed));

            _resetInitalValueButton = new Button();
            _resetInitalValueButton.Connect("pressed", this, nameof(OnResetButtonPressed));

            AddChild(_containerVarPopupSearch);

            firstRowHBox.AddChild(NameProperty);
            firstRowHBox.AddChild(_valueTypeButton);
            firstRowHBox.AddChild(_operationButton);

            //control = new Control();
            //control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            //control.SizeFlagsStretchRatio = 0.1f;
            //control.RectSize = new Vector2(24, 0);
            //secondRowHBox.AddChild(control);
            secondRowHBox.AddChild(_isPointerButton);
            secondRowHBox.AddChild(_containerPathProperty);
            secondRowHBox.AddChild(_containerVarSelectButton);
            secondRowHBox.AddChild(_valuePropertyContainer);
            secondRowHBox.AddChild(_resetInitalValueButton);
            secondRowHBox.AddChild(_deleteButton);
        }

        public override void _Ready()
        {
#if TOOLS
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
#endif
            _deleteButton.Icon = GetIcon("Remove", "EditorIcons");
            _isPointerButton.Icon = GetIcon("GuiScrollArrowRightHl", "EditorIcons");
            _resetInitalValueButton.Icon = GetIcon("Reload", "EditorIcons");

            InitValueTypes();
            InitOperationTypes();
            UpdateDisabledUI();
        }

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
            {
                NameProperty.ValueChanged -= OnNameChanged;
                _containerPathProperty.ValueChanged -= OnNodePathChanged;
            }
        }

        private void UpdateDisabledUI()
        {
            NameProperty.Disabled = _disabled || !NameEditable;
            _valueTypeButton.Disabled = _disabled || !ValueTypeEditable;
            _operationButton.Disabled = _disabled || !OperationEditable;
            _deleteButton.Visible = Deletable;
            _containerPathProperty.Disabled = _disabled;
            _containerVarSelectButton.Disabled = _disabled;
            _deleteButton.Disabled = _disabled;
        }

        private void OnResetButtonPressed()
        {
            Data.InitialValue = DefaultInitialValue;
            _valueProperty.SetValue(Data.InitialValue, false);
            UpdateResetButton();
            InvokeDataChanged();
        }

        private void OnContainerVarNameSelected(string name)
        {
            Data.ContainerVarName = name;
            _containerVarSelectButton.Text = name;
            InvokeDataChanged();
        }

        private void SetValueTypeValueDisplay(Type type)
        {
            var valueTypeData = _valueTypes.First(x => x.Type == type);
            _valueTypeButton.Select(valueTypeData.Index);
            _valueTypeButton.SizeFlagsHorizontal = (int)(valueTypeData.UseIconOnly ? SizeFlags.Fill : SizeFlags.ExpandFill);
            if (valueTypeData.UseIconOnly)
                _valueTypeButton.Text = "";
        }

        private void SetOperationsValueDisplay(NodeVarOperation operation)
        {
            var operationTypeData = _operationTypes.First(x => x.Operation == operation);
            _operationButton.Select(operationTypeData.Index);
        }

        public void SetData(NodeVarData value, object defaultInitialValue = null)
        {
            var oldData = Data;
            // Clone the data
            Data = value.Clone();
            DefaultInitialValue = defaultInitialValue;

            if ((oldData == null && Data != null) || (oldData != null && oldData.ValueType != Data.ValueType))
                UpdateValuePropertyType();

            SetValueTypeValueDisplay(Data.ValueType);
            SetOperationsValueDisplay(Data.Operation);
            NameProperty.SetValue(Data.Name);
            _valueProperty.SetValue(Data.InitialValue, false);
            _containerPathProperty.SetValue(Data.ContainerPath, false);
            _containerVarSelectButton.Text = Data.ContainerVarName ?? "[Empty]";
            _isPointerButton.SetPressedNoSignal(Data.IsPointer);
            UpdateIsPointerVisibility();
            UpdateResetButton();
        }

        private void UpdateIsPointerVisibility()
        {
            var containerNode = _relativeToNode.GetNodeOrNull(Data?.ContainerPath ?? new NodePath()) as INodeVarsContainer;
            _containerPathProperty.Visible = Data.IsPointer;
            _containerVarSelectButton.Visible = Data.IsPointer;
            _containerVarSelectButton.Disabled = containerNode == null;
            _valuePropertyContainer.Visible = !Data.IsPointer;
        }

        /// <summary>
        /// Recreates the ValueProperty based on the current Data.ValueType.
        /// </summary>
        private void UpdateValuePropertyType()
        {
            // Update the ValueProperty to the new data type if the data type changes.
            _valueProperty?.QueueFree();
            _valueProperty = ValueProperty.CreateValueProperty(Data.ValueType);
            _valueProperty.ValueChanged += (newValue) =>
            {
                Data.InitialValue = newValue;
                UpdateResetButton();
                InvokeDataChanged();
            };
            _valueProperty.SetValue(Data.InitialValue, false);
            _valuePropertyContainer.AddChild(_valueProperty);
        }

        private void UpdateResetButton()
        {
            _resetInitalValueButton.Visible = DefaultInitialValue != null && !Equals(Data.InitialValue, DefaultInitialValue);
        }

        private void InitOperationTypes()
        {
            _operationTypes = new[] {
                new OperationTypeData()
                {
                    Name = "Get/Set",
                    Operation = NodeVarOperation.GetSet
                },
                new OperationTypeData() {
                    Name = "Get",
                    Operation = NodeVarOperation.Get
                },
                new OperationTypeData() {
                    Name = "Set",
                    Operation = NodeVarOperation.Set
                },
            };
            foreach (var type in _operationTypes)
            {
                var index = _operationButton.GetItemCount();
                _operationButton.AddItem(type.Name);
                type.Index = index;
            }
        }

        private void InitValueTypes()
        {
            _valueTypes = new[] {
                new ValueTypeData() {
                    Name = "int",
                    Type = typeof(int),
                    Icon = GetIcon("int", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "float",
                    Type = typeof(float),
                    Icon = GetIcon("float", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "bool",
                    Type = typeof(bool),
                    Icon = GetIcon("bool", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "string",
                    Type = typeof(string),
                    Icon = GetIcon("String", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "Vector2",
                    Type = typeof(Vector2),
                    Icon = GetIcon("Vector2", "EditorIcons"),
                    UseIconOnly = true
                },
                new ValueTypeData() {
                    Name = "Vector3",
                    Type = typeof(Vector3),
                    Icon = GetIcon("Vector3", "EditorIcons"),
                    UseIconOnly = true
                }
            };
            foreach (var type in _valueTypes)
            {
                int currIndex = _valueTypeButton.GetItemCount();
                type.Index = currIndex;
                _valueTypeButton.AddIconItem(type.Icon, type.Name);
            }
        }

        private void InvokeDataChanged() => DataChanged?.Invoke(Data.Name, Data);

        private void OnNameChanged(string newName)
        {
            var oldName = Data.Name;
            Data.Name = newName;
            NameChanged?.Invoke(oldName, this);
        }

        private void OnNodePathChanged(NodePath newValue)
        {
            Data.ContainerPath = newValue;
            UpdateIsPointerVisibility();
            InvokeDataChanged();
        }

        private void OnValueTypeSelected(int index)
        {
            var newType = _valueTypes.First(x => x.Index == index).Type;
            if (Data.ValueType == newType)
                return;
            Data.ValueType = newType;
            Data.InitialValue = DefaultValueUtils.GetDefault(Data.ValueType);
            SetValueTypeValueDisplay(Data.ValueType);
            UpdateValuePropertyType();
            InvokeDataChanged();
        }

        private void OnOperationSelected(int index)
        {
            var operation = _operationTypes.First(x => x.Index == index).Operation;
            if (Data.Operation == operation)
                return;
            Data.Operation = operation;
            SetOperationsValueDisplay(Data.Operation);
            InvokeDataChanged();
        }

        private void OnIsPointerToggled(bool isPointer)
        {
            if (isPointer)
            {
                Data.ContainerPath = new NodePath();
            }
            else
            {
                _containerPathProperty.SetValue(Data.ContainerPath, false);
                Data.ContainerPath = null;
                Data.ContainerVarName = null;
            }
            UpdateIsPointerVisibility();
            InvokeDataChanged();
        }

        private void OnContainerVarSelectPressed()
        {
            var container = _relativeToNode.GetNode<INodeVarsContainer>(Data.ContainerPath);
            _containerVarPopupSearch.SearchEntries = container.GetDictNodeVarsList().Select(x => x.Name).ToArray();
            _containerVarPopupSearch.Popup_(_containerVarSelectButton.GetGlobalRect());
        }

        private void OnDeletePressed() => Deleted?.Invoke(Data.Name);
    }
}
