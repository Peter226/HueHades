using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutSystem : MonoBehaviour
{

    public static Action Undo;
    public static Action Redo;
    public static Action Save;
    public static Action New;
    public static Action Open;


    void Update()
    {
        if (!Input.GetKey(KeyCode.LeftControl)) return;
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Redo?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            New?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Open?.Invoke();
        }
    }
}
