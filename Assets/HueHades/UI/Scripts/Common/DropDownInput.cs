using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DropDownInput<T> : HueHadesElement
{
    private Button _button;
    private Label _selectedLabel;
    private Label _dropDownArrow;

    private const string ussDropDownLabel = "dropdown-label";
    private const string ussDropDownArrowLabel = "dropdown-arrow-label";
    private const string ussDropDownButton = "dropdown-button";
    private const string ussDropDown = "dropdown";

    private List<T> _data = new List<T>();
    Func<T, string> _displayNameMethod;
    private T _selectedValue;
    public T value { get { return _selectedValue; } set { _selectedLabel.text = _displayNameMethod(value); _selectedValue = value; ValueChanged?.Invoke(value); } }

    public Action<T> ValueChanged;
    
    private List<Button> _buttons = new List<Button>();

    private VisualElement _overlay;
    private Label _inputLabel;
    private string _label = "";

    private bool _dropped;

    public string label { get { return _label; } 
        set {
            if (value.Length > 0) _inputLabel.style.display = DisplayStyle.Flex;
            else _inputLabel.style.display = DisplayStyle.None;
            _label = value;
            _inputLabel.text = _label;
        }
    }


    public DropDownInput(HueHadesWindow window) : base(window)
    {
        _inputLabel = new Label();
        _inputLabel.style.display = DisplayStyle.None;
        _button = new Button();
        _selectedLabel = new Label();
        _dropDownArrow = new Label();
        _selectedLabel.text = "";
        _dropDownArrow.text = "▼";
        _button.hierarchy.Add(_selectedLabel);
        _button.hierarchy.Add(_dropDownArrow);

        _selectedLabel.style.flexGrow = 1;
        _dropDownArrow.style.flexGrow = 0;

        hierarchy.Add(_inputLabel);
        hierarchy.Add(_button);

        AddToClassList(ussDropDown);
        _button.AddToClassList(ussDropDownButton);
        _selectedLabel.AddToClassList(ussDropDownLabel);
        _dropDownArrow.AddToClassList(ussDropDownArrowLabel);

        _button.clicked += OnClicked;
        
        _overlay = new VisualElement();
        _overlay.style.position = Position.Absolute;

        _button.RegisterCallback<FocusOutEvent>(OnLostFocus);
    }

    private void OnLostFocus(FocusOutEvent evt)
    {
        foreach (Button button in _buttons)
        {
            if (button == evt.relatedTarget)
            {
                return;
            }
        }
        if (_overlay == evt.relatedTarget)
        {
            return;
        }
        window.HideOverlay(_overlay);
        _dropped = false;
    }

    private void SelectValue(T value)
    {
        this.value = value;
        window.HideOverlay(_overlay);
        _dropped = false;
    }


    private void OnClicked()
    {
        if (_dropped)
        {
            _dropped = false;
            window.HideOverlay(_overlay);
            return;
        }
        _dropped = true;
        window.ShowOverlay(_overlay, _button, OverlayPlacement.Bottom);
        _button.Focus();
        
    }

    public void SetDataSource(List<T> data, Func<T, string> displayNameMethod)
    {
        _data = data;
        _displayNameMethod = displayNameMethod;
        if ((value == null || (!data.Contains(value))) && data.Count > 0)
        {
            value = data[0];
        }
        value = value;

        _overlay.Clear();
        _buttons.Clear();
        foreach (var dataElement in data)
        {
            var dataButton = new DropDownButton(this, dataElement);
            dataButton.text = displayNameMethod(dataElement);
            _buttons.Add(dataButton);
            _overlay.Add(dataButton);
        }
    }

    private class DropDownButton : Button
    {
        T _dropdownValue;
        DropDownInput<T> _dropdownInput;
        public DropDownButton(DropDownInput<T> dropdown, T dropdownValue)
        {
            _dropdownValue = dropdownValue;
            _dropdownInput = dropdown;
            clicked += OnClicked;
        }

        private void OnClicked()
        {
            _dropdownInput.SelectValue(_dropdownValue);
        }
    }
}
