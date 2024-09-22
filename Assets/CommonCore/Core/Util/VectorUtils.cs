using System;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Utility functions for manipulating vectors.
    /// </summary>
    public static class VectorUtils
    {
        /// <summary>
        /// Gets a flat 2D (x->x, z->y) vector from a 3D vector
        /// </summary>
        public static Vector2 GetFlatVector(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }

        /// <summary>
        /// Gets a flat sideways 2D (z->x, y->y) vector from a 3D vector
        /// </summary>
        public static Vector2 GetSideVector(this Vector3 vec3)
        {
            return new Vector2(vec3.z, vec3.y);
        }

        /// <summary>
        /// Gets a space 3D (x->x, 0->y, y->z) vector from a flat 2D vector
        /// </summary>
        public static Vector3 GetSpaceVector(this Vector2 vec2)
        {
            return new Vector3(vec2.x, 0, vec2.y);
        }

        /// <summary>
        /// Gets a 3D vector (x->x, y->y) from a 2D vector, manually specifying the Z component
        /// </summary>
        public static Vector3 GetAtZ(this Vector2 vec2, float z)
        {
            return new Vector3(vec2.x, vec2.y, z);
        }

        /// <summary>
        /// Swaps the X and Z components of a vector.
        /// </summary>
        /// <remarks>
        /// Modifies the original vector
        /// </remarks>
        public static Vector3 SwapYZ(this Vector3 vec3)
        {
            float oldZ = vec3.z;
            vec3.z = vec3.y;
            vec3.y = oldZ;
            return vec3;
        }

        /// <summary>
        /// Gets the flat vector from position to target
        /// </summary>
        public static Vector3 GetFlatVectorToTarget(Vector3 pos, Vector3 target)
        {
            Vector3 dir = target - pos;
            return new Vector3(dir.x, 0, dir.z);
        }

        /// <summary>
        /// Gets a random Vector2 given a center and extents
        /// </summary>
        public static Vector2 GetRandomVector2(Vector2 center, Vector2 extents)
        {
            return new Vector2(
                UnityEngine.Random.Range(-extents.x, extents.x) + center.x,
                UnityEngine.Random.Range(-extents.y, extents.y) + center.y
                );
        }

        /// <summary>
        /// Scales a vector by another vector and returns the result
        /// </summary>
        public static Vector3 ScaleBy(this Vector3 vector0, Vector3 vector1)
        {
            return new Vector3(vector0.x * vector1.x, vector0.y * vector1.y, vector0.z * vector1.z);
        }

    }
}