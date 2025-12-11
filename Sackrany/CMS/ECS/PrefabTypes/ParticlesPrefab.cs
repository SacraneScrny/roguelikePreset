using Sackrany.CMS.CMSComponents;

using UnityEngine;

namespace Sackrany.CMS.ECS.PrefabTypes
{
    public class ParticlesPrefab : ScriptableObject, IPrefabECSTypeBase
    {
        public float LifeTime = 1;
        public float Scale = 1;
            
        public string DisplayName => "Particles";
    }
}