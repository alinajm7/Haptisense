using UnityEngine;
using UnityEditor;

namespace AliN.Microcontroller
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ActuatorLayout))]
    public class ActuatorLayoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ActuatorLayout myScript = (ActuatorLayout)target;
            if (GUILayout.Button("Update chind objects"))
            {
                myScript.SetAllChildObjectsAndActuator();
            }

            if (GUILayout.Button("Arrange Layout"))
            {
                myScript.ArrangeInLayout();
            }
        }
    }
#endif
}
