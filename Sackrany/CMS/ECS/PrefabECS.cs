using Unity.Entities;

namespace Sackrany.CMS.ECS
{
    public static class PrefabECS
    {
        public interface IECSPrefabElement { public uint GetHash { get; } public Entity GetEntity { get; } }
    }
}