using System;
using System.Collections.Generic;

namespace Sackrany.Unit.Data
{
    public class UnitEvent : AUnitData
    {
        private protected readonly Dictionary<string, Dictionary<Type, List<Delegate>>> events = new ();

        public void Subscribe<T>(string eventName, Action<T> callback)
        {
            if (!events.TryGetValue(eventName, out var typed))
            {
                typed = new ();
                events.Add(eventName, typed);
            }

            if (!typed.TryGetValue(typeof(T), out var list))
            {
                list = new();
                typed.Add(typeof(T), list);
            }
            
            list.Add(callback);
        }
        public bool Unsubscribe<T>(string eventName, Action<T> callback)
        {
            if (!events.TryGetValue(eventName, out var typed)) return false;
            if (!typed.TryGetValue(typeof(T), out var list)) return false;
            list.Remove(callback);
            if (list.Count == 0)
                typed.Remove(typeof(T));
            return true;
        }
        public bool Publish<T>(string eventName, T data)
        {
            if (!events.TryGetValue(eventName, out var typed)) return false;
            if (!typed.TryGetValue(typeof(T), out var list)) return false;
            
            foreach (var callback in list)
                ((Action<T>)callback)(data);
            
            return true;
        }

        public override void Reset()
        {
            events.Clear();
        }
        //weak references?
    }
}