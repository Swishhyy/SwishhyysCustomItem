// Add this to your project in a new file
using Exiled.API.Features;
using System;
using UnityEngine;

namespace SCI.Utils
{
    public class PrimitiveManager
    {
        // Creates a primitive object and returns a reference to it
        public static GameObject CreatePrimitive(PrimitiveType type, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            try
            {
                GameObject primitive = GameObject.CreatePrimitive(type);

                // Set transform properties
                primitive.transform.position = position;
                primitive.transform.rotation = rotation;
                primitive.transform.localScale = scale;

                // Set material color
                Renderer renderer = primitive.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"))
                    {
                        color = color
                    };
                }

                // Remove collider to avoid physics interactions
                if (primitive.TryGetComponent(out Collider collider))
                    GameObject.Destroy(collider);

                // Keep object persistent
                GameObject.DontDestroyOnLoad(primitive);

                return primitive;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create primitive: {ex.Message}");
                return null;
            }
        }

        // Destroys a primitive object
        public static void DestroyPrimitive(GameObject primitive)
        {
            if (primitive != null)
                GameObject.Destroy(primitive);
        }

        // Creates a beam primitive between two points
        public static GameObject CreateBeam(Vector3 startPoint, Vector3 endPoint, float width, Color color)
        {
            // Calculate parameters for the beam
            Vector3 direction = (endPoint - startPoint).normalized;
            float length = Vector3.Distance(startPoint, endPoint);
            Vector3 midPoint = (startPoint + endPoint) / 2f;

            // Create a cube primitive as our beam
            return CreatePrimitive(
                PrimitiveType.Cube,
                midPoint,
                Quaternion.LookRotation(direction),
                new Vector3(width, width, length),
                color
            );
        }
    }
}
