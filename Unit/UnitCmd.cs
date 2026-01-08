using System;
using System.Collections;
using System.Collections.Generic;

using Sackrany.Unit.Abstracts;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Unit
{
    public class UnitCmd : AManager<UnitCmd>
    {
        private void Start()
        {
            StartCoroutine(CommandCoroutine());
        }
        
        private readonly List<UnitCommand> _commandHandlers = new();
        public class UnitCommand
        {
            public Func<UnitBase, bool> cond;
            public Action<UnitBase> action;
            public readonly List<Action> callbacks = new();
            public bool completed;
        }
        public class CommandHandle
        {
            private readonly UnitCommand _cmd;
            public CommandHandle(UnitCommand cmd) => _cmd = cmd;

            public CommandHandle OnComplete(Action callback)
            {
                if (_cmd.completed) callback?.Invoke();
                else _cmd.callbacks.Add(callback);
                return this;
            }
        }
        private IEnumerator CommandCoroutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                for (int i = _commandHandlers.Count - 1; i >= 0; i--)
                {
                    if (!UnitManager.TryGetUnit(_commandHandlers[i].cond, out var unit)) continue;
                    if (!unit.IsCompleteInitialized) continue;
                    _commandHandlers[i].action(unit);
                    foreach (var callback in _commandHandlers[i].callbacks)
                    {
                        callback?.Invoke();
                    }
                    _commandHandlers[i].callbacks.Clear();
                    _commandHandlers[i].completed = true;
                    _commandHandlers.RemoveAt(i);
                }
            }
        }
        
        public static CommandHandle Execute(Func<UnitBase, bool> cond, Action<UnitBase> action)
        {
            var cmd = new UnitCommand()
            {
                cond = cond,
                action = action
            };
            Instance._commandHandlers.Add(cmd);
            return new CommandHandle(cmd);
        }
    }
}