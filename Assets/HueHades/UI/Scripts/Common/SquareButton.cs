using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SquareButton : Button
{
    private const string ussToggleButton = "toggle-button";
    private const string ussToggleButtonLabel = "toggle-button-label";
    private const string ussToggleButtonIcon = "toggle-button-icon";

    public SquareButton(string textContent = "", string icon = "")
    {
        AddToClassList(ussToggleButton);
        if (icon.Length > 0)
        {
            Image image = new Image();
            image.AddToClassList(ussToggleButtonIcon);
            image.image = Icons.GetIcon(icon);
            image.scaleMode = ScaleMode.ScaleToFit;
            hierarchy.Add(image);
        }
        if (textContent.Length > 0)
        {
            Label label = new Label();
            label.AddToClassList(ussToggleButtonLabel);
            label.text = textContent;
            hierarchy.Add(label);
        }
    }

}
