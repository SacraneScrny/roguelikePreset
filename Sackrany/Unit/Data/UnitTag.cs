using System.Collections.Generic;
using System.Linq;

namespace Sackrany.Unit.Data
{
    [System.Serializable]
    public class UnitTag : AUnitData
    {
        public List<string> Tags = new List<string>();
        
        private Dictionary<string, uint> _hash = new ();

        public UnitTag()
        {
            _hash = Tags.ToDictionary(x => x, x => x.XXHash());
        }
        
        public bool HasTag(string tag) => _hash.ContainsKey(tag);
        public bool HasAllTags(params string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
                if (!HasTag(tags[i]))
                    return false;
            return true;
        }
        public bool HasAnyTag(params string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
                if (HasTag(tags[i]))
                    return true;
            return false;
        }
        
        public bool AddTag(string tag) => _hash.TryAdd(tag, tag.XXHash());
        public bool AddTags(params string[] tags)
        {
            bool allAdded = true;
            for (int i = 0; i < tags.Length; i++)
                allAdded &= AddTag(tags[i]);
            return allAdded;
        }
        
        public bool RemoveTag(string tag) => _hash.Remove(tag);
        public bool RemoveTags(params string[] tags)
        {
            bool allRemoved = true;
            for (int i = 0; i < tags.Length; i++)
                allRemoved &= RemoveTag(tags[i]);
            return allRemoved;
        }
        
        public override void Reset()
        {
            _hash = Tags.ToDictionary(x => x, x => x.XXHash());
        }
    }
}