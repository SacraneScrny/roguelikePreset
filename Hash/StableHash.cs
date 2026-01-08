using System;

namespace Sackrany.Hash
{
    public static class StableHash
    {
        public static uint Begin() => 2166136261u;

        public static uint Add(uint hash, uint data)
            => (hash ^ data) * 16777619u;

        public static uint Add(uint hash, int data)
            => Add(hash, unchecked((uint)data));

        public static uint Add(uint hash, float data)
            => Add(hash, BitConverter.ToUInt32(
                BitConverter.GetBytes(data), 0));

        public static uint Add(uint hash, bool data)
            => Add(hash, data ? 1u : 0u);

        public static uint Add(uint hash, string data)
        {
            if (data == null) return Add(hash, 0u);
            for (int i = 0; i < data.Length; i++)
                hash = Add(hash, data[i]);
            return hash;
        }

        public static uint Add(uint hash, Type type)
            => Add(hash, type.FullName);
    }

}