using UnityEngine;
using UnityEditor;

namespace AliN.Microcontroller
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectMapperOnSurface))]
    public class ObjectMapperOnSurfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ObjectMapperOnSurface myScript = (ObjectMapperOnSurface)target;
            if (GUILayout.Button("Collect chind objects"))
            {
                myScript.SetAllChildObjectsAndActuator();
            }

            if (GUILayout.Button("Draw Line from Objects to the Surface"))
            {
                myScript.DrawLineFromObjectsToSurface();
            }

            if (GUILayout.Button("Place Objects to the Surface"))
            {
                myScript.MapObjectsToSurface();
            }


            if (GUILayout.Button("Place All Objects on the surface according to the reference object"))
            {
                myScript.PlaceAllObjectsOnTheSurfaceAccordingToTheReferenceObject();
            }
 

            
        }
    }
#endif
}
