using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToggleButton : Button
{
    private const string ussToggleButton = "toggle-button";
    private const string ussToggleButtonToggle = "toggle-button-toggle";
    private const string ussToggleButtonLabel = "toggle-button-label";
    private const string ussToggleButtonIcon = "toggle-button-icon";
    private bool _toggled;
    private bool _hasSwitchingIcon;
    private string _icon;
    private string _toggledIcon;
    private Image _iconImage;

    public bool Toggled { 
        get { 
            return _toggled;
        } 
        set {
            bool needsToggle = false;
            if (_toggled && !value)
            {
                needsToggle = true;
                RemoveFromClassList(ussToggleButtonToggle);
                if (_hasSwitchingIcon)
                {
                    _iconImage.image = Icons.GetIcon(_icon);
                }
            }
            else
            {
                if (!_toggled && value)
                {
                    needsToggle = true;
                    AddToClassList(ussToggleButtonToggle);
                    if (_hasSwitchingIcon)
                    {
                        _iconImage.image = Icons.GetIcon(_toggledIcon);
                    }
                }
            }
            _toggled = value;
            if (needsToggle)
            {
                OnToggle?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// Set the toggle state without raising any events
    /// </summary>
    /// <param name="value"></param>
    public void SetToggleStateSilent(bool value)
    {
        if (_toggled && !value)
        {
            RemoveFromClassList(ussToggleButtonToggle);
            if (_hasSwitchingIcon)
            {
                _iconImage.image = Icons.GetIcon(_icon);
            }
        }
        else
        {
            if (!_toggled && value)
            {
                AddToClassList(ussToggleButtonToggle);
                if (_hasSwitchingIcon)
                {
                    _iconImage.image = Icons.GetIcon(_toggledIcon);
                }
            }
        }
        _toggled = value;
    }



    public Action<bool> OnToggle;

    public ToggleButton(string textContent = "", string icon = "", string toggledIcon = "") {
        AddToClassList(ussToggleButton);

        _icon = icon;
        _toggledIcon = toggledIcon;

        if (icon.Length > 0)
        {
            Image image = new Image();
            _iconImage = image;
            if(toggledIcon.Length > 0) _hasSwitchingIcon = true;
            image.AddToClassList(ussToggleButtonIcon);
            image.image = Icons.GetIcon(icon);
            image.scaleMode = ScaleMode.ScaleToFit;
            hierarchy.Add(image);
        }
        if (textContent.Length > 0){
            Label label = new Label();
            label.AddToClassList(ussToggleButtonLabel);
            label.text = textContent;
            hierarchy.Add(label);
        }
        clicked += OnClick;
    }

    private void OnClick()
    {
        Toggled = !Toggled;
    }
}
