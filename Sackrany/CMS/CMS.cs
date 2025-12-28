using System.Collections.Generic;
using System.Linq;

using Logic.CMS.ResourceCatalog;

using Sackrany.CMS.ResourceCatalog.ScriptableObjects;

using UnityEngine;

namespace Sackrany.CMS
{
    [CreateAssetMenu(menuName = "Create CMS", fileName = "CMS", order = 0)]
    public class CMS : ScriptableObject
    {
        public CatalogObject Catalog;
        public List<CMSEntry> Entries;
        
        private Dictionary<uint, GameObject> _hashedObjects;
        
        private void OnEnable()
        {
            _hashedObjects = null;
        }
        public void UpdateDictionary(bool forceUpdate = false)
        {
            if (!forceUpdate && _hashedObjects != null) return;
            _hashedObjects = new Dictionary<uint, GameObject>();
            foreach (var a in Entries.SelectMany(x => x.Objects))
                _hashedObjects.Add(a.HashKey, a.Object);
        }
        
        public GameObject Get(HashCMS hash)
        {
            UpdateDictionary();
            return _hashedObjects[(uint)hash];
        }
        public GameObject GetInstance(HashCMS hash, Transform parent = null)
        {
            UpdateDictionary();
            if (!_hashedObjects.TryGetValue((uint)hash, out var obj)) return null;
            var g = Instantiate(obj, parent);
            return g;
        }
    }

    [System.Serializable]
    public class CMSEntry
    {
        public CatalogType Type;
        public List<CMSEntryObject> Objects;
    }
    
    [System.Serializable]
    public class CMSEntryObject
    {
        public uint HashKey;
        public GameObject Object;
        public ScriptableObject PrefabType;
    }
}