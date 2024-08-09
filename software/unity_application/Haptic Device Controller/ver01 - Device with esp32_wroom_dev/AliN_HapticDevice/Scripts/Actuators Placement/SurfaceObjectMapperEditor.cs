using UnityEngine;
using UnityEditor;

namespace AliN.Microcontroller
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SurfaceObjectMapper))]
    public class SurfaceObjectMapperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SurfaceObjectMapper myScript = (SurfaceObjectMapper)target;
            if (GUILayout.Button("Update chind objects"))
            {
                myScript.SetAllChildObjectsAndActuator();
            }

            if (GUILayout.Button("Draw Line from Objects to the Surface"))
            {
                myScript.DrawLineFromObjectsToSurface();
            }

            if (GUILayout.Button("Mapp to Surface"))
            {
                myScript.MapObjectsToSurface();
            }
        }
    }
#endif
}
