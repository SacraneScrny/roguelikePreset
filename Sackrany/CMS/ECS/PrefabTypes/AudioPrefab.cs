using Sackrany.CMS.CMSComponents;

using UnityEngine;

namespace Sackrany.CMS.ECS.PrefabTypes
{
    public class AudioPrefab : ScriptableObject, IPrefabECSTypeBase
    {
        public float LifeTime = 1;
        public float Volume = 1;
            
        public string DisplayName => "Audio";
    }
}