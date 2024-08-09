using System;
using UnityEngine;

namespace AliN.Microcontroller.Classes
{
    [Serializable]
    public class TaggedObject
    {
        public string name;
        public GameObject taggedObject;
        public Transform transform { get; }
        public Collider collider { get; }
        public bool IsMeshCollider { get; }

        public TaggedObject(GameObject taggedGameObject)
        {
            taggedObject = taggedGameObject;
            name = taggedObject.name;
            transform = taggedObject.transform;
            collider = taggedObject.GetComponent<Collider>();
            // If not found, try to find it in the children
            if (collider == null)
            {
                collider = taggedObject.GetComponentInChildren<Collider>();
            }
            IsMeshCollider = collider is MeshCollider;
        }
    }
}
