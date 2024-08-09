using UnityEngine;

namespace AliN
{

    public class ObjectInstantiationOn3DSurface : MonoBehaviour
    {
        public GameObject objectToInstantiate; // The object to be instantiated.
        public GameObject targetObject; // The object whose mesh will be used.
        public bool alignWithNormals = false; // Whether to align with normals.
        public int density = 1; // Density of instantiated objects (e.g., 1 object per vertex).
        public bool deleteExistingChildren = false; // Option to delete existing children of arrangedObjects.

        public void InstantiateObjectsIn3DShape()
        {
            // Delete all GameObjects with the name "arrangedObjects" if the option is enabled.
            if (deleteExistingChildren)
            {
                //bool allObjectsFound = false;
                //while (!allObjectsFound)
                //{
                //    // Find the "arrangedObjects" GameObject among children.
                //    GameObject arrangedObjectsParent = GameObject.Find("arrangedObjects");
                //    if (arrangedObjectsParent != null)
                //    {
                //        if (arrangedObjectsParent.transform.IsChildOf(transform))
                //        {
                //            DestroyImmediate(arrangedObjectsParent);
                //        }
                //    }
                //    else { allObjectsFound = true; }

                //}


                    // Find all child objects and destroy those with the name "arrangedObjects."
                    GameObject[] allChildren = new GameObject[transform.childCount];

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        allChildren[i] = transform.GetChild(i).gameObject;
                    }

                    foreach (GameObject child in allChildren)
                    {
                        if (child.name == "arrangedObjects")
                        {
                            DestroyImmediate(child);
                        }
                    }
                


            }


            if (objectToInstantiate == null || targetObject == null)
            {
                Debug.LogError("Please assign both objectToInstantiate and targetObject in the Inspector.");
                return;
            }

            Mesh targetMesh = targetObject.GetComponent<MeshFilter>().sharedMesh;

            if (targetMesh == null)
            {
                Debug.LogError("The targetObject must have a MeshFilter component with a valid mesh.");
                return;
            }

            // Create a parent GameObject for the instantiated objects.
            GameObject arrangedObjects = new GameObject("arrangedObjects");
            arrangedObjects.transform.parent = transform; // Make it a child of the ObjectArranger.



            // Loop through the vertices of the target mesh and instantiate objects.
            for (int i = 0; i < targetMesh.vertices.Length; i += density)
            {
                Vector3 vertexPosition = targetObject.transform.TransformPoint(targetMesh.vertices[i]);
                Quaternion rotation = alignWithNormals ? Quaternion.FromToRotation(Vector3.up, targetObject.transform.TransformDirection(targetMesh.normals[i])) : Quaternion.identity;
                GameObject instantiatedObject = Instantiate(objectToInstantiate, vertexPosition, rotation, arrangedObjects.transform);
            }
        }
    }
}
