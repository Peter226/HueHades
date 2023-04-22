using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IMenuBarElement
{
    public abstract VisualElement Element { get; }

    public abstract void InitializeMenu();

}
