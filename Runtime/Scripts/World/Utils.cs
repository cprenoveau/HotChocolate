using System.Collections.Generic;
using UnityEngine;

namespace HotChocolate.World
{
    public static class Utils
    {
        public static List<T> FindAllInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : MonoBehaviour
        {
            List<T> components = new List<T>();

            var go = scene.GetRootGameObjects();
            foreach (var obj in go)
            {
                components.AddRange(obj.GetComponentsInChildren<T>());
            }

            return components;
        }

        public static T FindInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : MonoBehaviour
        {
            var go = scene.GetRootGameObjects();
            foreach(var obj in go)
            {
                var component = obj.GetComponentInChildren<T>();
                if (component != null)
                    return component;
            }

            return null;
        }
    }
}
