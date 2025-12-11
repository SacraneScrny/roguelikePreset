using UnityEngine;

namespace Sackrany.Pool.Abstracts
{
    public interface IPoolable
    {
        public GameObject gameObject { get; }
        public void OnPooled();
        public void OnReleased();
    }
}