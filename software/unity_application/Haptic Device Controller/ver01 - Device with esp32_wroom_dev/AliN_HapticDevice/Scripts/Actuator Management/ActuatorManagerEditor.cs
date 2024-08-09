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
                    if (actuator.actuatorValue == 0)
                    {
                        style.normal.textColor = new Color(0, 0, 0, 0.15f); // Black with 0.15 alpha
                    }
                    else
                    {
                        // Complementary color
                        Color complementaryColor = new Color(1 - actuator.material.color.r, 1 - actuator.material.color.g, 1 - actuator.material.color.b, 1);
                        style.normal.textColor = complementaryColor;
                    }

                    Handles.Label(actuator.transform.position, actuator.actuatorValue.ToString("F2"), style);
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

            if (iterator.name == "haptableObjects")
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Haptable Objects", EditorStyles.boldLabel);
                if (GUILayout.Button("Update Haptable Objects List"))
                {
                    ((ActuatorsManager)target).FindAllHaptableObjectsInTheScene();
                }
            }

            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
