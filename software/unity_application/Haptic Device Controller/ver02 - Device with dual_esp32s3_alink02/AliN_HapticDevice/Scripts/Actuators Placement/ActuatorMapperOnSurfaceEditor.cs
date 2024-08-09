using UnityEngine;
using UnityEditor;

namespace AliN.Microcontroller
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ActuatorMapperOnSurface))]
    public class ActuatorMapperOnSurfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ActuatorMapperOnSurface script = (ActuatorMapperOnSurface)target;

            // Show general buttons and actions
            if (GUILayout.Button("Draw Line from Objects to the Surface"))
            {
                script.DrawLineFromObjectsToSurface();
            }

            if (GUILayout.Button("Place Objects to the Surface"))
            {
                script.PlaceActuatorsOnTheSurface();
            }

            if (GUILayout.Button("Reset Actuators Position"))
            {
                script.ResetActuators();
            }

            // Conditional display based on TypeOfMapping
            switch (script.TypeOfMapping)
            {
                case ActuatorMapperOnSurface.MappingType.SimpleSurfaceMapping:
                    EditorGUILayout.HelpBox("Simple Surface Mapping: Projects each actuator directly onto the surface along a specified direction. Actuators are moved only if they align and collide with the target object in the chosen direction. Actuators that do not encounter any object along this path will remain in their original positions, ensuring that only obstructed actuators are adjusted.", MessageType.Info);
                    break;

                case ActuatorMapperOnSurface.MappingType.ReferenceBasedSurfaceMapping:
                    EditorGUILayout.HelpBox("Reference Based Surface Mapping: This method positions all actuators in relation to a specified reference actuator, ensuring that each actuator is placed relative to the initial distances from this reference. The reference actuator is used as a fixed point to project other actuators onto the surface, maintaining their relative spatial configuration based on their original setup.", MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("referenceObjectRow"), new GUIContent("Reference Object Row"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("referenceObjectCol"), new GUIContent("Reference Object Column"));
                    break;

                case ActuatorMapperOnSurface.MappingType.DistanceMaintainedSurfaceMapping:
                    EditorGUILayout.HelpBox("Distance Maintained Surface Mapping: Prioritizes the maintenance of predefined distances between actuators upon mapping them onto the surface. The process begins with the first actuator located at the bottom-left corner of the grid ([0,0]), which must make contact with the surface. Subsequent actuators are then positioned in such a way that the specified distances between them and their neighbors are preserved as closely as possible, adapting to the surface contours.", MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("priorityIsRowDistance"), new GUIContent("Priority is Row Distance"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
