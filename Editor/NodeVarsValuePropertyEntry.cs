using Fractural.Plugin;
using Fractural.Utils;
using Godot;
using System;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.DependencyInjection
{
    [Tool]
    public class NodeVarsValuePropertyEntry : HBoxContainer
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
        public event Action<string, NodeVarsValuePropertyEntry> NameChanged;
        /// <summary>
        /// DataChanged(string name, NodePath newValue)
        /// </summary>
        public event Action<string, GDC.Dictionary> DataChanged;
        /// <summary>
        /// Deleted(string name)
        /// </summary>
        public event Action<string> Deleted;

        private bool _disabled;
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                if (IsInsideTree())
                {
                    NameProperty.Disabled = !NameEditable || value;
                    _deleteButton.Disabled = value;
                }
            }
        }
        public bool NameEditable { get; private set; }

        public StringValueProperty NameProperty { get; set; }
        public NodeVarData Data { get; set; }

        private PopupSearch _containerVarPopupSearch;
        private Button _containerVarSelectButton;
        private NodePathValueProperty _nodePathProperty;
        private OptionButton _valueTypeButton;
        private OptionButton _operationButton;
        private Button _isPointerButton;
        private MarginContainer _valuePropertyContainer;
        private ValueProperty _valueProperty;
        private Button _deleteButton;
        private ValueTypeData[] _valueTypes;
        private OperationTypeData[] _operationTypes;
        private Node _relativeToNode;

        public NodeVarsValuePropertyEntry() { }
        public NodeVarsValuePropertyEntry(Node sceneRoot, Node relativeToNode)
        {
            _relativeToNode = relativeToNode;

            var control = new Control();
            control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            control.SizeFlagsStretchRatio = 0.25f;
            control.RectSize = new Vector2(24, 0);
            AddChild(control);

            NameProperty = new StringValueProperty();
            NameProperty.ValueChanged += OnNameChanged;
            NameProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            _containerVarSelectButton = new Button();
            _containerVarSelectButton.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            _containerVarSelectButton.Connect("pressed", this, nameof(OnContainerVarSelectPressed));

            _containerVarPopupSearch = new PopupSearch();
            _containerVarPopupSearch.Connect(nameof(PopupSearch.EntrySelected), this, nameof(OnContainerVarNameSelected));

            _valuePropertyContainer = new MarginContainer();
            _valuePropertyContainer.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            _nodePathProperty = new NodePathValueProperty(sceneRoot, (node) => node is INodeVarsContainer);
            _nodePathProperty.ValueChanged += OnNodePathChanged;
            _nodePathProperty.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _nodePathProperty.RelativeToNode = relativeToNode;

            _valueTypeButton = new OptionButton();
            _valueTypeButton.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            _valueTypeButton.ClipText = true;
            _valueTypeButton.Connect("item_selected", this, nameof(OnValueTypeSelected));

            _operationButton = new OptionButton();
            _operationButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _operationButton.SizeFlagsStretchRatio = 0.6f;
            _operationButton.ClipText = true;
            _operationButton.Connect("item_selected", this, nameof(OnOperationSelected));

            _isPointerButton = new Button();
            _isPointerButton.SizeFlagsHorizontal = (int)SizeFlags.Fill; ;
            _isPointerButton.ClipText = true;
            _isPointerButton.Connect("toggled", this, nameof(OnIsPointerToggled));

            AddChild(_containerVarPopupSearch);
            AddChild(NameProperty);
            AddChild(_valueTypeButton);
            AddChild(_operationButton);
            AddChild(_nodePathProperty);
            AddChild(_containerVarSelectButton);
            AddChild(_valuePropertyContainer);
            AddChild(_isPointerButton);

            _deleteButton = new Button();
            _deleteButton.Connect("pressed", this, nameof(OnDeletePressed));
            AddChild(_deleteButton);
        }

        public override void _Ready()
        {
            base._Ready();
            if (NodeUtils.IsInEditorSceneTab(this))
                return;
            _deleteButton.Icon = GetIcon("Remove", "EditorIcons");
            _isPointerButton.Icon = GetIcon("GuiScrollArrowRightHl", "EditorIcons");

            InitValueTypes();
            InitOperationTypes();
        }

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
            {
                NameProperty.ValueChanged -= OnNameChanged;
                _nodePathProperty.ValueChanged -= OnNodePathChanged;
            }
        }

        private void OnContainerVarNameSelected(string name)
        {
            Data.ContainerVarName = name;
            InvokeDataChanged();
        }

        private void SetValueTypeValueDisplay(Type type)
        {
            var valueTypeData = _valueTypes.First(x => x.Type == type);
            _valueTypeButton.Select(valueTypeData.Index);
            if (valueTypeData.UseIconOnly)
                _valueTypeButton.Text = "";
        }

        private void SetOperationsValueDisplay(NodeVarOperation operation)
        {
            var operationTypeData = _operationTypes.First(x => x.Operation == operation);
            _operationButton.Select(operationTypeData.Index);
        }

        public void SetData(NodeVarData value)
        {
            var oldData = Data;
            Data = value;

            if ((oldData == null && value != null) || (oldData != null && oldData.ValueType != Data.ValueType))
                UpdateValuePropertyType();

            NameProperty.Disabled = Data.IsFixed;
            _valueTypeButton.Disabled = Data.IsFixed;
            _operationButton.Disabled = Data.IsFixed;
            _deleteButton.Visible = !Data.IsFixed;
            SetValueTypeValueDisplay(value.ValueType);
            SetOperationsValueDisplay(value.Operation);
            NameProperty.SetValue(value.Name);
            _valueProperty.SetValue(value.Value, false);
            _nodePathProperty.SetValue(value.ContainerPath, false);
            UpdateIsPointerVisibility();
        }

        private void UpdateIsPointerVisibility()
        {
            _nodePathProperty.Visible = Data.IsPointer;
            _containerVarSelectButton.Visible = Data.IsPointer;
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
                Data.Value = newValue;
                InvokeDataChanged();
            };
            _valuePropertyContainer.AddChild(_valueProperty);
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

        private void InvokeDataChanged() => DataChanged?.Invoke(Data.Name, Data.ToGDDict());

        private void OnNameChanged(string newName)
        {
            GD.Print("internal name changed ", newName);
            var oldName = Data.Name;
            Data.Name = newName;
            NameChanged?.Invoke(oldName, this);
        }

        private void OnNodePathChanged(NodePath newValue)
        {
            Data.ContainerPath = newValue;
            InvokeDataChanged();
        }

        private void OnValueTypeSelected(int index)
        {
            var newType = _valueTypes.First(x => x.Index == index).Type;
            if (Data.ValueType == newType)
                return;
            Data.ValueType = newType;
            Data.Value = DefaultValueUtils.GetDefault(Data.ValueType);
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
                Data.ContainerPath = new NodePath();
            else
                Data.ContainerPath = null;
            UpdateIsPointerVisibility();
            InvokeDataChanged();
        }

        private void OnContainerVarSelectPressed()
        {
            var container = _relativeToNode.GetNode<INodeVarsContainer>(Data.ContainerPath);
            _containerVarPopupSearch.SearchEntries = container.GetNodeVarsList().Select(x => x.Name).ToArray();
            _containerVarPopupSearch.Popup_(_containerVarPopupSearch.GetGlobalRect());
        }

        private void OnDeletePressed() => Deleted?.Invoke(Data.Name);
    }
}
