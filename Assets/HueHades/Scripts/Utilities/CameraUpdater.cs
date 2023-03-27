using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Utilities
{

    public class CameraUpdater : MonoBehaviour
    {
        private Dictionary<Camera, GameObject> cameras = new Dictionary<Camera, GameObject>();


        public void QueueRender(Camera camera, GameObject hierarchy)
        {
            if (!cameras.ContainsKey(camera))
            {
                cameras.Add(camera, hierarchy);
            }
        }


        void Update()
        {
            foreach (var (camera, hierarchy) in cameras)
            {
                hierarchy.SetActive(true);
                camera.Render();
                hierarchy.SetActive(false);
            }
            cameras.Clear();
        }
    }
}