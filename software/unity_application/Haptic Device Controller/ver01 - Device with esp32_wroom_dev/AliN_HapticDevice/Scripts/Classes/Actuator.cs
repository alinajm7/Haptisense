using System;
using UnityEngine;

namespace AliN.Microcontroller.Classes
{
    [Serializable]
    public class Actuator
    {
        public string name; // to have correct name in the inspector array list
        public GameObject actuatorObject;
        public Transform transform { get; }
        public Material material { get; }
        public Color originalColor { get; }
        public Color distanceColor;
        public float actuatorValue = 0;
        public Vector3 targetObjectPoint;


        public Actuator(GameObject actuatorGameObject)
        {
            actuatorObject = actuatorGameObject;
            name = actuatorObject.name;
            transform = actuatorObject.transform;
            // Check if there is a material; if not, assign a new one.
            Renderer renderer = actuatorObject.GetComponent<Renderer>();
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }

            this.material = renderer.sharedMaterial; // Now this should be safe
            originalColor = this.material.color;

        }
        public Actuator()
        {
            this.actuatorValue = 0;
            // Initialize other fields if they exist
        }
    }
}
