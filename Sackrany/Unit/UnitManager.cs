using System;
using System.Collections.Generic;
using System.Linq;

using Sackrany.ExpandedVariable.Entities;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Unit
{
    public class UnitManager : AManager<UnitManager>
    {
        public ExpandedFloat UnitsTimeFlow = new ExpandedFloat(1f);
        
        private readonly Dictionary<long, UnitBase> _cachedUnits = new ();
        private readonly List<UnitBase> _cachedArray = new();

        public static bool RegisterUnit(UnitBase unit)
        {
            if (Instance._cachedUnits.ContainsKey(unit.GetId())) return false;
            Instance._cachedUnits.Add(unit.GetId(), unit);
            Instance._cachedArray.Add(unit);
            Instance.OnUnitRegistered?.Invoke(unit);
            unit.OnUnitInitialized += Instance.HandleUnitInitialized;
            return true;
        }
        public static bool UnregisterUnit(UnitBase unit)
        {
            if (!Instance._cachedUnits.ContainsKey(unit.GetId())) return false;
            Instance._cachedUnits.Remove(unit.GetId());
            Instance._cachedArray.Remove(unit);
            unit.OnUnitInitialized -= Instance.HandleUnitInitialized;
            Instance.OnUnitUnregistered?.Invoke(unit);
            return true;
        }
        
        public static IReadOnlyList<UnitBase> GetAllUnits() => Instance._cachedArray;
        public static bool HasUnits(Func<UnitBase, bool> cond)
        {
            foreach (var unit in Instance._cachedArray)
                if (cond(unit)) return true;
            return false;
        }
        public static IEnumerable<UnitBase> GetAllUnits(Func<UnitBase, bool> cond) =>
            Instance._cachedArray.Where(cond);
        
        public event System.Action<UnitBase> OnUnitRegistered;
        public event System.Action<UnitBase> OnUnitUnregistered;
        
        public event System.Action<UnitBase> OnUnitInitialized;
        private void HandleUnitInitialized(UnitBase unit)
        {
            OnUnitInitialized?.Invoke(unit);
        }
    }
}