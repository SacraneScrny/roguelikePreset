using UnityEngine;

namespace Sackrany.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 Mul(this Vector3 vector, float x, float y, float z)
            => new Vector3(vector.x * x, vector.y * y, vector.z * z);
    }
}