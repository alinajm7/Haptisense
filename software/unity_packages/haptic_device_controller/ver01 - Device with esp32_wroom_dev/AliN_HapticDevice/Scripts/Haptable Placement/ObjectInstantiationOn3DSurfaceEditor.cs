using UnityEngine;
using UnityEditor;

namespace AliN
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectInstantiationOn3DSurface))]
    public class ObjectInstantiationOn3DSurfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ObjectInstantiationOn3DSurface arranger = (ObjectInstantiationOn3DSurface)target;

            if (GUILayout.Button("Instantiate On Each Face"))
            {
                arranger.InstantiateObjectsIn3DShape();
            }
        }
    }
#endif
}
