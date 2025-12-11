using System;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem;
using Sackrany.Unit.ModuleSystem.Main;

namespace Sackrany.Unit
{
    public static class UnitMaybe
    {
        public static bool TryExecute<TController, TModule>(
            this UnitBase unit,
            Action<TModule> command)
            where TController : class, IModuleConnector<TModule>, IBaseModuleController
            where TModule : ModuleBase, new()
        {
            if (unit.TryGet<TController>(out var controller))
            {
                if (controller.TryGet(out TModule module))
                {
                    command(module);
                    return true;
                }
            }
            return false;
        }
        
        public static bool TryGetModule<TController, TModule>(
            this UnitBase unit,
            out TModule result)
            where TController : class, IModuleConnector<TModule>, IBaseModuleController
            where TModule : ModuleBase, new()
        {
            if (unit.TryGet<TController>(out var controller))
            {
                if (controller.TryGet(out TModule module))
                {
                    result = module;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}