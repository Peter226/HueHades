using System;
using System.Collections;
using System.Collections.Generic;
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
    public T value { get { return _selectedValue; } set { _selectedLabel.text = _displayNameMethod(value); _selectedValue = value; } }
    private List<Button> _buttons = new List<Button>();

    private VisualElement _overlay;
    private Label _inputLabel;
    private string _label = "";
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
        _overlay.RegisterCallback<FocusOutEvent>(OnLostFocus);
    }

    private void OnLostFocus(FocusOutEvent evt)
    {
        Debug.Log($"LostFocusTo: {evt.target}");
        foreach (Button button in _buttons)
        {
            if (button == evt.target)
            {
                return;
            }
        }
        if (_overlay == evt.target)
        {
            return;
        }
        if (this == evt.target)
        {
            return;
        }
        window.HideOverlay(_overlay);
    }

    private void OnClicked()
    {
        window.ShowOverlay(_overlay, _button, OverlayPlacement.Bottom);
        _overlay.Focus();
    }

    public void SetDataSource(List<T> data, Func<T, string> displayNameMethod)
    {
        _data = data;
        _displayNameMethod = displayNameMethod;
        if (value == null && data.Count > 0)
        {
            value = data[0];
        }
        value = value;

        _overlay.Clear();
        foreach (var dataElement in data)
        {
            var dataButton = new Button();
            dataButton.text = displayNameMethod(dataElement);
            _overlay.Add(dataButton);
        }

    }
}
