using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;
using Sackrany.Unit.ModuleSystem.Main;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Unit
{
    public class UnitManager : AManager<UnitManager>
    {
        public ExpandedFloat UnitsTimeFlow = new ExpandedFloat(1f);
        
        private readonly Dictionary<long, UnitBase> _cachedUnits = new ();
        private readonly Dictionary<UnitArchetype, Dictionary<long, UnitBase>> _cachedArchetypes = new ();
        private readonly List<UnitBase> _cachedArray = new();
        
        public static bool RegisterUnit(UnitBase unit)
        {
            if (Instance._cachedUnits.ContainsKey(unit.GetId())) return false;

            if (!Instance._cachedArchetypes.TryGetValue(unit.GetArchetype(), out var archetypes))
            {
                archetypes = new ();
                Instance._cachedArchetypes.Add(unit.GetArchetype(), archetypes);
            }
            archetypes.TryAdd(unit.GetId(), unit);
            
            Instance._cachedUnits.Add(unit.GetId(), unit);
            Instance._cachedArray.Add(unit);
            Instance.OnUnitRegistered?.Invoke(unit);
            unit.OnUnitInitialized += Instance.HandleUnitInitialized;
            return true;
        }
        public static bool UnregisterUnit(UnitBase unit)
        {
            if (!Instance._cachedUnits.ContainsKey(unit.GetId())) return false;
            
            if (Instance._cachedArchetypes.TryGetValue(unit.GetArchetype(), out var archetypes))
                archetypes.Remove(unit.GetId());
            
            Instance._cachedUnits.Remove(unit.GetId());
            Instance._cachedArray.Remove(unit);
            unit.OnUnitInitialized -= Instance.HandleUnitInitialized;
            Instance.OnUnitUnregistered?.Invoke(unit);
            return true;
        }
        
        public static bool HasUnits(Func<UnitBase, bool> cond)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit)) return true;
            return false;
        }
        public static UnitBase GetUnit(Func<UnitBase, bool> cond)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit)) return unit;
            return null;
        }
        public static bool TryGetUnit(Func<UnitBase, bool> cond, out UnitBase value)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit))
                {
                    value = unit;
                    return true;
                }
            value = null;
            return false;
        }
        
        public static IReadOnlyList<UnitBase> GetAllUnits() => Instance._cachedArray;
        public static IEnumerable<UnitBase> GetAllUnits(Func<UnitBase, bool> cond) =>
            Instance._cachedArray.Where(cond);
        public static IEnumerable<UnitBase> GetAllUnits(UnitArchetype archetype) 
            => Instance._cachedArchetypes.TryGetValue(archetype, out var archetypes) 
                ? archetypes.Select(x => x.Value) 
                : Array.Empty<UnitBase>();
        
        public event System.Action<UnitBase> OnUnitRegistered;
        public event System.Action<UnitBase> OnUnitUnregistered;
        
        public event System.Action<UnitBase> OnUnitInitialized;
        private void HandleUnitInitialized(UnitBase unit)
        {
            OnUnitInitialized?.Invoke(unit);
        }
    }
}