using System;

namespace Sackrany.Unit.ModuleSystem.Main
{
    public interface ITemplate<out TModule>
        where TModule : ModuleBase
    {
        public Type GetModuleType();
        public TModule GetModuleInstance();
    }
        
    [Serializable] 
    public abstract class TemplateBase<TModule> : ITemplate<TModule>
        where TModule : ModuleBase
    {
        public virtual Type GetModuleType() => typeof(TModule);
        public virtual TModule GetModuleInstance() => null;
    }
}