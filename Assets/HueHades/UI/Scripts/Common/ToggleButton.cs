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
    public bool Toggled { 
        get { 
            return _toggled;
        } 
        set {
            bool needsToggle = false; ;
            if (_toggled && !value)
            {
                needsToggle = true;
                RemoveFromClassList(ussToggleButtonToggle);
            }
            else
            {
                if (!_toggled && value)
                {
                    needsToggle = true;
                    AddToClassList(ussToggleButtonToggle);
                }
            }
            _toggled = value;
            if (needsToggle)
            {
                OnToggle?.Invoke(value);
            }
        }
    }

    public Action<bool> OnToggle;

    public ToggleButton(string textContent = "", string icon = "") {
        AddToClassList(ussToggleButton);
        if (icon.Length > 0)
        {
            Image image = new Image();
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
