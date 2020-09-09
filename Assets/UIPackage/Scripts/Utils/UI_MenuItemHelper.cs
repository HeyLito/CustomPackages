#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CustomUI.Utils
{
    public class UI_MenuItemHelper<T> where T : Component
    {
        public static T CreateObject()
        {
            var parent = FindParent();
            GameObject obj = new GameObject("New GameObject", typeof(T)) { layer = parent.gameObject.layer };
            obj.transform.SetParent(parent);
            obj.transform.localPosition = new Vector3();
            return obj.GetComponent<T>();
        }
        public static T CreateObject(string name)
        {
            var parent = FindParent();
            GameObject obj = new GameObject(name, typeof(T)) { layer = parent.gameObject.layer };
            obj.transform.SetParent(parent);
            obj.transform.localPosition = new Vector3();
            return obj.GetComponent<T>();
        }
        public static T CreateObject(string name, params Type[] components)
        {
            var parent = FindParent();
            var types = new Type[components.Length + 1];
            types[components.Length] = typeof(T);
            for (int i = 0; i < components.Length; i++)
                types[i] = components[i];
            GameObject obj = new GameObject(name, types) { layer = parent.gameObject.layer };
            obj.transform.SetParent(parent);
            obj.transform.localPosition = new Vector3();
            return obj.GetComponent<T>();
        }

        private static Transform FindParent() 
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                    return Selection.activeTransform;
                else
                {
                    var parent = CreateCanvas().transform;
                    parent.SetParent(Selection.activeTransform);
                    if (!UnityEngine.Object.FindObjectOfType<EventSystem>())
                        CreateEventSystem();
                    return parent;
                }
            }
            else
            {

                var parent = UnityEngine.Object.FindObjectOfType<Canvas>()?.transform;
                if (!parent)
                {
                    parent = CreateCanvas().transform;
                    if (!UnityEngine.Object.FindObjectOfType<EventSystem>())
                        CreateEventSystem();
                }
                return parent;
            }
        }
        private static Canvas CreateCanvas()
        {
            var obj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)) { layer = LayerMask.NameToLayer("UI") };
            var canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return canvas;
        }
        private static EventSystem CreateEventSystem()
        {
            var obj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            return obj.GetComponent<EventSystem>();
        }
    }
}
#endif