using System;

namespace Sackrany.Unit.ModuleSystem.Main
{
    public interface ITemplate<out TModule>
        where TModule : ModuleBase, new()
    {
        public Type GetModuleType();
        public TModule GetModuleInstance();
    }
        
    public abstract class TemplateBase<TModule> : ITemplate<TModule>
        where TModule : ModuleBase, new()
    {
        public virtual Type GetModuleType() => typeof(TModule);
        public virtual TModule GetModuleInstance() => new ();
    }
}