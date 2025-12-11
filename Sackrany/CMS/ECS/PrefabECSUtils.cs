using Unity.Entities;

namespace Sackrany.CMS.ECS
{
    public static class PrefabECSUtils
    {
        public static bool GetPrefab<T>(this DynamicBuffer<T> buffer, uint hashKey, out T prefab) where T : unmanaged, IBufferElementData, PrefabECS.IECSPrefabElement
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].GetHash == hashKey)
                {
                    prefab = buffer[i];
                    return true;
                }
            }
            prefab = default;
            return false;
        }
        public static T GetPrefab<T>(this DynamicBuffer<T> buffer, uint hashKey) where T : unmanaged, IBufferElementData, PrefabECS.IECSPrefabElement
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].GetHash == hashKey)
                {
                    return buffer[i];
                }
            }
            return default;
        }
    }
}