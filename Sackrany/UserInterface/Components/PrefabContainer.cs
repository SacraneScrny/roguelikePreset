using System;
using System.Collections.Generic;

using Sackrany.Hash;
using Sackrany.UserInterface.Entities;

using UnityEngine;

namespace Sackrany.UserInterface.Components
{
    public class PrefabContainer : MonoBehaviour
    {
        public PrefabElement[] PrefabElements;
        
        private Dictionary<uint, int> _prefabElements = new Dictionary<uint, int>();

        private void Awake()
        {
            for (int i = 0; i < PrefabElements.Length; i++)
                _prefabElements.Add(PrefabElements[i].Key.XXHash(), i);
        }

        public GameObject GetActualGameObject(string key, Transform parent)
        {
            if (_prefabElements.TryGetValue(key.XXHash(), out int index))
                return Instantiate(PrefabElements[index].Prefab, parent) as GameObject;
            return null;
        }
        
        private void OnValidate()
        {
            foreach (var element in PrefabElements)
                if (string.IsNullOrEmpty(element.Key)) element.Key = element.Prefab.name;
        }
    }
}