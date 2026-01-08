using System;
using System.Collections;
using System.Linq;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Main;

namespace Sackrany.Unit
{
    public static class UnitMaybe
    {
        public static bool HasModule<TModule>(this UnitBase unit)
            where TModule : ModuleBase
        {
            if (unit == null)
            {
                return false;
            }
            if (!unit.IsInitialized)
            {
                return false;
            }
            
            var controllers = unit.GetControllers().ToArray();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].HasModule<TModule>())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TryGetModule<TModule>(this UnitBase unit, out TModule module)
            where TModule : ModuleBase
        {
            if (unit == null)
            {
                module = null;
                return false;
            }
            if (!unit.IsInitialized)
            {
                module = null;
                return false;
            }
            
            var controllers = unit.GetControllers().ToArray();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].TryGetModule(out TModule m))
                {
                    module = m;
                    return true;
                }
            }
            module = null;
            return false;
        }
        public static bool TryGetModule(this UnitBase unit, Type type, out ModuleBase module)
        {
            if (unit == null)
            {
                module = null;
                return false;
            }
            if (!unit.IsInitialized)
            {
                module = null;
                return false;
            }
            
            var controllers = unit.GetControllers().ToArray();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].TryGetModule(type, out var m))
                {
                    module = m;
                    return true;
                }
            }
            module = null;
            return false;
        }
        public static bool Maybe<TModule>(this UnitBase unit, Action<TModule> action)
            where TModule : ModuleBase
        {
            if (unit == null) return false;
            if (!unit.IsInitialized) return false;
            
            var controllers = unit.GetControllers().ToArray();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].TryGetModule(out TModule module))
                {
                    action(module);
                    return true;
                }
            }
            return false;
        }
        public static void Command<TModule>(this UnitBase unit, Action<TModule> action)
            where TModule : ModuleBase
        {
            if (unit == null) return;
            if (!unit.IsInitialized || !Maybe<TModule>(unit, action))
            {
                unit.StartCoroutine(UnitModuleCommand(unit, action));
                return;
            }
        }
        private static IEnumerator UnitModuleCommand<TModule>(UnitBase unit, Action<TModule> action)
            where TModule : ModuleBase
        {
            while (!unit.IsInitialized) yield return null;
            
            Maybe<TModule>(unit, action);
        }
        
        public static bool MaybeController<TController>(this UnitBase unit, Action<TController> action)
            where TController : IBaseModuleController
        {
            if (unit == null) return false;

            if (unit.TryGet<TController>(out var controller))
            {
                if (controller.IsModulesInitialized()) 
                {
                    action(controller);
                    return true;
                }
            }
            return false;
        }
        public static void ControllerCommand<TController>(this UnitBase unit, Action<TController> action)
            where TController : IBaseModuleController
        {
            if (unit == null) return;
            if (!unit.IsInitialized || !MaybeController<TController>(unit, action))
            {
                unit.StartCoroutine(UnitControllerCommand(unit, action));
                return;
            }
        }        
        private static IEnumerator UnitControllerCommand<TController>(UnitBase unit, Action<TController> action)
            where TController : IBaseModuleController
        {
            while (!unit.IsInitialized) yield return null;

            MaybeController<TController>(unit, action);
        }
        
        public static bool Maybe(this UnitBase unit, Action<UnitBase> action)
        {
            if (unit != null && unit.IsInitialized)
            {
                action(unit);
                return true;
            }
            return false;
        }
        public static void Command(this UnitBase unit, Action<UnitBase> action)
        {
            if (unit == null) return;
            if (!unit.IsInitialized)
            {
                unit.StartCoroutine(UnitCommand(unit, action));
                return;
            }
        }
        private static IEnumerator UnitCommand(UnitBase unit, Action<UnitBase> action)
        {
            while (!unit.IsInitialized) yield return null;
            Maybe(unit, action);
        }
    }
}