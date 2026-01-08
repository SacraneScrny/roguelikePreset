using System;
using System.Collections.Generic;

namespace Sackrany.Unit.Data
{
    public class UnitEvent : AUnitData
    {
        private protected readonly Dictionary<string, Dictionary<Type, List<Delegate>>> events = new ();
        private protected readonly Dictionary<string, List<Action>> simpleEvents = new ();

        public void Subscribe(string eventName, Action callback)
        {
            if (!simpleEvents.TryGetValue(eventName, out var list))
            {
                list = new();
                simpleEvents.Add(eventName, list);
            }
            
            list.Add(callback);
        }
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
        
        public bool Unsubscribe<T>(string eventName, Action callback)
        {
            if (!simpleEvents.TryGetValue(eventName, out var list)) return false;
            list.Remove(callback);
            if (list.Count == 0)
                simpleEvents.Remove(eventName);
            return true;
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
        
        public bool Publish(string eventName)
        {
            if (!simpleEvents.TryGetValue(eventName, out var list)) return false;
            
            foreach (var callback in list)
                callback.Invoke();
            
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
            simpleEvents.Clear();
        }
    }
}