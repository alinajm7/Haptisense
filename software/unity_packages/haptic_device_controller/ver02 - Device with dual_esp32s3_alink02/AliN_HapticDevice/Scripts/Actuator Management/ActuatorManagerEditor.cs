using UnityEngine;
using UnityEditor;
using AliN.Microcontroller;
using AliN.Microcontroller.Classes;

// Edit Unity Inspector
#if UNITY_EDITOR
[CustomEditor(typeof(ActuatorsManager))]
public class ActuatorManagerEditor : Editor
{
    void OnSceneGUI()
    {
        ActuatorsManager manager = (ActuatorsManager)target;

        if (manager.ShowActuatorValues && manager.arrayOfActuators != null)
        {
            // Create a single GUIStyle instance outside the loop
            GUIStyle style = new GUIStyle();

            foreach (Actuator actuator in manager.arrayOfActuators)
            {
                if (actuator != null && actuator.transform != null)
                {
                    style.fontSize = manager.actuatorValueForntSize;
                    style.alignment = TextAnchor.MiddleCenter;
                    if (actuator.actuatorValue == 0)
                    {
                        style.normal.textColor = new Color(0, 0, 0, 0.05f); // Black with 0.15 alpha
                    }
                    else
                    {
                        // Complementary color
                        Color complementaryColor = new Color(1 - actuator.material.color.r, 1 - actuator.material.color.g, 1 - actuator.material.color.b, 1);
                        style.normal.textColor = complementaryColor;                       
                    }
                    if (manager.ShowActuatorMoreData)
                    {
                        Handles.Label(actuator.transform.position, actuator.actuatorValue.ToString("F2") + "\n" + actuator.lowStateDuration.ToString("F0") + " - " + actuator.highStateDuration.ToString("F0"), style);

                    }
                    else
                    {
                        Handles.Label(actuator.transform.position, actuator.actuatorValue.ToString("F2"), style);

                    }
                }
            }
        }
    }



    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        serializedObject.Update();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "arrayOfActuators")
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Actuators GameObject", EditorStyles.boldLabel);
                if (GUILayout.Button("Update Actuators List"))
                {
                    ((ActuatorsManager)target).SetAllChildObjectsAndActuator();
                }
            }

            if (iterator.name == "tangibleObjects")
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tangible Objects", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("This section allows you to identify objects with the 'Tangible Object' component in the scene. While the script does store these objects in a list for reference, it primarily uses a separate set that dynamically tracks objects entering and exiting the defined area for real-time actuator updates.", MessageType.Info);
                if (GUILayout.Button("Find Tangible Objects"))
                {
                    ((ActuatorsManager)target).FindAllTangibleObjectsInTheScene();
                }
            }

            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
