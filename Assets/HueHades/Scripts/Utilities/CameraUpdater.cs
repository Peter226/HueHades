using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Utilities
{

    public class CameraUpdater : MonoBehaviour
    {
        public Action OnUpdate;

        void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}