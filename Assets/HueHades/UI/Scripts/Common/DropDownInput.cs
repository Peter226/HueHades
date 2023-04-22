using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DropDownInput<T> : HueHadesElement
{
    private Button _button;
    private Label _label;
    private Label _dropDownArrow;

    private const string ussDropDownLabel = "dropdown-label";
    private const string ussDropDownArrowLabel = "dropdown-arrow-label";
    private const string ussDropDownButton = "dropdown-button";
    private const string ussDropDown = "dropdown";

    private List<T> _data = new List<T>();
    Func<T, string> _displayNameMethod;
    private T _selectedValue;
    public T selectedValue { get { return _selectedValue; } set { _label.text = _displayNameMethod(value); _selectedValue = value; } }
    private List<Button> _buttons = new List<Button>();

    private VisualElement _overlay;

    public DropDownInput(HueHadesWindow window) : base(window)
    {
        _button = new Button();
        _label = new Label();
        _dropDownArrow = new Label();
        _label.text = "";
        _dropDownArrow.text = "▼";
        _button.hierarchy.Add(_label);
        _button.hierarchy.Add(_dropDownArrow);

        _label.style.flexGrow = 1;
        _dropDownArrow.style.flexGrow = 0;

        hierarchy.Add(_button);

        AddToClassList(ussDropDown);
        _button.AddToClassList(ussDropDownButton);
        _label.AddToClassList(ussDropDownLabel);
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
        window.ShowOverlay(_overlay, this, OverlayPlacement.Bottom);
        _overlay.Focus();
    }

    public void SetDataSource(List<T> data, Func<T, string> displayNameMethod)
    {
        _data = data;
        _displayNameMethod = displayNameMethod;
        if (selectedValue == null && data.Count > 0)
        {
            selectedValue = data[0];
        }
        selectedValue = selectedValue;

        foreach (var dataElement in data)
        {
            var dataButton = new Button();
            dataButton.text = displayNameMethod(dataElement);
            _overlay.Add(dataButton);
        }

    }
}
