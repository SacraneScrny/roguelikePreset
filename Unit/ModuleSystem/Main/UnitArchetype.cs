using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sackrany.Hash;
using Sackrany.Unit.Abstracts;

using UnityEngine;

namespace Sackrany.Unit.ModuleSystem.Main
{
    public readonly struct UnitArchetype : IEquatable<UnitArchetype>
    {
        public readonly uint Hash;

        public UnitArchetype(UnitBase unit)
        {
            Hash = HashBuilder.BuildFromTemplates(CollectTemplates(unit));
        }
        static List<object> CollectTemplates(UnitBase unit)
        {
            var list = new List<object>(16);

            foreach (var controller in unit.GetControllers())
            {
                var type = controller.GetType();

                var field = type.GetField(
                    "Default",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (field == null) continue;

                if (field.GetValue(controller) is IEnumerable templates)
                {
                    foreach (var t in templates)
                        if (t != null)
                            list.Add(t);
                }
            }

            return list;
        }
        
        public bool Equals(UnitArchetype other)
        {
            return Hash == other.Hash;
        }
        public override bool Equals(object obj)
            => obj is UnitArchetype other && Equals(other);

        public override int GetHashCode()
            => unchecked((int)Hash);

        public static bool operator ==(UnitArchetype left, UnitArchetype right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(UnitArchetype left, UnitArchetype right)
            => !(left == right);
    }
}